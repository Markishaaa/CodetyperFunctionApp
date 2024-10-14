using CodetyperFunctionBackend.Model;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using System.Data.SqlClient;
using System.Net;

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
            [HttpTrigger(AuthorizationLevel.User, "post", Route = "tasks/add")] HttpRequestData req)
        {
            _logger.LogInformation("Processing a request to add a task.");

            bool isStaff = false;
            if (AuthHelper.IsUserAuthorized(req, Roles.SuperAdmin, Roles.Admin, Roles.Moderator))
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
            string message;
            if (isStaff)
            {
                message = $"Task '{taskName}' added.";
            }
            else
            {
                message = $"Request to add task '{taskName}' sent.";
            }
            await response.WriteStringAsync(message);
            return response;
        }

        [Function("GetShownTasks")]
        public async Task<HttpResponseData> GetShownTasksWithPaging([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "tasks/shown")] HttpRequestData req)
        {
            var tasks = new List<CodingTask>();

            // query parameters from request
            var queryParameters = System.Web.HttpUtility.ParseQueryString(req.Url.Query);
            int page = int.TryParse(queryParameters.Get("page"), out var parsedPage) ? parsedPage : 1;
            int pageSize = int.TryParse(queryParameters.Get("pageSize"), out var parsedPageSize) ? parsedPageSize : 15;

            int offset = (page - 1) * pageSize;
            int totalTasks = 0;
            int totalPages = 0;

            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                await conn.OpenAsync();

                string countQuery = "SELECT COUNT(*) FROM Tasks WHERE Shown = 1";

                using (SqlCommand countCmd = new SqlCommand(countQuery, conn))
                {
                    totalTasks = (int)await countCmd.ExecuteScalarAsync();
                }

                totalPages = (int)Math.Ceiling((double)totalTasks / pageSize);

                string query = @"
                    SELECT Id, Name, Description, CreatorId
                    FROM Tasks 
                    WHERE Shown = 1 
                    ORDER BY Id 
                    OFFSET @Offset ROWS 
                    FETCH NEXT @PageSize ROWS ONLY";

                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@Offset", offset);
                    cmd.Parameters.AddWithValue("@PageSize", pageSize);

                    using (SqlDataReader reader = await cmd.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            var task = new CodingTask
                            {
                                Id = reader.GetInt32(reader.GetOrdinal("Id")),
                                Name = reader["Name"]?.ToString(),
                                Description = reader["Description"]?.ToString(),
                                CreatorId = reader["CreatorId"]?.ToString()
                            };

                            tasks.Add(task);
                        }
                    }
                }
            }

            var response = req.CreateResponse(HttpStatusCode.OK);
            await response.WriteAsJsonAsync(new
            {
                tasks = tasks,
                currentPage = page,
                pageSize = pageSize,
                totalPages = totalPages
            });
            return response;
        }

        [Function("GetRandomTaskRequest")]
        public async Task<HttpResponseData> GetRandomTaskRequestAndCount([HttpTrigger(AuthorizationLevel.User, "get", Route = "tasks/randomRequest")] HttpRequestData req)
        {
            if (!AuthHelper.IsUserAuthorized(req, Roles.SuperAdmin, Roles.Admin, Roles.Moderator))
            {
                var forbiddenResponse = req.CreateResponse(HttpStatusCode.Forbidden);
                await forbiddenResponse.WriteStringAsync("You do not have permission to perform this action.");
                return forbiddenResponse;
            }

            var task = new CodingTask();
            var creator = new User();
            int count = 0;

            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                await conn.OpenAsync();

                // Query to get the count of tasks with Shown = 0
                string countQuery = "SELECT COUNT(*) FROM Tasks WHERE Shown = 0";
                using (SqlCommand countCmd = new SqlCommand(countQuery, conn))
                {
                    count = (int)await countCmd.ExecuteScalarAsync();
                }

                if (count == 0)
                {
                    var errorResponse = req.CreateResponse(HttpStatusCode.NotFound);
                    await errorResponse.WriteStringAsync("No task requests found.");
                    return errorResponse;
                }

                // Query to randomly select a task that has Shown = 0
                string randomQuery = @"
                     SELECT TOP 1 
                        t.Id AS TaskId,                    -- Task's ID
                        t.Name AS TaskName,                -- Task's Name
                        t.Description AS TaskDescription,  -- Task's Description
                        t.CreatorId AS CreatorId,          -- Creator ID in Task table
                        u.UserId AS UserId,                -- User's ID (from Users table)
                        u.Username,                        -- User's Username
                        u.Email                            -- User's Email
                    FROM Tasks t
                    JOIN Users u ON t.CreatorId = u.UserId -- Join Tasks with Users on CreatorId
                    WHERE t.Shown = 0
                    ORDER BY NEWID()";


                using (SqlCommand randomCmd = new SqlCommand(randomQuery, conn))
                {
                    using (SqlDataReader reader = await randomCmd.ExecuteReaderAsync())
                    {
                        if (await reader.ReadAsync())
                        {
                            // Fill the task information
                            task.Id = reader.GetInt32(reader.GetOrdinal("TaskId"));
                            task.Name = reader["TaskName"]?.ToString();
                            task.Description = reader["TaskDescription"]?.ToString();
                            task.CreatorId = reader["CreatorId"]?.ToString();

                            // Fill the creator information
                            creator.UserId = reader["UserId"]?.ToString();
                            creator.Username = reader["Username"]?.ToString();
                            creator.Email = reader["Email"]?.ToString();
                        }
                    }
                }
            }

            var responseData = new
            {
                Task = task,
                Creator = creator,
                Count = count
            };

            var response = req.CreateResponse(HttpStatusCode.OK);
            await response.WriteAsJsonAsync(responseData);
            return response;
        }

    }
}