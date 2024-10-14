using CodetyperFunctionBackend.Model;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using System.Data.SqlClient;
using System.Net;

namespace CodetyperFunctionBackend.Functions
{
    public class SnippetFunctions
    {
        private readonly ILogger<SnippetFunctions> _logger;
        private readonly string _connectionString;

        public SnippetFunctions(ILogger<SnippetFunctions> logger)
        {
            _logger = logger;
            _connectionString = Environment.GetEnvironmentVariable("SqlConnectionString")!;
        }

        [Function("AddSnippet")]
        public async Task<HttpResponseData> AddSnippet(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "snippets/add")] HttpRequestData req)
        {
            _logger.LogInformation("Processing a request to add a code snippet.");

            bool isStaff = false;
            if (AuthHelper.IsUserAuthorized(req, Roles.SuperAdmin, Roles.Admin, Roles.Moderator))
            {
                isStaff = true;
            }

            var requestBody = await req.ReadAsStringAsync();
            dynamic? data = Newtonsoft.Json.JsonConvert.DeserializeObject(requestBody);

            string? content = data?.content;
            string? languageName = data?.languageName;
            int? taskId = data?.taskId;
            string? creatorId = data?.creatorId;

            if (string.IsNullOrEmpty(content) || string.IsNullOrEmpty(languageName) || taskId == null || string.IsNullOrEmpty(creatorId))
            {
                var badRequestResponse = req.CreateResponse(HttpStatusCode.BadRequest);
                await badRequestResponse.WriteStringAsync("Please provide valid snippet content, language, task ID, and creator ID.");
                return badRequestResponse;
            }

            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                await conn.OpenAsync();
                string query = @"
                    INSERT INTO Snippets (Content, Shown, LanguageName, TaskId, CreatorId) 
                    VALUES (@content, @shown, @languageName, @taskId, @creatorId)";

                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@content", content);
                    cmd.Parameters.AddWithValue("@shown", isStaff);
                    cmd.Parameters.AddWithValue("@languageName", languageName);
                    cmd.Parameters.AddWithValue("@taskId", taskId);
                    cmd.Parameters.AddWithValue("@creatorId", creatorId);

                    await cmd.ExecuteNonQueryAsync();
                }
            }

            string message;
            if (!isStaff)
            {
                message = $"Snippet request sent.";
            }
            else
            {
                message = $"Snippet added.";
            }

            var response = req.CreateResponse(HttpStatusCode.OK);
            await response.WriteStringAsync(message);
            return response;
        }

        [Function("GetRandomSnippet")]
        public async Task<HttpResponseData> GetRandomSnippet(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "snippets/random")] HttpRequestData req)
        {
            _logger.LogInformation("Fetching a random code snippet.");

            CodeSnippet? snippet = null;
            string? connectionString = Environment.GetEnvironmentVariable("SqlConnectionString");

            if (string.IsNullOrEmpty(connectionString))
            {
                var serverErrorResponse = req.CreateResponse(HttpStatusCode.InternalServerError);
                await serverErrorResponse.WriteStringAsync("Connection string is missing.");
                return serverErrorResponse;
            }

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                await conn.OpenAsync();
                string query = @"
                    SELECT TOP 1 Id, Content, Shown, LanguageName, TaskId, CreatorId 
                    FROM Snippets 
                    WHERE Shown = 1 
                    ORDER BY NEWID()";

                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    using (SqlDataReader reader = await cmd.ExecuteReaderAsync())
                    {
                        if (await reader.ReadAsync())
                        {
                            snippet = new CodeSnippet
                            {
                                Id = reader.GetInt32(reader.GetOrdinal("Id")),
                                Content = reader["Content"].ToString(),
                                Shown = reader.GetBoolean(reader.GetOrdinal("Shown")),
                                LanguageName = reader["LanguageName"].ToString(),
                                TaskId = reader.GetInt32(reader.GetOrdinal("TaskId")),
                                CreatorId = reader["CreatorId"].ToString()
                            };
                        }
                    }
                }
            }

            var response = req.CreateResponse();

            if (snippet == null)
            {
                response.StatusCode = HttpStatusCode.NotFound;
                await response.WriteStringAsync("No snippets available.");
            }
            else
            {
                await response.WriteAsJsonAsync(snippet, HttpStatusCode.OK);
            }

            return response;
        }

        [Function("GetShownSnippets")]
        public async Task<HttpResponseData> GetShownSnippetsWithPaging(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "snippets/shown")] HttpRequestData req)
        {
            var snippets = new List<CodeSnippet>();

            var queryParameters = System.Web.HttpUtility.ParseQueryString(req.Url.Query);
            int page = int.TryParse(queryParameters.Get("page"), out var parsedPage) ? parsedPage : 1;
            int pageSize = int.TryParse(queryParameters.Get("pageSize"), out var parsedPageSize) ? parsedPageSize : 10;
            int taskId = int.TryParse(queryParameters.Get("taskId"), out var parsedTaskId) ? parsedTaskId : 0;
            string? languageName = queryParameters.Get("languageName");

            int offset = (page - 1) * pageSize;
            int totalSnippets = 0;
            int totalPages = 0;

            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                await conn.OpenAsync();

                string countQuery = @"
                    SELECT COUNT(*) 
                    FROM Snippets 
                    WHERE Shown = 1 AND TaskId = @TaskId";

                if (!string.IsNullOrEmpty(languageName))
                {
                    countQuery += " AND LanguageName = @LanguageName";
                }

                using (SqlCommand countCmd = new SqlCommand(countQuery, conn))
                {
                    countCmd.Parameters.AddWithValue("@TaskId", taskId);
                    if (!string.IsNullOrEmpty(languageName))
                    {
                        countCmd.Parameters.AddWithValue("@LanguageName", languageName);
                    }

                    totalSnippets = (int)await countCmd.ExecuteScalarAsync();
                }

                totalPages = (int)Math.Ceiling((double)totalSnippets / pageSize);

                string query = @"
                    SELECT Id, Content, CreatorId, LanguageName 
                    FROM Snippets 
                    WHERE Shown = 1 AND TaskId = @TaskId";

                if (!string.IsNullOrEmpty(languageName))
                {
                    query += " AND LanguageName = @LanguageName";
                }

                query += " ORDER BY Id OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY";

                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@TaskId", taskId);
                    cmd.Parameters.AddWithValue("@Offset", offset);
                    cmd.Parameters.AddWithValue("@PageSize", pageSize);

                    if (!string.IsNullOrEmpty(languageName))
                    {
                        cmd.Parameters.AddWithValue("@LanguageName", languageName);
                    }

                    using (SqlDataReader reader = await cmd.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            var snippet = new CodeSnippet
                            {
                                Id = reader.GetInt32(reader.GetOrdinal("Id")),
                                Content = reader["Content"]?.ToString(),
                                LanguageName = reader["LanguageName"]?.ToString(),
                                CreatorId = reader["CreatorId"]?.ToString()
                            };

                            snippets.Add(snippet);
                        }
                    }
                }
            }

            var response = req.CreateResponse(HttpStatusCode.OK);
            await response.WriteAsJsonAsync(new
            {
                snippets = snippets,
                currentPage = page,
                pageSize = pageSize,
                totalPages = totalPages
            });

            return response;
        }

        [Function("GetRandomSnippetRequest")]
        public async Task<HttpResponseData> GetRandomSnippetRequest(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "snippets/randomRequest")] HttpRequestData req)
        {
            if (string.IsNullOrEmpty(_connectionString))
            {
                var errorResponse = req.CreateResponse(HttpStatusCode.InternalServerError);
                await errorResponse.WriteAsJsonAsync(new
                {
                    success = false,
                    message = "Database connection string is missing."
                });
                return errorResponse;
            }

            CodeSnippet? snippet = null;
            CodingTask? task = null;
            User? creator = null;

            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                await conn.OpenAsync();

                // Get a random snippet where shown = false
                string snippetQuery = @"
                    SELECT TOP 1 Id, Content, Shown, LanguageName, TaskId, CreatorId
                    FROM Snippets
                    WHERE Shown = 0
                    ORDER BY NEWID()";

                using (SqlCommand snippetCmd = new SqlCommand(snippetQuery, conn))
                using (SqlDataReader snippetReader = await snippetCmd.ExecuteReaderAsync())
                {
                    if (await snippetReader.ReadAsync())
                    {
                        snippet = new CodeSnippet
                        {
                            Id = snippetReader.GetInt32(snippetReader.GetOrdinal("Id")),
                            Content = snippetReader["Content"]?.ToString(),
                            Shown = snippetReader.GetBoolean(snippetReader.GetOrdinal("Shown")),
                            LanguageName = snippetReader["LanguageName"]?.ToString(),
                            TaskId = snippetReader.GetInt32(snippetReader.GetOrdinal("TaskId")),
                            CreatorId = snippetReader["CreatorId"]?.ToString()
                        };
                    }
                }

                if (snippet == null)
                {
                    var notFoundResponse = req.CreateResponse(HttpStatusCode.NotFound);
                    await notFoundResponse.WriteAsJsonAsync(new
                    {
                        success = false,
                        message = "No snippet requests found."
                    });
                    return notFoundResponse;
                }

                string taskQuery = @"
                    SELECT Name, Description
                    FROM Tasks
                    WHERE Id = @taskId";

                using (SqlCommand taskCmd = new SqlCommand(taskQuery, conn))
                {
                    taskCmd.Parameters.AddWithValue("@taskId", snippet.TaskId);

                    using (SqlDataReader taskReader = await taskCmd.ExecuteReaderAsync())
                    {
                        if (await taskReader.ReadAsync())
                        {
                            task = new CodingTask
                            {
                                Name = taskReader["Name"]?.ToString(),
                                Description = taskReader["Description"]?.ToString()
                            };
                        }
                    }
                }

                string userQuery = @"
                    SELECT UserId, Username, RoleName
                    FROM Users
                    WHERE UserId = @creatorId";

                using (SqlCommand userCmd = new SqlCommand(userQuery, conn))
                {
                    userCmd.Parameters.AddWithValue("@creatorId", snippet.CreatorId);

                    using (SqlDataReader userReader = await userCmd.ExecuteReaderAsync())
                    {
                        if (await userReader.ReadAsync())
                        {
                            creator = new User
                            {
                                UserId = userReader["UserId"]?.ToString(),
                                Username = userReader["Username"]?.ToString(),
                                RoleName = userReader["RoleName"]?.ToString()
                            };
                        }
                    }
                }
            }

            var response = req.CreateResponse(HttpStatusCode.OK);
            await response.WriteAsJsonAsync(new
            {
                success = true,
                message = "Random snippet request retrieved successfully.",
                snippet = snippet,
                task = task,
                creator = creator
            });

            return response;
        }

    }
}
