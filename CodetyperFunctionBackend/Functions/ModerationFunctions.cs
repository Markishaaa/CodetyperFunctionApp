using CodetyperFunctionBackend.Model;
using CodetyperFunctionBackend.Services;
using CodetyperFunctionBackend.Utils;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.Net;

namespace CodetyperFunctionBackend.Functions
{
    internal class ModerationFunctions
    {
        private readonly ILogger<ModerationFunctions> _logger;
        private readonly TaskService _taskService;
        private readonly SnippetService _snippetService;

        public ModerationFunctions(ILogger<ModerationFunctions> logger, TaskService taskService, SnippetService snippetService)
        {
            _logger = logger;
            _taskService = taskService;
            _snippetService = snippetService;
        }

        [Function("AcceptTaskRequest")]
        public async Task<HttpResponseData> AcceptTaskRequest(
        [HttpTrigger(AuthorizationLevel.User, "put", Route = "tasks/acceptRequest/{id}")] HttpRequestData req, int id)
        {
            if (!AuthHelper.IsUserAuthorized(req, Roles.SuperAdmin, Roles.Admin, Roles.Moderator))
            {
                return await req.CreateResponseAsync(HttpStatusCode.Forbidden, "You do not have permission to perform this action.");
            }

            _logger.LogInformation($"Processing a request to accept task with ID {id}");

            bool success = await _taskService.AcceptTaskRequestAsync(id);
            if (success)
            {
                return await req.CreateResponseAsync(HttpStatusCode.OK, $"Task request accepted.");
            }

            return await req.CreateResponseAsync(HttpStatusCode.NotFound, $"Task with ID {id} not found.");
        }

        [Function("DenyTaskRequest")]
        public async Task<HttpResponseData> DenyRequest(
            [HttpTrigger(AuthorizationLevel.Anonymous, "delete", Route = "tasks/denyRequest/{id}")] HttpRequestData req, int id)
        {
            if (!AuthHelper.IsUserAuthorized(req, Roles.SuperAdmin, Roles.Admin, Roles.Moderator))
            {
                return await req.CreateResponseAsync(HttpStatusCode.Forbidden, "You do not have permission to perform this action.");
            }

            _logger.LogInformation($"Processing a request to deny task with ID {id}");

            var requestBody = await req.ReadAsStringAsync();

            if (string.IsNullOrEmpty(requestBody))
            {
                return await req.CreateResponseAsync(HttpStatusCode.BadRequest, "Request body is empty.");
            }

            dynamic? data = JsonConvert.DeserializeObject(requestBody);
            string? reason = data?.reason;
            string? staffId = data?.staffId;

            var (success, message) = await _taskService.DenyAndArchiveTaskRequestAsync(id, reason!, staffId!);

            if (!success) return await req.CreateResponseAsync(HttpStatusCode.BadRequest, message);

            return await req.CreateResponseAsync(HttpStatusCode.OK, message);
        }


        [Function("AcceptSnippetRequest")]
        public async Task<HttpResponseData> AcceptSnippetRequest(
            [HttpTrigger(AuthorizationLevel.User, "put", Route = "snippets/acceptRequest/{id}")] HttpRequestData req, int id)
        {
            if (!AuthHelper.IsUserAuthorized(req, Roles.SuperAdmin, Roles.Admin, Roles.Moderator))
            {
                return await req.CreateResponseAsync(HttpStatusCode.Forbidden, "You do not have permission to perform this action.");
            }

            _logger.LogInformation($"Processing a request to accept snippet with ID {id}");

            bool success = await _snippetService.AcceptSnippetRequestAsync(id);
            if (success)
            {
                return await req.CreateResponseAsync(HttpStatusCode.OK, $"Snippet request accepted.");
            }

            return await req.CreateResponseAsync(HttpStatusCode.NotFound, $"Task with ID {id} not found.");
        }

        [Function("DenySnippetRequest")]
        public async Task<HttpResponseData> DenySnippetRequest(
            [HttpTrigger(AuthorizationLevel.User, "delete", Route = "snippets/denyRequest/{id}")] HttpRequestData req, int id)
        {
            if (!AuthHelper.IsUserAuthorized(req, Roles.SuperAdmin, Roles.Admin, Roles.Moderator))
            {
                return await req.CreateResponseAsync(HttpStatusCode.Forbidden, "You do not have permission to perform this action.");
            }

            _logger.LogInformation($"Processing a request to deny snippet with ID {id}");

            var requestBody = await req.ReadAsStringAsync();

            if (string.IsNullOrEmpty(requestBody))
            {
                return await req.CreateResponseAsync(HttpStatusCode.BadRequest, "Request body is empty.");
            }

            dynamic? data = JsonConvert.DeserializeObject(requestBody);
            string? reason = data?.reason;
            string? staffId = data?.staffId;

            var (success, message) = await _snippetService.DenyAndArchiveSnippetRequestAsync(id, reason!, staffId!);

            if (!success) return await req.CreateResponseAsync(HttpStatusCode.BadRequest, message);

            return await req.CreateResponseAsync(HttpStatusCode.OK, message);
        }
    }
}
