using System.Text.Json.Serialization;

namespace CodetyperFunctionBackend.Model
{
    public class Language
    {
        [JsonPropertyName("name")]
        public required string Name { get; set; }
    }
}
