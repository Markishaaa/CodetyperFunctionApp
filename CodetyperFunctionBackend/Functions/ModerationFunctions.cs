using CodetyperFunctionBackend.Model;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using System.Data.SqlClient;
using System.Net;

namespace CodetyperFunctionBackend.Functions
{
    public class ModerationFunctions
    {
        private readonly ILogger<ModerationFunctions> _logger;
        private readonly string _connectionString;

        public ModerationFunctions(ILogger<ModerationFunctions> logger)
        {
            _logger = logger;
            _connectionString = Environment.GetEnvironmentVariable("SqlConnectionString")!;
        }

        [Function("AcceptTaskRequest")]
        public async Task<HttpResponseData> AcceptTaskRequest(
        [HttpTrigger(AuthorizationLevel.User, "put", Route = "tasks/acceptRequest/{id}")] HttpRequestData req,
        int id)
        {
            if (!AuthHelper.IsUserAuthorized(req, Roles.SuperAdmin, Roles.Admin, Roles.Moderator))
            {
                var forbiddenResponse = req.CreateResponse(HttpStatusCode.Forbidden);
                await forbiddenResponse.WriteStringAsync("You do not have permission to perform this action.");
                return forbiddenResponse;
            }

            _logger.LogInformation($"Processing a request to accept task with ID {id}");

            if (string.IsNullOrEmpty(_connectionString))
            {
                var errorResponse = req.CreateResponse(HttpStatusCode.InternalServerError);
                await errorResponse.WriteStringAsync("Database connection string is missing.");
                return errorResponse;
            }

            string query = "UPDATE Tasks SET Shown = @shown WHERE Id = @id";

            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                await conn.OpenAsync();

                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@shown", true);
                    cmd.Parameters.AddWithValue("@id", id);

                    int rowsAffected = await cmd.ExecuteNonQueryAsync();

                    if (rowsAffected > 0)
                    {
                        var successResponse = req.CreateResponse(HttpStatusCode.OK);
                        await successResponse.WriteStringAsync($"Task with ID {id} was successfully updated.");
                        return successResponse;
                    }
                    else
                    {
                        var notFoundResponse = req.CreateResponse(HttpStatusCode.NotFound);
                        await notFoundResponse.WriteStringAsync($"Task with ID {id} not found.");
                        return notFoundResponse;
                    }
                }
            }
        }

        [Function("DenyTaskRequest")]
        public async Task<HttpResponseData> DenyRequest(
            [HttpTrigger(AuthorizationLevel.Anonymous, "delete", Route = "tasks/denyRequest/{id}")] HttpRequestData req, int id)
        {
            if (!AuthHelper.IsUserAuthorized(req, Roles.SuperAdmin, Roles.Admin, Roles.Moderator))
            {
                var forbiddenResponse = req.CreateResponse(HttpStatusCode.Forbidden);
                await forbiddenResponse.WriteStringAsync("You do not have permission to perform this action.");
                return forbiddenResponse;
            }

            _logger.LogInformation($"Processing a request to deny task with ID {id}");

            if (string.IsNullOrEmpty(_connectionString))
            {
                var errorResponse = req.CreateResponse(HttpStatusCode.InternalServerError);
                await errorResponse.WriteStringAsync("Database connection string is missing.");
                return errorResponse;
            }

            string selectQuery = "SELECT Name, Description, CreatorId FROM Tasks WHERE Id = @id";

            string archiveQuery = @"
                INSERT INTO Archive_Tasks (Name, Description, DeniedAt, Reason, CreatorId, StaffId)
                VALUES (@name, @description, @deniedAt, @reason, @creatorId, @staffId)";

            string deleteQuery = "DELETE FROM Tasks WHERE Id = @id";

            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                await conn.OpenAsync();

                using (SqlTransaction transaction = conn.BeginTransaction())
                {
                    string taskName, taskDescription, creatorId;

                    using (SqlCommand selectCmd = new SqlCommand(selectQuery, conn, transaction))
                    {
                        selectCmd.Parameters.AddWithValue("@id", id);
                        using (var reader = await selectCmd.ExecuteReaderAsync())
                        {
                            if (!reader.Read())
                            {
                                transaction.Rollback();
                                var notFoundResponse = req.CreateResponse(HttpStatusCode.NotFound);
                                await notFoundResponse.WriteStringAsync($"Task with ID {id} not found.");
                                return notFoundResponse;
                            }

                            taskName = reader["Name"].ToString();
                            taskDescription = reader["Description"].ToString();
                            creatorId = reader["CreatorId"].ToString();
                        }
                    }

                    var requestBody = await req.ReadAsStringAsync();
                    dynamic? data = Newtonsoft.Json.JsonConvert.DeserializeObject(requestBody);
                    string? reason = data?.reason;
                    string staffId = data?.staffId;

                    if (string.IsNullOrEmpty(reason) || string.IsNullOrEmpty(staffId))
                    {
                        var badRequestResponse = req.CreateResponse(HttpStatusCode.BadRequest);
                        await badRequestResponse.WriteStringAsync("Please provide a reason and staffId in the request body.");
                        return badRequestResponse;
                    }

                    // archiving the task
                    using (SqlCommand archiveCmd = new SqlCommand(archiveQuery, conn, transaction))
                    {
                        archiveCmd.Parameters.AddWithValue("@name", taskName);
                        archiveCmd.Parameters.AddWithValue("@description", taskDescription);
                        archiveCmd.Parameters.AddWithValue("@deniedAt", DateTime.UtcNow);
                        archiveCmd.Parameters.AddWithValue("@reason", reason);
                        archiveCmd.Parameters.AddWithValue("@creatorId", creatorId);
                        archiveCmd.Parameters.AddWithValue("@staffId", staffId);

                        await archiveCmd.ExecuteNonQueryAsync();
                    }

                    // deleting the task
                    using (SqlCommand deleteCmd = new SqlCommand(deleteQuery, conn, transaction))
                    {
                        deleteCmd.Parameters.AddWithValue("@id", id);
                        await deleteCmd.ExecuteNonQueryAsync();
                    }

                    transaction.Commit();

                    var successResponse = req.CreateResponse(HttpStatusCode.OK);
                    await successResponse.WriteStringAsync($"Task with ID {id} has been denied and archived.");
                    return successResponse;
                }
            }
        }


        [Function("AcceptSnippetRequest")]
        public async Task<HttpResponseData> AcceptSnippetRequest(
            [HttpTrigger(AuthorizationLevel.User, "put", Route = "snippets/acceptRequest/{id}")] HttpRequestData req,
            int id)
        {
            if (!AuthHelper.IsUserAuthorized(req, Roles.SuperAdmin, Roles.Admin, Roles.Moderator))
            {
                var forbiddenResponse = req.CreateResponse(HttpStatusCode.Forbidden);
                await forbiddenResponse.WriteStringAsync("You do not have permission to perform this action.");
                return forbiddenResponse;
            }

            _logger.LogInformation($"Processing a request to accept snippet with ID {id}");

            if (string.IsNullOrEmpty(_connectionString))
            {
                var errorResponse = req.CreateResponse(HttpStatusCode.InternalServerError);
                await errorResponse.WriteStringAsync("Database connection string is missing.");
                return errorResponse;
            }

            string query = "UPDATE Snippets SET Shown = @shown WHERE Id = @id";

            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                await conn.OpenAsync();

                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@shown", true);
                    cmd.Parameters.AddWithValue("@id", id);

                    int rowsAffected = await cmd.ExecuteNonQueryAsync();

                    if (rowsAffected > 0)
                    {
                        var successResponse = req.CreateResponse(HttpStatusCode.OK);
                        await successResponse.WriteStringAsync($"Snippet with ID {id} was successfully updated.");
                        return successResponse;
                    }
                    else
                    {
                        var notFoundResponse = req.CreateResponse(HttpStatusCode.NotFound);
                        await notFoundResponse.WriteStringAsync($"Snippet with ID {id} not found.");
                        return notFoundResponse;
                    }
                }
            }
        }

        [Function("DenySnippetRequest")]
        public async Task<HttpResponseData> DenySnippetRequest(
            [HttpTrigger(AuthorizationLevel.User, "delete", Route = "snippets/denyRequest/{id}")] HttpRequestData req,
            int id)
        {
            if (!AuthHelper.IsUserAuthorized(req, Roles.SuperAdmin, Roles.Admin, Roles.Moderator))
            {
                var forbiddenResponse = req.CreateResponse(HttpStatusCode.Forbidden);
                await forbiddenResponse.WriteStringAsync("You do not have permission to perform this action.");
                return forbiddenResponse;
            }

            _logger.LogInformation($"Processing a request to deny snippet with ID {id}");

            if (string.IsNullOrEmpty(_connectionString))
            {
                var errorResponse = req.CreateResponse(HttpStatusCode.InternalServerError);
                await errorResponse.WriteStringAsync("Database connection string is missing.");
                return errorResponse;
            }

            string selectQuery = "SELECT Content, LanguageName, TaskId, CreatorId FROM Snippets WHERE Id = @id";
            string archiveQuery = @"
                INSERT INTO ArchivedSnippet (Content, DeniedAt, LanguageName, TaskId, CreatorId, StaffId)
                VALUES (@content, @deniedAt, @languageName, @taskId, @creatorId, @staffId)";
            string deleteQuery = "DELETE FROM Snippets WHERE Id = @id";

            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                await conn.OpenAsync();

                using (SqlTransaction transaction = conn.BeginTransaction())
                {
                    string snippetContent, languageName, creatorId;
                    int taskId;

                    using (SqlCommand selectCmd = new SqlCommand(selectQuery, conn, transaction))
                    {
                        selectCmd.Parameters.AddWithValue("@id", id);
                        using (SqlDataReader reader = await selectCmd.ExecuteReaderAsync())
                        {
                            if (!reader.Read())
                            {
                                transaction.Rollback();
                                var notFoundResponse = req.CreateResponse(HttpStatusCode.NotFound);
                                await notFoundResponse.WriteStringAsync($"Snippet with ID {id} not found.");
                                return notFoundResponse;
                            }

                            snippetContent = reader["Content"].ToString();
                            languageName = reader["LanguageName"].ToString();
                            taskId = reader.GetInt32(reader.GetOrdinal("TaskId"));
                            creatorId = reader["CreatorId"].ToString();
                        }
                    }

                    var requestBody = await req.ReadAsStringAsync();
                    dynamic? data = Newtonsoft.Json.JsonConvert.DeserializeObject(requestBody);
                    string? reason = data?.reason;
                    string staffId = data?.staffId;

                    if (string.IsNullOrEmpty(reason) || string.IsNullOrEmpty(staffId))
                    {
                        var badRequestResponse = req.CreateResponse(HttpStatusCode.BadRequest);
                        await badRequestResponse.WriteStringAsync("Please provide a reason and staffId in the request body.");
                        return badRequestResponse;
                    }

                    using (SqlCommand archiveCmd = new SqlCommand(archiveQuery, conn, transaction))
                    {
                        // no @id parameter here
                        archiveCmd.Parameters.AddWithValue("@content", snippetContent);
                        archiveCmd.Parameters.AddWithValue("@deniedAt", DateTime.UtcNow);
                        archiveCmd.Parameters.AddWithValue("@languageName", languageName);
                        archiveCmd.Parameters.AddWithValue("@taskId", taskId);
                        archiveCmd.Parameters.AddWithValue("@creatorId", creatorId);
                        archiveCmd.Parameters.AddWithValue("@staffId", staffId);

                        await archiveCmd.ExecuteNonQueryAsync();
                    }

                    using (SqlCommand deleteCmd = new SqlCommand(deleteQuery, conn, transaction))
                    {
                        deleteCmd.Parameters.AddWithValue("@id", id);
                        await deleteCmd.ExecuteNonQueryAsync();
                    }

                    transaction.Commit();

                    var successResponse = req.CreateResponse(HttpStatusCode.OK);
                    await successResponse.WriteStringAsync($"Snippet with ID {id} has been denied and archived.");
                    return successResponse;
                }
            }
        }

    }
}
