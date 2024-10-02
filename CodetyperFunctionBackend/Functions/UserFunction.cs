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
            var username = data?.username;
            var password = data?.password;
            var email = data?.email;
            string roleName = "User";

            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password) || string.IsNullOrEmpty(email)) 
            {
                var badRequestResponse = req.CreateResponse(HttpStatusCode.BadRequest);
                await badRequestResponse.WriteStringAsync("Username, password, and email are required.");
                return badRequestResponse;
            }

            string passwordHash = HashPassword(password);

            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                conn.Open();

                string insertUserQuery = "INSERT INTO Users (UserId, Username, PasswordHash, Email, RoleId, CreatedAt, UpdatedAt) " +
                                     "VALUES (NEWID(), @Username, @PasswordHash, @Email, @RoleId, GETDATE(), GETDATE())";
                SqlCommand insertUserCmd = new SqlCommand(insertUserQuery, conn);
                insertUserCmd.Parameters.AddWithValue("@Username", username);
                insertUserCmd.Parameters.AddWithValue("@PasswordHash", passwordHash);
                insertUserCmd.Parameters.AddWithValue("@Email", email);
                insertUserCmd.Parameters.AddWithValue("@RoleName", roleName);
            }

            var response = req.CreateResponse(HttpStatusCode.Created);
            await response.WriteStringAsync("User registered successfully.");
            return response;
        }
    }
}
