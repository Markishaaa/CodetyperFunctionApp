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

        public static class Fields
        {
            public const string TableName = "Users";
            public const string UserId = "UserId";
            public const string Username = "Username";
            public const string PasswordHash = "PasswordHash";
            public const string Email = "Email";
            public const string CreatedAt = "CreatedAt";
            public const string UpdatedAt = "UpdatedAt";
            public const string RoleName = "RoleName";
        }
    }
}
