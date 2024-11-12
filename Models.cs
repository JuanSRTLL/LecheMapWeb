using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace LecheMap
{
    public class CohereRequest
    {
        [JsonProperty("model")]
        public string Model { get; set; }

        [JsonProperty("prompt")]
        public string Prompt { get; set; }

        [JsonProperty("max_tokens")]
        public int MaxTokens { get; set; }

        [JsonProperty("temperature")]
        public double Temperature { get; set; }

        [JsonProperty("k")]
        public int K { get; set; }

        [JsonProperty("stop_sequences")]
        public List<string> StopSequences { get; set; }

        [JsonProperty("return_likelihoods")]
        public string ReturnLikelihoods { get; set; }
    }

    public class CohereResponse
    {
        [JsonProperty("generations")]
        public List<Generation> Generations { get; set; }
    }

    public class Generation
    {
        [JsonProperty("text")]
        public string Text { get; set; }
    }
    public class BingNewsResponse
    {
        [JsonProperty("webPages")]
        public WebPages WebPages { get; set; }
    }

    public class WebPages
    {
        [JsonProperty("value")]
        public List<NewsArticle> Value { get; set; }
    }

    public class NewsArticle
    {
        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("url")]
        public string Url { get; set; }

        [JsonProperty("snippet")]
        public string Description { get; set; }

        [JsonProperty("dateLastCrawled")]
        public DateTime DatePublished { get; set; }
    }
}