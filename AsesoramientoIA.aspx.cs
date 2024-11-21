using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web.UI.WebControls;
using Newtonsoft.Json;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Web.UI;

namespace LecheMap
{
    public partial class AsesoramientoIA : System.Web.UI.Page
    {
        protected HiddenField hdnChartData;

        private CohereApiClient _cohereApiClient;
        private static List<AptitudData> aptitudDataList = new List<AptitudData>();

        protected async void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                if (Session["CohereApiClient"] == null)
                {
                    _cohereApiClient = new CohereApiClient();
                    Session["CohereApiClient"] = _cohereApiClient;
                }
                else
                {
                    _cohereApiClient = (CohereApiClient)Session["CohereApiClient"];
                }

                if (Session["ConversationId"] == null)
                {
                    Session["ConversationId"] = Guid.NewGuid().ToString();
                }

                await LoadAptitudDataAsync();
                LoadDepartamentos();
                await LoadTopDepartmentsDataAsync(); // Cargar los datos de departamentos
                await LoadTopMunicipiosDataAsync(); // Cargar los datos de los municipios
            }
            else
            {
                _cohereApiClient = (CohereApiClient)Session["CohereApiClient"];
                if (Session["AptitudDataList"] != null)
                {
                    aptitudDataList = (List<AptitudData>)Session["AptitudDataList"];
                }

                // Recrear el gráfico usando los datos del HiddenField
                if (!string.IsNullOrEmpty(hdnChartData.Value))
                {
                    ScriptManager.RegisterStartupScript(this, GetType(), "RecreateTopDepartmentsChart",
                        $"createTopDepartmentsChart({hdnChartData.Value});", true);
                }

                // Recrear el gráfico de Top 10 Municipios
                if (Session["TopMunicipiosData"] != null)
                {
                    string topMunicipiosJson = Session["TopMunicipiosData"].ToString();
                    ScriptManager.RegisterStartupScript(this, GetType(), "RecreateTopMunicipiosChart",
                        $"createTopMunicipiosChart({topMunicipiosJson});", true);
                }
            }
        }

        private async Task LoadTopDepartmentsDataAsync()
        {
            string apiUrl = "https://www.datos.gov.co/resource/bfuy-8yvf.json?$query=SELECT%20departamen,%20ROUND(SUM(area_ha)%20FILTER%20(WHERE%20aptitud%20=%20'Aptitud%20alta'),%200)%20AS%20aptitud_alta_ha%20WHERE%20departamen%20IS%20NOT%20NULL%20AND%20aptitud%20=%20'Aptitud%20alta'%20GROUP%20BY%20departamen%20HAVING%20SUM(area_ha)%20>%200%20ORDER%20BY%20aptitud_alta_ha%20DESC%20LIMIT%205";

            string appToken = ConfigurationManager.AppSettings["AppToken"];

            using (HttpClient client = new HttpClient())
            {
                client.DefaultRequestHeaders.Add("X-App-Token", appToken);
                HttpResponseMessage response = await client.GetAsync(apiUrl);

                if (response.IsSuccessStatusCode)
                {
                    string json = await response.Content.ReadAsStringAsync();
                    hdnChartData.Value = json; // Guardar los datos en el HiddenField
                    Session["TopDepartmentsData"] = json; // Guardar en sesión

                    // Llamar a la función de JavaScript para crear el gráfico
                    ScriptManager.RegisterStartupScript(this, GetType(), "CreateTopDepartmentsChart",
                        $"createTopDepartmentsChart({json});", true);
                }
                else
                {
                    string error = $"No se pudo obtener los datos de los departamentos. Código de estado: {response.StatusCode}";
                    ShowError(error);
                }
            }
        }

        private async Task LoadTopMunicipiosDataAsync()
        {
            string apiUrl = "https://www.datos.gov.co/resource/bfuy-8yvf.json?$query=SELECT municipio, ROUND(SUM(area_ha) FILTER (WHERE aptitud = 'Aptitud alta'), 0) AS aptitud_alta_ha WHERE municipio IS NOT NULL AND aptitud = 'Aptitud alta' GROUP BY municipio HAVING SUM(area_ha) > 0 ORDER BY aptitud_alta_ha DESC LIMIT 5";

            string appToken = ConfigurationManager.AppSettings["AppToken"];

            using (HttpClient client = new HttpClient())
            {
                client.DefaultRequestHeaders.Add("X-App-Token", appToken);
                HttpResponseMessage response = await client.GetAsync(apiUrl);

                if (response.IsSuccessStatusCode)
                {
                    string json = await response.Content.ReadAsStringAsync();
                    Session["TopMunicipiosData"] = json; // Guardar en sesión

                    // Llamar a la función de JavaScript para crear el gráfico
                    ScriptManager.RegisterStartupScript(this, GetType(), "CreateTopMunicipiosChart",
                        $"createTopMunicipiosChart({json});", true);
                }
                else
                {
                    string error = $"No se pudo obtener los datos de los municipios. Código de estado: {response.StatusCode}";
                    ShowError(error);
                }
            }
        }

        public class BingNewsService
        {
            private readonly HttpClient _httpClient;
            private readonly string _apiKey;

            public BingNewsService()
            {
                _httpClient = new HttpClient();
                _apiKey = ConfigurationManager.AppSettings["BingApiKey"]; // Cambiado aquí
                _httpClient.DefaultRequestHeaders.Add("Ocp-Apim-Subscription-Key", _apiKey);
            }

            public async Task<List<NewsArticle>> GetDairyNewsAsync(string departamento)
            {
                try
                {
                    string query = Uri.EscapeDataString($"noticias sector lechero producción leche {departamento} Colombia");
                    string url = $"https://api.bing.microsoft.com/v7.0/search?q={query}&mkt=es-CO&count=5";

                    var response = await _httpClient.GetAsync(url);
                    response.EnsureSuccessStatusCode();

                    var content = await response.Content.ReadAsStringAsync();
                    var newsResponse = JsonConvert.DeserializeObject<BingNewsResponse>(content);

                    return newsResponse?.WebPages?.Value ?? new List<NewsArticle>();
                }
                catch (Exception ex)
                {
                    // Log the error if you have logging configured
                    Console.WriteLine($"Error fetching news: {ex.Message}");
                    return new List<NewsArticle>();
                }
            }
        }

        public static class MessageFormatter
        {
            public static string FormatMessage(string message)
            {
                if (string.IsNullOrEmpty(message))
                    return message;

                // Paso 1: Reemplazar los enlaces markdown con enlaces HTML
                message = System.Text.RegularExpressions.Regex.Replace(
                    message,
                    @"\[([^\]]+)\]\(([^\)]+)\)",
                    "<a href=\"$2\" target=\"_blank\" class=\"news-link\">$1</a>"
                );

                // Paso 2: Reemplazar los marcadores de encabezado ##
                message = System.Text.RegularExpressions.Regex.Replace(
                    message,
                    @"##\s*([^#\n]+)",
                    "<h2 class='chat-heading'>$1</h2>"
                );

                // Paso 3: Reemplazar los marcadores de negrita **
                message = System.Text.RegularExpressions.Regex.Replace(
                    message,
                    @"\*\*([^*]+)\*\*",
                    "<strong>$1</strong>"
                );

                // Paso 4: Reemplazar los saltos de línea
                message = message.Replace("\n", "<br/>");

                return message;
            }
        }


        public class FormattedAptitudData
        {
            private string GetDefaultValue(string value, string defaultValue = "0")
            {
                return string.IsNullOrEmpty(value) ? defaultValue : value;
            }

            private string _departamen;
            public string departamen
            {
                get { return GetDefaultValue(_departamen); }
                set { _departamen = value; }
            }

            private string _municipio;
            public string municipio
            {
                get { return GetDefaultValue(_municipio); }
                set { _municipio = value; }
            }

            private string _total_area_ha;
            public string total_area_ha
            {
                get { return GetDefaultValue(_total_area_ha, "0"); }
                set { _total_area_ha = value; }
            }

            private string _aptitud_alta_ha;
            public string aptitud_alta_ha
            {
                get { return GetDefaultValue(_aptitud_alta_ha, "0"); }
                set { _aptitud_alta_ha = value; }
            }

            private string _aptitud_alta_pct;
            public string aptitud_alta_pct
            {
                get { return GetDefaultValue(_aptitud_alta_pct, "0.00"); }
                set { _aptitud_alta_pct = value; }
            }

            private string _aptitud_media_ha;
            public string aptitud_media_ha
            {
                get { return GetDefaultValue(_aptitud_media_ha, "0"); }
                set { _aptitud_media_ha = value; }
            }

            private string _aptitud_media_pct;
            public string aptitud_media_pct
            {
                get { return GetDefaultValue(_aptitud_media_pct, "0.00"); }
                set { _aptitud_media_pct = value; }
            }

            private string _aptitud_baja_ha;
            public string aptitud_baja_ha
            {
                get { return GetDefaultValue(_aptitud_baja_ha, "0"); }
                set { _aptitud_baja_ha = value; }
            }

            private string _aptitud_baja_pct;
            public string aptitud_baja_pct
            {
                get { return GetDefaultValue(_aptitud_baja_pct, "0.00"); }
                set { _aptitud_baja_pct = value; }
            }

            private string _exclusion_legal_ha;
            public string exclusion_legal_ha
            {
                get { return GetDefaultValue(_exclusion_legal_ha, "0"); }
                set { _exclusion_legal_ha = value; }
            }

            private string _exclusion_legal_pct;
            public string exclusion_legal_pct
            {
                get { return GetDefaultValue(_exclusion_legal_pct, "0.00"); }
                set { _exclusion_legal_pct = value; }
            }

            private string _no_apta_ha;
            public string no_apta_ha
            {
                get { return GetDefaultValue(_no_apta_ha, "0"); }
                set { _no_apta_ha = value; }
            }

            private string _no_apta_pct;
            public string no_apta_pct
            {
                get { return GetDefaultValue(_no_apta_pct, "0.00"); }
                set { _no_apta_pct = value; }
            }
        }

        private FormattedAptitudData FormatAptitudData(AptitudData rawData)
        {
            return new FormattedAptitudData
            {
                departamen = rawData.departamen,
                municipio = rawData.municipio,
                total_area_ha = rawData.total_area_ha,
                aptitud_alta_ha = rawData.aptitud_alta_ha,
                aptitud_alta_pct = rawData.aptitud_alta_pct,
                aptitud_media_ha = rawData.aptitud_media_ha,
                aptitud_media_pct = rawData.aptitud_media_pct,
                aptitud_baja_ha = rawData.aptitud_baja_ha,
                aptitud_baja_pct = rawData.aptitud_baja_pct,
                exclusion_legal_ha = rawData.exclusion_legal_ha,
                exclusion_legal_pct = rawData.exclusion_legal_pct,
                no_apta_ha = rawData.no_apta_ha,
                no_apta_pct = rawData.no_apta_pct
            };
        }

        private async Task LoadAptitudDataAsync()
        {
            string apiUrl = "https://www.datos.gov.co/resource/bfuy-8yvf.json?$query=SELECT%20departamen,%20municipio,%20ROUND(SUM(area_ha),%200)%20AS%20total_area_ha,%20ROUND(SUM(area_ha)%20FILTER%20(WHERE%20aptitud%20=%20'Aptitud%20alta'),%200)%20AS%20aptitud_alta_ha,%20ROUND((SUM(area_ha)%20FILTER%20(WHERE%20aptitud%20=%20'Aptitud%20alta')%20/%20SUM(area_ha))%20*%20100,%202)%20AS%20aptitud_alta_pct,%20ROUND(SUM(area_ha)%20FILTER%20(WHERE%20aptitud%20=%20'Aptitud%20media'),%200)%20AS%20aptitud_media_ha,%20ROUND((SUM(area_ha)%20FILTER%20(WHERE%20aptitud%20=%20'Aptitud%20media')%20/%20SUM(area_ha))%20*%20100,%202)%20AS%20aptitud_media_pct,%20ROUND(SUM(area_ha)%20FILTER%20(WHERE%20aptitud%20=%20'Aptitud%20baja'),%200)%20AS%20aptitud_baja_ha,%20ROUND((SUM(area_ha)%20FILTER%20(WHERE%20aptitud%20=%20'Aptitud%20baja')%20/%20SUM(area_ha))%20*%20100,%202)%20AS%20aptitud_baja_pct,%20ROUND(SUM(area_ha)%20FILTER%20(WHERE%20aptitud%20=%20'Exclusión%20legal'),%200)%20AS%20exclusion_legal_ha,%20ROUND((SUM(area_ha)%20FILTER%20(WHERE%20aptitud%20=%20'Exclusión%20legal')%20/%20SUM(area_ha))%20*%20100,%202)%20AS%20exclusion_legal_pct,%20ROUND(SUM(area_ha)%20FILTER%20(WHERE%20aptitud%20=%20'No%20apta'),%200)%20AS%20no_apta_ha,%20ROUND((SUM(area_ha)%20FILTER%20(WHERE%20aptitud%20=%20'No%20apta')%20/%20SUM(area_ha))%20*%20100,%202)%20AS%20no_apta_pct%20WHERE%20departamen%20IS%20NOT%20NULL%20AND%20municipio%20IS%20NOT%20NULL%20AND%20aptitud%20IS%20NOT%20NULL%20GROUP%20BY%20departamen,%20municipio%20ORDER%20BY%20departamen,%20municipio";

            string appToken = ConfigurationManager.AppSettings["AppToken"];

            using (HttpClient client = new HttpClient())
            {
                client.DefaultRequestHeaders.Add("X-App-Token", appToken);
                HttpResponseMessage response = await client.GetAsync(apiUrl);

                if (response.IsSuccessStatusCode)
                {
                    string json = await response.Content.ReadAsStringAsync();
                    aptitudDataList = JsonConvert.DeserializeObject<List<AptitudData>>(json);
                    Session["AptitudDataList"] = aptitudDataList;

                    // Guardar el JSON directamente en el HiddenField
                    hdnChartData.Value = json;

                    // Llamar a la función para cargar los datos de los Top 10 departamentos
                    await LoadTopDepartmentsDataAsync();
                }
                else
                {
                    string error = $"No se pudo obtener los datos de aptitud. Código de estado: {response.StatusCode}";
                    ShowError(error);
                }
            }
        }

        private void LoadDepartamentos()
        {
            var departamentos = aptitudDataList.Select(a => a.departamen).Distinct().OrderBy(d => d).ToList();

            ddlDepartamentos.DataSource = departamentos;
            ddlDepartamentos.DataBind();

            ddlDepartamentos.Items.Insert(0, new ListItem("-- Selecciona un departamento --", ""));
        }

        protected void ddlDepartamentos_SelectedIndexChanged(object sender, EventArgs e)
        {
            string selectedDepartamento = ddlDepartamentos.SelectedValue;

            var municipios = aptitudDataList.Where(a => a.departamen == selectedDepartamento)
                                            .Select(a => a.municipio)
                                            .Distinct()
                                            .OrderBy(m => m)
                                            .ToList();

            ddlMunicipios.DataSource = municipios;
            ddlMunicipios.DataBind();

            ddlMunicipios.Items.Insert(0, new ListItem("-- Selecciona un municipio --", ""));

            // Actualizar la validez del formulario
            ScriptManager.RegisterStartupScript(this, GetType(), "checkStartFormValidity", "checkStartFormValidity();", true);
        }

        protected async void btnIniciar_Click(object sender, EventArgs e)
        {
            try
            {
                string selectedDepartamento = ddlDepartamentos.SelectedValue;
                string selectedMunicipio = ddlMunicipios.SelectedValue;
                await LoadMapDataAsync(selectedDepartamento, selectedMunicipio);
                if (string.IsNullOrEmpty(selectedDepartamento) || string.IsNullOrEmpty(selectedMunicipio))
                {
                    ShowError("Por favor, selecciona un departamento y un municipio antes de iniciar.");
                    return;
                }

                Session["SelectedDepartamento"] = selectedDepartamento;
                Session["SelectedMunicipio"] = selectedMunicipio;

                // Obtener y formatear los datos
                var selectedData = aptitudDataList.FirstOrDefault(a =>
                    a.departamen == selectedDepartamento && a.municipio == selectedMunicipio);

                if (selectedData != null)
                {
                    var formattedData = FormatAptitudData(selectedData);

                    // Actualizar el GridView con los datos formateados
                    var displayData = new[]
                    {
                        new
                        {
                            Departamento = formattedData.departamen,
                            Municipio = formattedData.municipio,
                            TotalArea = $"{formattedData.total_area_ha} Hectáreas",
                            AptitudAlta = $"{formattedData.aptitud_alta_ha} Hectáreas ({formattedData.aptitud_alta_pct:N2}%)",
                            AptitudMedia = $"{formattedData.aptitud_media_ha} Hectáreas ({formattedData.aptitud_media_pct:N2}%)",
                            AptitudBaja = $"{formattedData.aptitud_baja_ha} Hectáreas ({formattedData.aptitud_baja_pct:N2}%)",
                            ExclusionLegal = $"{formattedData.exclusion_legal_ha} Hectáreas ({formattedData.exclusion_legal_pct:N2}%)",
                            NoApta = $"{formattedData.no_apta_ha} Hectáreas ({formattedData.no_apta_pct:N2}%)"
                        }
                    };

                    gvAptitudData.DataSource = displayData;
                    gvAptitudData.DataBind();
                    string chartScript = $@"
    createPieChart({{
        aptitudAlta: {formattedData.aptitud_alta_pct},
        aptitudMedia: {formattedData.aptitud_media_pct},
        aptitudBaja: {formattedData.aptitud_baja_pct},
        exclusionLegal: {formattedData.exclusion_legal_pct},
        noApta: {formattedData.no_apta_pct}
    }});";

                    ScriptManager.RegisterStartupScript(this, GetType(), "CreatePieChart", chartScript, true);
                    // Mostrar el contenedor de datos de aptitud
                    aptitudDataContainer.Style["display"] = "block";
                    // Mostrar el contenedor para iniciar una nueva asesoría
                    ScriptManager.RegisterStartupScript(this, GetType(), "ShowNewConsultationButton", "document.getElementById('newConsultationContainer').style.display = 'block';", true);
                    // Generar mensaje para Cohere
                    string cohereMessage = GenerateCohereMessage(formattedData);

                    // Enviar mensaje a Cohere y procesar respuesta
                    string conversationId = Session["ConversationId"] as string;
                    var response = await _cohereApiClient.SendMessage(cohereMessage, conversationId);

                    // Mostrar el panel de chat y agregar el mensaje inicial
                    chatPanel.Style["display"] = "block";
                    AddMessageToChat("Asistente", response.Generations[0].Text);

                    // Deshabilitar controles de selección
                    ddlDepartamentos.Enabled = false;
                    ddlMunicipios.Enabled = false;
                    btnIniciar.Enabled = false;

                    // Habilitar el chat
                    txtMessage.Enabled = true;
                    btnSend.Enabled = true;

                    // Actualizar la interfaz usando JavaScript
                    string script = @"
                document.getElementById('chatPanel').style.display = 'block';
                document.getElementById('" + txtMessage.ClientID + @"').focus();
                
            ";
                    ScriptManager.RegisterStartupScript(this, GetType(), "ShowChat", script, true);
                }
                else
                {
                    ShowError("No se encontró información de aptitud para el departamento y municipio seleccionados.");
                }
            }
            catch (Exception ex)
            {
                ShowError($"Ocurrió un error al iniciar el chat: {ex.Message}");
            }
        }

        private string FormatHectares(string hectaresStr)
        {
            if (string.IsNullOrEmpty(hectaresStr))
            {
                return "0.00 Hectáreas";
            }

            if (double.TryParse(hectaresStr, out double hectares))
            {
                return $"{Math.Round(hectares, 2):N2} Hectáreas";
            }
            return "0.00 Hectáreas";
        }

        private string FormatHectaresWithPercentage(string hectaresStr, string percentageStr)
        {
            double hectares = 0.00;
            double percentage = 0.00;

            if (!string.IsNullOrEmpty(hectaresStr))
            {
                double.TryParse(hectaresStr, out hectares);
            }

            if (!string.IsNullOrEmpty(percentageStr))
            {
                double.TryParse(percentageStr, out percentage);
            }

            return $"{Math.Round(hectares, 2):N2} Hectáreas ({Math.Round(percentage, 2):N2}%)";
        }

        protected async void btnSend_Click(object sender, EventArgs e)
        {
            try
            {
                string message = txtMessage.Text.Trim();
                if (string.IsNullOrEmpty(message))
                {
                    ShowError("Por favor, ingresa un mensaje antes de consultar.");
                    return;
                }

                // Lógica para enviar el mensaje a la IA
                string conversationId = Session["ConversationId"] as string;
                var response = await _cohereApiClient.SendMessage(message, conversationId);

                // Agregar mensajes al chat
                AddMessageToChat("Usuario", message);
                AddMessageToChat("Asistente", response.Generations[0].Text);

                // Limpiar el campo de texto
                txtMessage.Text = string.Empty;
                txtMessage.Focus();
            }
            catch (Exception ex)
            {
                ShowError($"Ocurrió un error al procesar tu consulta: {ex.Message}");
            }
        }

        private void UpdateGridView(FormattedAptitudData data)
        {
            var displayData = new[]
            {
                new
                {
                    Departamento = data.departamen,
                    Municipio = data.municipio,
                    TotalArea = FormatHectares(data.total_area_ha),
                    AptitudAlta = FormatHectaresWithPercentage(data.aptitud_alta_ha, data.aptitud_alta_pct),
                    AptitudMedia = FormatHectaresWithPercentage(data.aptitud_media_ha, data.aptitud_media_pct),
                    AptitudBaja = FormatHectaresWithPercentage(data.aptitud_baja_ha, data.aptitud_baja_pct),
                    ExclusionLegal = FormatHectaresWithPercentage(data.exclusion_legal_ha, data.exclusion_legal_pct),
                    NoApta = FormatHectaresWithPercentage(data.no_apta_ha, data.no_apta_pct)
                }
            };

            gvAptitudData.DataSource = displayData;
            gvAptitudData.DataBind();
        }

        private string GenerateCohereMessage(FormattedAptitudData data)
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine($"<strong>Resumen de Datos de Aptitud de Suelo en {data.municipio}, {data.departamen}:</strong><br/>");
            sb.AppendLine($"<strong>Área Total:</strong> {data.total_area_ha} Hectáreas<br/>");
            sb.AppendLine($"<strong>Aptitud Alta:</strong> {data.aptitud_alta_ha} Hectáreas ({data.aptitud_alta_pct}%)<br/>");
            sb.AppendLine($"<strong>Aptitud Media:</strong> {data.aptitud_media_ha} Hectáreas ({data.aptitud_media_pct}%)<br/>");
            sb.AppendLine($"<strong>Aptitud Baja:</strong> {data.aptitud_baja_ha} Hectáreas ({data.aptitud_baja_pct}%)<br/>");
            sb.AppendLine($"<strong>Exclusión Legal:</strong> {data.exclusion_legal_ha} Hectáreas ({data.exclusion_legal_pct}%)<br/>");
            sb.AppendLine($"<strong>Área No Apta:</strong> {data.no_apta_ha} Hectáreas ({data.no_apta_pct}%)<br/>");

            // Añadir análisis detallado
            sb.AppendLine("<br/><strong>Análisis Detallado:</strong><br/>");
            sb.AppendLine($"<strong>1. Aptitud Alta y Media:</strong> La distribución de los suelos en {data.municipio} muestra un potencial significativo para la inversión en el sector lechero. La categoría de Aptitud Alta representa más del {data.aptitud_alta_pct} del área total, lo que es muy prometedor.<br/>");
            sb.AppendLine($"<strong>2. Restricciones y Áreas No Aptas:</strong> Es importante considerar las áreas de Exclusión Legal, que comprenden el {data.exclusion_legal_pct} del territorio. Estas zonas pueden estar sujetas a restricciones ambientales o legales que limiten las actividades agrícolas.<br/>");
            sb.AppendLine($"<strong>3. Potencial de Expansión:</strong> El potencial de expansión es moderado. Si bien hay una cantidad considerable de tierra con aptitud alta y media, las áreas no aptas y de exclusión legal reducen las opciones.<br/>");

            // Añadir análisis de riesgo-beneficio
            sb.AppendLine("<br/><strong>Análisis de Riesgo-Beneficio:</strong> La inversión en el sector lechero en {data.municipio} presenta un balance positivo. La alta proporción de tierras con aptitud alta sugiere una alta productividad potencial.<br/>");

            // Añadir recomendaciones
            sb.AppendLine("<br/><strong>Recomendaciones:</strong><br/>");
            sb.AppendLine($"<strong>Zonas Óptimas:</strong> Comenzar operaciones en las áreas de Aptitud Alta es la estrategia más favorable.<br/>");
            sb.AppendLine($"<strong>Consideraciones Especiales:</strong> Investigar y cumplir con las regulaciones legales en áreas de exclusión para evitar conflictos.<br/>");
            sb.AppendLine($"<strong>Pasos Siguientes:</strong> Realizar un estudio más detallado de las zonas de Aptitud Alta para identificar las ubicaciones exactas de las granjas lecheras.<br/>");

            // Conclusión
            sb.AppendLine("<br/><strong>Conclusión:</strong> La inversión en el sector lechero en {data.municipio} es <strong>Favorable</strong>, especialmente en áreas de Aptitud Alta.");

            return sb.ToString();
        }

        private void ShowError(string message)
        {
            lblErrorMessage.Text = message;
            lblErrorMessage.Visible = true;
        }

        public class AptitudMapData
        {
            public Dictionary<string, object> the_geom { get; set; }
            public string aptitud { get; set; }
        }

        private async Task LoadMapDataAsync(string departamento, string municipio)
        {
            string apiUrl = $"https://www.datos.gov.co/resource/bfuy-8yvf.json?$query=SELECT%20the_geom,%20aptitud%20WHERE%20departamen%20=%20'{departamento}'%20AND%20municipio%20=%20'{municipio}'";

            string appToken = ConfigurationManager.AppSettings["AppToken"];

            using (HttpClient client = new HttpClient())
            {
                if (!string.IsNullOrEmpty(appToken))
                {
                    client.DefaultRequestHeaders.Add("X-App-Token", appToken);
                }

                HttpResponseMessage response = await client.GetAsync(apiUrl);

                if (response.IsSuccessStatusCode)
                {
                    string json = await response.Content.ReadAsStringAsync();
                    var mapData = JsonConvert.DeserializeObject<List<AptitudMapData>>(json);

                    // Guardar los datos en la sesión
                    Session["MapData"] = json;

                    // Registrar el script para inicializar el mapa
                    string script = $@"
                if (typeof initializeMap === 'function') {{
                    var mapData = {json};
                    initializeMap(mapData);
                }}
            ";
                    ScriptManager.RegisterStartupScript(this, GetType(), "InitMap", script, true);
                }
                else
                {
                    ShowError($"Error al cargar datos del mapa: {response.StatusCode}");
                }
            }
        }

        private void AddMessageToChat(string sender, string message)
        {
            string formattedMessage = MessageFormatter.FormatMessage(message);
            string messageClass = sender.ToLower() == "usuario" ? "usuario-message" : "ia-message";
            chatContainer.InnerHtml += $@"
        <div class='{messageClass}'>
            <strong class='sender-name'>{sender}:</strong>
            <div class='message-content'>{formattedMessage}</div>
        </div>";
        }

        private void RemoveLoadingMessage()
        {
            // Remover mensaje de "Generando..." si existe
            string loadingMessage = "<p class='loading-message'>Generando...</p>";
            chatContainer.InnerHtml = chatContainer.InnerHtml.Replace(loadingMessage, "");
        }

        protected void gvAptitudData_RowDataBound(object sender, GridViewRowEventArgs e)
        {
            if (e.Row.RowType == DataControlRowType.DataRow)
            {
                e.Row.Attributes["style"] = $"--row-index: {e.Row.RowIndex};";
            }
        }
    }
}