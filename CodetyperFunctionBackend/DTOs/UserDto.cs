namespace CodetyperFunctionBackend.DTOs
{
    internal class UserDto
    {
        public string? UserId { get; set; }
        public required string Username { get; set; }

        public string? Password { get; set; }
        public string? PasswordHash { get; set; }

        public string? RoleName { get; set; }
        public string? Email { get; set; }
    }
}
