using CodetyperFunctionBackend.Model;
using CodetyperFunctionBackend.Services;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using System.Net;

namespace CodetyperFunctionBackend.Functions
{
    internal class TaskFunctions
    {
        private readonly ILogger _logger;
        private readonly TaskService _taskService;

        public TaskFunctions(ILoggerFactory loggerFactory, TaskService taskService)
        {
            _logger = loggerFactory.CreateLogger<LanguageFunctions>();
            _taskService = taskService;
        }

        [Function("AddTask")]
        public async Task<HttpResponseData> AddTask(
            [HttpTrigger(AuthorizationLevel.User, "post", Route = "tasks/add")] HttpRequestData req)
        {
            _logger.LogInformation("Processing a request to add a task.");

            bool isStaff = AuthHelper.IsUserAuthorized(req, Roles.SuperAdmin, Roles.Admin, Roles.Moderator);

            var requestBody = await req.ReadAsStringAsync();

            if (string.IsNullOrEmpty(requestBody))
            {
                var badRequestResponse = req.CreateResponse(HttpStatusCode.BadRequest);
                await badRequestResponse.WriteStringAsync("Request body is empty.");
                return badRequestResponse;
            }

            var taskDto = Newtonsoft.Json.JsonConvert.DeserializeObject<CodingTask>(requestBody);

            if (taskDto != null)
                taskDto.Shown = isStaff;

            var (success, message) = await _taskService.AddTaskAsync(taskDto!);

            var response = req.CreateResponse(success ? HttpStatusCode.OK : HttpStatusCode.BadRequest);
            await response.WriteStringAsync(message);
            return response;
        }

        [Function("GetShownTasks")]
        public async Task<HttpResponseData> GetShownTasksWithPaging([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "tasks/shown")] HttpRequestData req)
        {
            _logger.LogInformation("Processing a request to get shown tasks with pagination.");

            var queryParameters = System.Web.HttpUtility.ParseQueryString(req.Url.Query);
            int page = int.TryParse(queryParameters.Get("page"), out var parsedPage) ? parsedPage : 1;
            int pageSize = int.TryParse(queryParameters.Get("pageSize"), out var parsedPageSize) ? parsedPageSize : 15;

            var (tasks, totalPages) = await _taskService.GetShownTasksWithPagingAsync(page, pageSize);

            var response = req.CreateResponse(HttpStatusCode.OK);
            await response.WriteAsJsonAsync(new
            {
                tasks,
                currentPage = page,
                pageSize,
                totalPages
            });
            return response;
        }

        [Function("GetRandomTaskRequest")]
        public async Task<HttpResponseData> GetRandomTaskRequestAndCount([HttpTrigger(AuthorizationLevel.User, "get", Route = "tasks/randomRequest")] HttpRequestData req)
        {
            _logger.LogInformation("Processing a request to get a random task request.");

            if (!AuthHelper.IsUserAuthorized(req, Roles.SuperAdmin, Roles.Admin, Roles.Moderator))
            {
                var forbiddenResponse = req.CreateResponse(HttpStatusCode.Forbidden);
                await forbiddenResponse.WriteStringAsync("You do not have permission to perform this action.");
                return forbiddenResponse;
            }

            var (task, creator, count) = await _taskService.GetRandomTaskRequestAndCountAsync();

            if (task == null || creator == null)
            {
                var errorResponse = req.CreateResponse(HttpStatusCode.NotFound);
                await errorResponse.WriteStringAsync("No task requests found.");
                return errorResponse;
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