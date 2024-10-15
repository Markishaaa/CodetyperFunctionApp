using System.Text.Json.Serialization;

namespace CodetyperFunctionBackend.Model
{
    public class Language
    {
        [JsonPropertyName("name")]
        public required string Name { get; set; }

        public static class Fields
        {
            public const string TableName = "Languages";
            public const string Name = "Name";
        }
    }
}
