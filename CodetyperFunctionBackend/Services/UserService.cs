using CodetyperFunctionBackend.DTOs;
using CodetyperFunctionBackend.Model;
using CodetyperFunctionBackend.Repositories;
using CodetyperFunctionBackend.Utils;
using Microsoft.Extensions.Logging;

namespace CodetyperFunctionBackend.Services
{
    internal class UserService
    {
        private readonly UserRepository _userRepository;

        public UserService(UserRepository userRepository, ILoggerFactory loggerFactory)
        {
            _userRepository = userRepository;
        }

        public async Task<(bool success, string message)> RegisterUserAsync(UserDto userDto)
        {
            if (string.IsNullOrEmpty(userDto.Username) || userDto.Username.Length < 3)
                return (false, "Username must be at least 3 characters long.");
            if (string.IsNullOrEmpty(userDto.Password) || userDto.Password.Length < 8)
                return (false, "Password must be at least 8 characters long.");
            if (string.IsNullOrEmpty(userDto.Email))
                return (false, "Email is required.");

            bool isUsernameTaken = await _userRepository.IsUsernameTakenAsync(userDto.Username);
            if (isUsernameTaken)
                return (false, "Username is already taken.");

            string passwordHash = BCrypt.Net.BCrypt.HashPassword(userDto.Password);

            User user = new User();
            user.UserId = Guid.NewGuid().ToString();
            user.Username = userDto.Username;
            user.PasswordHash = passwordHash;
            user.Email = userDto.Email;
            user.CreatedAt = DateTime.UtcNow;
            user.UpdatedAt = DateTime.UtcNow;
            user.RoleName = "User";

            await _userRepository.AddUserAsync(user);

            return (true, "User registered successfully.");
        }

        public async Task<(bool Success, string Message, string? Token)> LoginAsync(string username, string password)
        {
            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
            {
                return (false, "Username and password are required.", null);
            }

            var user = await _userRepository.GetUserByUsernameAsync(username);

            if (user == null || user.PasswordHash == null || !BCrypt.Net.BCrypt.Verify(password, user.PasswordHash))
            {
                return (false, "Invalid credentials", null);
            }

            var token = AuthHelper.GenerateJwtToken(username, user.RoleName!, user.UserId!.ToString());
            return (true, "Login successful", token);
        }
    }
}
