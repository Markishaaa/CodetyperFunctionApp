using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Azure.Functions.Worker;
using System.Net;
using Microsoft.Extensions.Logging;
using System.Data.SqlClient;
using CodetyperFunctionBackend.Model;

namespace CodetyperFunctionBackend.Functions
{
    internal class TaskFunctions
    {
        private readonly ILogger _logger;
        private readonly string _connectionString;

        public TaskFunctions(ILoggerFactory loggerFactory)
        {
            _logger = loggerFactory.CreateLogger<LanguageFunctions>();
            _connectionString = Environment.GetEnvironmentVariable("SqlConnectionString")!;
        }

        [Function("AddTask")]
        public async Task<HttpResponseData> AddTask(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "tasks/add")] HttpRequestData req)
        {
            _logger.LogInformation("Processing a request to add a task.");

            bool isStaff = false;
            if (AuthHelper.IsUserAuthorized(req, "SuperAdmin", "Admin", "Moderator"))
            {
                isStaff = true;
            }

            var requestBody = await req.ReadAsStringAsync();
            dynamic? data = Newtonsoft.Json.JsonConvert.DeserializeObject(requestBody);
            string? taskName = data?.name;
            string? taskDescription = data?.description;
            string? creatorId = data?.creatorId;

            if (string.IsNullOrEmpty(taskName) || string.IsNullOrEmpty(taskDescription))
            {
                var badRequestResponse = req.CreateResponse(HttpStatusCode.BadRequest);
                await badRequestResponse.WriteStringAsync("Please pass both task name and description in the request body.");
                return badRequestResponse;
            }

            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                await conn.OpenAsync();
                var query = "INSERT INTO Tasks (Name, Description, Shown, CreatorId) VALUES (@Name, @Description, @Shown, @CreatorId)";

                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@Name", taskName);
                    cmd.Parameters.AddWithValue("@Description", taskDescription);
                    cmd.Parameters.AddWithValue("@Shown", isStaff);
                    cmd.Parameters.AddWithValue("@CreatorId", creatorId);

                    await cmd.ExecuteNonQueryAsync();
                }
            }

            var response = req.CreateResponse(HttpStatusCode.OK);
            await response.WriteStringAsync($"Task '{taskName}' added to the database.");
            return response;
        }

        [Function("GetShownTasks")]
        public async Task<HttpResponseData> GetShownTasks([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "tasks/shown")] HttpRequestData req)
        {
            var tasks = new List<CodingTask>();

            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                await conn.OpenAsync();
                string query = "SELECT Id, Name, Description FROM Tasks WHERE Shown = 1";

                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    using (SqlDataReader reader = await cmd.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            var task = new CodingTask
                            {
                                Id = reader.GetInt32(reader.GetOrdinal("Id")),
                                Name = reader["Name"]?.ToString(),
                                Description = reader["Description"]?.ToString(),
                            };

                            tasks.Add(task);
                        }
                    }
                }
            }

            var response = req.CreateResponse(HttpStatusCode.OK);
            await response.WriteAsJsonAsync(tasks);
            return response;
        }

        [Function("GetTaskByIdWithSnippetsForLanguage")]
        public async Task<HttpResponseData> GetTaskByIdWithSnippetsForLanguage(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "tasks/{id}/snippets/{language}")] HttpRequestData req, int id, string language)
        {
            CodingTask task = null;

            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                await conn.OpenAsync();

                // First query to get task details
                string taskQuery = "SELECT Id, Name, Description, Shown, CreatorId FROM Tasks WHERE Id = @Id AND Shown = 1";

                using (SqlCommand cmd = new SqlCommand(taskQuery, conn))
                {
                    cmd.Parameters.AddWithValue("@Id", id);

                    using (SqlDataReader reader = await cmd.ExecuteReaderAsync())
                    {
                        if (await reader.ReadAsync())
                        {
                            task = new CodingTask
                            {
                                Id = reader.GetInt32(reader.GetOrdinal("Id")),
                                Name = reader["Name"]?.ToString(),
                                Description = reader["Description"]?.ToString(),
                                Shown = reader.GetBoolean(reader.GetOrdinal("Shown")),
                                CreatorId = reader["CreatorId"]?.ToString(),
                                CodeSnippets = new List<CodeSnippet>()
                            };
                        }
                    }
                }

                if (task == null)
                {
                    var notFoundResponse = req.CreateResponse(HttpStatusCode.NotFound);
                    await notFoundResponse.WriteStringAsync($"Task with ID '{id}' not found or not shown.");
                    return notFoundResponse;
                }

                // Second query to get associated code snippets, filtered by the selected language
                string snippetQuery = "SELECT Id, Snippet, LanguageName FROM CodeSnippets WHERE TaskId = @TaskId AND LanguageName = @LanguageName";

                using (SqlCommand cmd = new SqlCommand(snippetQuery, conn))
                {
                    cmd.Parameters.AddWithValue("@TaskId", task.Id);
                    cmd.Parameters.AddWithValue("@LanguageName", language);

                    using (SqlDataReader reader = await cmd.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            var snippet = new CodeSnippet
                            {
                                Id = reader.GetInt32(reader.GetOrdinal("Id")),
                                Content = reader["Content"]?.ToString(),
                                LanguageName = reader["LanguageName"]?.ToString()
                            };

                            task.CodeSnippets.Add(snippet);
                        }
                    }
                }
            }

            var response = req.CreateResponse(HttpStatusCode.OK);
            await response.WriteAsJsonAsync(task);
            return response;
        }
    }
}
