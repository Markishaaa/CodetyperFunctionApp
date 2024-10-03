using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using System.Data.SqlClient;
using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Security.Claims;
using System.Text;

namespace CodetyperFunctionBackend.Functions
{
    internal class AuthFunctions
    {
        private readonly ILogger _logger;
        private readonly string _connectionString;

        private readonly string _jwtSecret;
        private readonly string _issuer;
        private readonly string _audience;

        public AuthFunctions(ILoggerFactory loggerFactory)
        {
            _logger = loggerFactory.CreateLogger<AuthFunctions>();
            _connectionString = Environment.GetEnvironmentVariable("SqlConnectionString")!;

            _jwtSecret = Environment.GetEnvironmentVariable("JWT_SECRET") ?? throw new InvalidOperationException("JWT_SECRET not set");
            _issuer = Environment.GetEnvironmentVariable("JWT_ISSUER") ?? throw new InvalidOperationException("JWT_ISSUER not set");
            _audience = Environment.GetEnvironmentVariable("JWT_AUDIENCE") ?? throw new InvalidOperationException("JWT_AUDIENCE not set");
        }

        [Function("RegisterUser")]
        public async Task<HttpResponseData> RegisterUser([HttpTrigger(AuthorizationLevel.Anonymous, "post")] HttpRequestData req)
        {
            _logger.LogInformation("Processing user registration.");

            var requestBody = await req.ReadAsStringAsync();
            dynamic data = Newtonsoft.Json.JsonConvert.DeserializeObject(requestBody!)!;
            string username = data?.username;
            string password = data?.password;
            string email = data?.email;
            string roleName = "Admin";

            if (string.IsNullOrEmpty(username) || username.Length < 3)
            {
                var badRequestResponse = req.CreateResponse(HttpStatusCode.BadRequest);
                await badRequestResponse.WriteStringAsync("Username must be at least 3 characters long.");
                return badRequestResponse;
            }

            if (string.IsNullOrEmpty(password) || password.Length < 8)
            {
                var badRequestResponse = req.CreateResponse(HttpStatusCode.BadRequest);
                await badRequestResponse.WriteStringAsync("Password must be at least 8 characters long.");
                return badRequestResponse;
            }

            if (string.IsNullOrEmpty(email))
            {
                var badRequestResponse = req.CreateResponse(HttpStatusCode.BadRequest);
                await badRequestResponse.WriteStringAsync("Email is required.");
                return badRequestResponse;
            }

            string passwordHash = BCrypt.Net.BCrypt.HashPassword(password);

            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                await conn.OpenAsync();

                string checkUserQuery = "SELECT COUNT(1) FROM Users WHERE Username = @Username";
                SqlCommand checkUserCmd = new SqlCommand(checkUserQuery, conn);
                checkUserCmd.Parameters.AddWithValue("@Username", username);

                int userExists = (int)await checkUserCmd.ExecuteScalarAsync();

                if (userExists > 0)
                {
                    var conflictResponse = req.CreateResponse(HttpStatusCode.Conflict); // 409 Conflict
                    await conflictResponse.WriteStringAsync("Username is already taken.");
                    return conflictResponse;
                }

                string insertUserQuery = "INSERT INTO Users (UserId, Username, PasswordHash, Email, RoleName, CreatedAt, UpdatedAt) " +
                                     "VALUES (NEWID(), @Username, @PasswordHash, @Email, @RoleName, GETDATE(), GETDATE())";
                SqlCommand insertUserCmd = new SqlCommand(insertUserQuery, conn);
                insertUserCmd.Parameters.AddWithValue("@Username", username);
                insertUserCmd.Parameters.AddWithValue("@PasswordHash", passwordHash);
                insertUserCmd.Parameters.AddWithValue("@Email", email);
                insertUserCmd.Parameters.AddWithValue("@RoleName", roleName);

                await insertUserCmd.ExecuteNonQueryAsync();
            }

            var response = req.CreateResponse(HttpStatusCode.Created);
            await response.WriteStringAsync("User registered successfully.");
            return response;
        }

        [Function("LoginUser")]
        public async Task<HttpResponseData> LoginUser([HttpTrigger(AuthorizationLevel.Anonymous, "post")] HttpRequestData req)
        {
            _logger.LogInformation("Processing user login.");

            var requestBody = await req.ReadAsStringAsync();
            dynamic data = Newtonsoft.Json.JsonConvert.DeserializeObject(requestBody!)!;
            string username = data?.username;
            string password = data?.password;

            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
            {
                var badRequestResponse = req.CreateResponse(HttpStatusCode.BadRequest);
                await badRequestResponse.WriteStringAsync("Username and password are required.");
                return badRequestResponse;
            }

            string storedPasswordHash = null;
            string userRole = null;
            string userId = null;

            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                await conn.OpenAsync();

                var query = @"
                    SELECT u.PasswordHash, r.RoleName, u.UserId
                    FROM Users u 
                    INNER JOIN Roles r ON u.RoleName = r.RoleName
                    WHERE u.Username = @Username";

                using (var cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@Username", username);

                    using (var reader = await cmd.ExecuteReaderAsync())
                    {
                        if (reader.Read())
                        {
                            storedPasswordHash = reader["PasswordHash"].ToString();
                            userRole = reader["RoleName"].ToString();
                            userId = reader["UserId"].ToString();
                        }
                    }
                }
            }

            if (storedPasswordHash == null || !BCrypt.Net.BCrypt.Verify(password, storedPasswordHash))
            {
                var unauthorizedResponse = req.CreateResponse(HttpStatusCode.Unauthorized);
                await unauthorizedResponse.WriteStringAsync("Invalid credentials.");
                return unauthorizedResponse;
            }

            var token = GenerateJwtToken(username, userRole, userId);

            var successResponse = req.CreateResponse(HttpStatusCode.OK);
            successResponse.Headers.Add("Authorization", $"Bearer {token}");
            successResponse.Headers.Add("Access-Control-Expose-Headers", "Authorization");
            await successResponse.WriteStringAsync("Login successful.");
            return successResponse;
        }

        private string GenerateJwtToken(string username, string userRole, string userId)
        {
            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSecret));
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

            var claims = new[]
            {
            new Claim("username", username),
            new Claim("role", userRole),
            new Claim("userId", userId)
        };

            var token = new JwtSecurityToken(
                issuer: _issuer,
                audience: _audience,
                claims: claims,
                expires: DateTime.Now.AddHours(1),
                signingCredentials: credentials
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}
