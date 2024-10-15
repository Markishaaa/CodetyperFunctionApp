using System.Text.Json.Serialization;

namespace CodetyperFunctionBackend.Model
{
    internal class CodeSnippet
    {
        [JsonPropertyName("id")]
        public int Id { get; set; }
        [JsonPropertyName("content")]
        public required string Content { get; set; }
        [JsonPropertyName("shown")]
        public bool Shown { get; set; }
        [JsonPropertyName("languageName")]
        public required string LanguageName { get; set; }
        [JsonPropertyName("taskId")]
        public int TaskId { get; set; }
        [JsonPropertyName("creatorId")]
        public required string CreatorId { get; set; }

        public static class Fields
        {
            public const string TableName = "Snippets";
            public const string Id = "Id";
            public const string Content = "Content";
            public const string Shown = "Shown";
            public const string LanguageName = "LanguageName";
            public const string TaskId = "TaskId";
            public const string CreatorId = "CreatorId";
        }
    }
}
