using System.Text.Json.Serialization;

namespace CodetyperFunctionBackend.Model
{
    internal class CodingTask
    {
        [JsonPropertyName("id")]
        public int Id { get; set; }
        [JsonPropertyName("name")]
        public string Name { get; set; }
        [JsonPropertyName("description")]
        public string Description { get; set; }
        [JsonPropertyName("shown")]
        public bool Shown { get; set; }
        [JsonPropertyName("creatorId")]
        public string CreatorId { get; set; }
        [JsonPropertyName("codeSnippets")]
        public List<CodeSnippet> CodeSnippets { get; set; }

        public CodingTask()
        {
            CodeSnippets = new List<CodeSnippet>();
        }
    }
}
