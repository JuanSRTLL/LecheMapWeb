using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using static LecheMap.AsesoramientoIA;

namespace LecheMap
{
    public class CohereApiClient
    {
        private readonly BingNewsService _newsService;
        private readonly HttpClient _httpClient;
        private readonly string _apiKey;
        private const string ApiUrl = "https://api.cohere.ai/v1/generate";
        private List<string> conversationHistory;
        private Dictionary<string, List<NewsArticle>> cachedNews;
        private readonly string systemPrompt = @"Eres un asistente virtual especializado en evaluar la viabilidad de inversión en el sector lechero de Colombia. 
Tu objetivo es proporcionar análisis concisos y recomendaciones basadas en datos oficiales de aptitud del suelo y noticias actuales del sector.

Directrices para respuestas:
1. Primera respuesta: 
   - Proporciona un análisis detallado completo basado en los datos de aptitud
   - Incluye evaluación de viabilidad y recomendaciones
   - Analiza las noticias proporcionadas y su impacto en el sector, incluyendo los enlaces a las fuentes
   - Concluye con una recomendación clara (Favorable/Desfavorable/Requiere más análisis)

2. Respuestas subsiguientes:
   - Sé conciso y directo
   - Responde específicamente a la pregunta planteada
   - Evita repetir información ya proporcionada
   - Relaciona tu respuesta con noticias relevantes cuando sea aplicable, incluyendo siempre los enlaces a las fuentes
   - Al mencionar una noticia, incluye el enlace usando el formato: [título de la noticia](URL)

Mantén un tono profesional pero conciso y asegúrate de justificar cada conclusión con datos cuando sea relevante.
Al analizar noticias, proporciona recomendaciones prácticas basadas en la actualidad del sector.
IMPORTANTE: Siempre que menciones una noticia, incluye su enlace usando el formato [título](URL).";

        public CohereApiClient()
        {
            _newsService = new BingNewsService();
            _httpClient = new HttpClient();
            _apiKey = ConfigurationManager.AppSettings["CohereApiKey"];
            _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {_apiKey}");
            conversationHistory = new List<string>();
            cachedNews = new Dictionary<string, List<NewsArticle>>();
            conversationHistory.Add(systemPrompt);
        }

        private async Task<string> FormatNewsData(string departamento)
        {
            var news = await _newsService.GetDairyNewsAsync(departamento);
            StringBuilder sb = new StringBuilder();

            if (news != null && news.Any())
            {
                
                cachedNews[departamento] = news;

                sb.AppendLine($"\nNoticias relevantes del sector lechero en {departamento}:\n");
                foreach (var article in news.Take(3))
                {
                    sb.AppendLine($"- [{article.Name}]({article.Url})");
                    sb.AppendLine($"  {article.Description}");
                    sb.AppendLine($"  Fecha: {article.DatePublished:dd/MM/yyyy}\n");
                }

                
                sb.AppendLine("\nRecuerda incluir los enlaces a las noticias usando el formato [título](URL) cuando las menciones en tu análisis.");
            }
            else
            {
                sb.AppendLine("\nNo se encontraron noticias recientes específicas para esta región.");
            }

            return sb.ToString();
        }

        private string ProcessResponse(string response, string departamento)
        {
            if (cachedNews.ContainsKey(departamento))
            {
                foreach (var article in cachedNews[departamento])
                {
                    
                    if (response.Contains(article.Name) && !response.Contains($"[{article.Name}]({article.Url})"))
                    {
                       
                        response = response.Replace(article.Name, $"[{article.Name}]({article.Url})");
                    }
                }
            }
            return response;
        }

        public async Task<CohereResponse> SendMessage(string message, string conversationId)
        {
            try
            {
                string departamento = string.Empty;
                string completeMessage = message;

               
                if (message.Contains("Resumen de Datos de Aptitud"))
                {
                    var match = System.Text.RegularExpressions.Regex.Match(message, @"en\s+([^,]+),\s+([^:]+):");
                    if (match.Success)
                    {
                        departamento = match.Groups[2].Value.Trim();
                        string newsData = await FormatNewsData(departamento);
                        completeMessage = message + "\n\n" + newsData;
                    }
                }

                conversationHistory.Add($"Usuario: {completeMessage}");
                var prompt = string.Join("\n", conversationHistory) + "\nAsistente:";

                var request = new CohereRequest
                {
                    Model = "command-r-plus-08-2024",
                    Prompt = prompt,
                    MaxTokens = 1000,
                    Temperature = 0.7,
                    K = 0,
                    StopSequences = new List<string> { "Usuario:" },
                    ReturnLikelihoods = "NONE"
                };

                var content = new StringContent(
                    JsonConvert.SerializeObject(request),
                    Encoding.UTF8,
                    "application/json"
                );

                var response = await _httpClient.PostAsync(ApiUrl, content);
                response.EnsureSuccessStatusCode();

                var responseString = await response.Content.ReadAsStringAsync();
                var cohereResponse = JsonConvert.DeserializeObject<CohereResponse>(responseString);

                if (cohereResponse?.Generations?.Count > 0)
                {
                    string aiResponse = cohereResponse.Generations[0].Text.Trim();

                    
                    if (!string.IsNullOrEmpty(departamento))
                    {
                        aiResponse = ProcessResponse(aiResponse, departamento);
                    }

                    conversationHistory.Add($"Asistente: {aiResponse}");
                    cohereResponse.Generations[0].Text = aiResponse;
                    return cohereResponse;
                }
                else
                {
                    throw new Exception("La respuesta de la API no contiene generaciones válidas.");
                }
            }
            catch (Exception ex)
            {
                return new CohereResponse
                {
                    Generations = new List<Generation>
                    {
                        new Generation { Text = $"Lo siento, ha ocurrido un error al procesar tu solicitud: {ex.Message}" }
                    }
                };
            }
        }

        public void ClearConversation()
        {
            conversationHistory.Clear();
            cachedNews.Clear();
            conversationHistory.Add(systemPrompt);
        }
    }
}
