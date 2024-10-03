namespace CodetyperFunctionBackend.Model
{
    public class User
    {
        public int UserId { get; set; }
        public required string Username { get; set; }
        public string? PasswordHash { get; set; }
        public string? Email { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public required string RoleName { get; set; }
    }
}
