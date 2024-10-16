using CodetyperFunctionBackend.Model;
using CodetyperFunctionBackend.Utils;
using Microsoft.AspNetCore.Identity;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Logging;
using System.Net;

namespace CodetyperFunctionBackend.Functions
{
    internal class UserFunctions
    {
        private readonly ILogger _logger;
        private readonly string _connectionString;

        public UserFunctions(ILoggerFactory loggerFactory)
        {
            _logger = loggerFactory.CreateLogger<AuthFunctions>();
            _connectionString = Environment.GetEnvironmentVariable("SqlConnectionString")!;
        }

        [Function("PromoteToModerator")]
        public async Task<HttpResponseData> PromoteToModerator(
            [HttpTrigger(AuthorizationLevel.User, "post", Route = "users/promote/moderator/{userId}")] HttpRequestData req, string userId)
        {
            if (!AuthHelper.IsUserAuthorized(req, Roles.SuperAdmin, Roles.Admin))
            {
                var forbiddenResponse = req.CreateResponse(HttpStatusCode.Forbidden);
                await forbiddenResponse.WriteStringAsync("You do not have permission to perform this action.");
                return forbiddenResponse;
            }

            _logger.LogInformation($"Promoting user {userId} to Moderator.");

            var result = await PromoteUser("Moderator", userId);
            var response = req.CreateResponse();

            if (!result.Succeeded)
            {
                response.StatusCode = HttpStatusCode.BadRequest;
                await response.WriteAsJsonAsync(result.Errors);
                return response;
            }

            response.StatusCode = HttpStatusCode.OK;
            await response.WriteAsJsonAsync("User promoted to Moderator successfully.");
            return response;
        }

        [Function("PromoteToAdmin")]
        public async Task<HttpResponseData> PromoteToAdmin(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = "users/promote/admin/{userId}")] HttpRequestData req, string userId)
        {
            if (!AuthHelper.IsUserAuthorized(req, Roles.SuperAdmin))
            {
                var forbiddenResponse = req.CreateResponse(HttpStatusCode.Forbidden);
                await forbiddenResponse.WriteStringAsync("You do not have permission to perform this action.");
                return forbiddenResponse;
            }

            _logger.LogInformation($"Promoting user {userId} to Admin.");

            var result = await PromoteUser("Admin", userId);
            var response = req.CreateResponse();

            if (!result.Succeeded)
            {
                response.StatusCode = HttpStatusCode.BadRequest;
                await response.WriteAsJsonAsync(result.Errors);
                return response;
            }

            response.StatusCode = HttpStatusCode.OK;
            await response.WriteAsJsonAsync("User promoted to Admin successfully.");
            return response;
        }
        private async Task<IdentityResult> PromoteUser(string rolePromotion, string userId)
        {
            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                await conn.OpenAsync();
                var user = await FindUserById(conn, userId);
                if (user == null)
                {
                    return IdentityResult.Failed(new IdentityError { Description = "User not found." });
                }

                if (await IsUserInRole(conn, userId, rolePromotion))
                {
                    return IdentityResult.Failed(new IdentityError { Description = $"User is already in the {rolePromotion} role." });
                }

                return await AddUserToRole(conn, userId, rolePromotion);
            }
        }

        [Function("GetUserById")]
        public async Task<HttpResponseData> GetUserById(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "users/{userId}")] HttpRequestData req, string userId)
        {
            var response = req.CreateResponse();
            string? connectionString = Environment.GetEnvironmentVariable("SqlConnectionString");

            if (string.IsNullOrEmpty(connectionString))
            {
                response.StatusCode = HttpStatusCode.InternalServerError;
                await response.WriteStringAsync("Connection string is missing.");
                return response;
            }

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                await conn.OpenAsync();
                var user = await FindUserById(conn, userId);

                if (user == null)
                {
                    response.StatusCode = HttpStatusCode.NotFound;
                    await response.WriteStringAsync($"User with ID '{userId}' not found.");
                }
                else
                {
                    await response.WriteAsJsonAsync(user);
                }
            }

            return response;
        }

        private async Task<User?> FindUserById(SqlConnection conn, string userId)
        {
            string query = "SELECT UserId, Username, RoleName FROM Users WHERE UserId = @UserId";
            using (SqlCommand cmd = new SqlCommand(query, conn))
            {
                cmd.Parameters.AddWithValue("@UserId", userId);
                using (var reader = await cmd.ExecuteReaderAsync())
                {
                    if (await reader.ReadAsync())
                    {
                        return new User
                        {
                            UserId = reader.GetString(0),
                            Username = reader.GetString(1),
                            RoleName = reader.GetString(2)
                        };
                    }
                }
            }
            return null;
        }

        private async Task<bool> IsUserInRole(SqlConnection conn, string userId, string role)
        {
            string query = "SELECT COUNT(1) FROM UserRoles WHERE UserId = @UserId AND RoleName = @Role";
            using (SqlCommand cmd = new SqlCommand(query, conn))
            {
                cmd.Parameters.AddWithValue("@UserId", userId);
                cmd.Parameters.AddWithValue("@Role", role);

                var result = await cmd.ExecuteNonQueryAsync();

                return result > 0;
            }
        }

        private async Task<IdentityResult> AddUserToRole(SqlConnection conn, string userId, string role)
        {
            string query = "INSERT INTO UserRoles (UserId, RoleName) VALUES (@UserId, @RoleName)";
            using (SqlCommand cmd = new SqlCommand(query, conn))
            {
                cmd.Parameters.AddWithValue("@UserId", userId);
                cmd.Parameters.AddWithValue("@RoleName", role);

                var rowsAffected = await cmd.ExecuteNonQueryAsync();
                if (rowsAffected > 0)
                {
                    return IdentityResult.Success;
                }
            }
            return IdentityResult.Failed(new IdentityError { Description = "Failed to promote user." });
        }
    }
}
