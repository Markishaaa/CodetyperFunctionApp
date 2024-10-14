using System.Text.Json.Serialization;

namespace CodetyperFunctionBackend.Model
{
    internal class CodeSnippet
    {
        [JsonPropertyName("id")]
        public int Id { get; set; }
        [JsonPropertyName("content")]
        public string Content { get; set; }
        [JsonPropertyName("shown")]
        public bool Shown { get; set; }
        [JsonPropertyName("languageName")]
        public string LanguageName { get; set; }
        [JsonPropertyName("taskId")]
        public int TaskId { get; set; }
        [JsonPropertyName("creatorId")]
        public string CreatorId { get; set; }
    }
}
