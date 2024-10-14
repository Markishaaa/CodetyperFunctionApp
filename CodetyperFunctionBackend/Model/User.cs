using System.Text.Json.Serialization;

namespace CodetyperFunctionBackend.Model
{
    public class User
    {
        [JsonPropertyName("userId")]
        public string? UserId { get; set; }
        [JsonPropertyName("username")]
        public string? Username { get; set; }
        [JsonPropertyName("passwordHash")]
        public string? PasswordHash { get; set; }
        [JsonPropertyName("email")]
        public string? Email { get; set; }
        [JsonPropertyName("createdAt")]
        public DateTime CreatedAt { get; set; }
        [JsonPropertyName("updatedAt")]
        public DateTime UpdatedAt { get; set; }
        [JsonPropertyName("roleName")]
        public string? RoleName { get; set; }
    }
}
