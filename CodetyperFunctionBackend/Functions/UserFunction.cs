using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using System.Data.SqlClient;
using System.Net;
using System.Security.Cryptography;
using System.Text;

namespace CodetyperFunctionBackend.Functions
{
    internal class UserFunction
    {
        private readonly ILogger _logger;
        private readonly string _connectionString;

        public UserFunction(ILoggerFactory loggerFactory)
        {
            _logger = loggerFactory.CreateLogger<UserFunction>();
            _connectionString = Environment.GetEnvironmentVariable("SqlConnectionString")!;
        }

        private string HashPassword(string password)
        {
            using (SHA256 sha256 = SHA256.Create())
            {
                byte[] hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
                return Convert.ToBase64String(hashedBytes);
            }
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
            string roleName = "User";

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

            string passwordHash = HashPassword(password);

            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                conn.Open();

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

        private bool VerifyPassword(string password, string storedPasswordHash)
        {
            string passwordHash = HashPassword(password);
            return passwordHash == storedPasswordHash;
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

            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                conn.Open();
                string query = "SELECT PasswordHash FROM Users WHERE Username = @Username";
                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@Username", username);

                storedPasswordHash = (string)await cmd.ExecuteScalarAsync();
            }

            if (storedPasswordHash == null || !VerifyPassword(password, storedPasswordHash))
            {
                var unauthorizedResponse = req.CreateResponse(HttpStatusCode.Unauthorized);
                await unauthorizedResponse.WriteStringAsync("Invalid credentials.");
                return unauthorizedResponse;
            }

            var successResponse = req.CreateResponse(HttpStatusCode.OK);
            await successResponse.WriteStringAsync("Login successful.");
            return successResponse;
        }
        
    }
}
