using System.Text.Json.Serialization;

namespace CodetyperFunctionBackend.Model
{
    internal class CodingTask
    {
        [JsonPropertyName("id")]
        public int Id { get; set; }
        [JsonPropertyName("name")]
        public required string Name { get; set; }
        [JsonPropertyName("description")]
        public required string Description { get; set; }
        [JsonPropertyName("shown")]
        public bool Shown { get; set; }
        [JsonPropertyName("creatorId")]
        public required string CreatorId { get; set; }

        public static class Fields
        {
            public const string TableName = "Tasks";
            public const string Id = "Id";
            public const string Name = "Name";
            public const string Description = "Description";
            public const string Shown = "Shown";
            public const string CreatorId = "CreatorId";
        }
    }
}
