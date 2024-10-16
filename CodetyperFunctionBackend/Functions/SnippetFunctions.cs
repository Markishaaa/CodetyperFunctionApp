using CodetyperFunctionBackend.Model;
using CodetyperFunctionBackend.Services;
using CodetyperFunctionBackend.Utils;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using System.Net;

namespace CodetyperFunctionBackend.Functions
{
    internal class SnippetFunctions
    {
        private readonly ILogger<SnippetFunctions> _logger;
        private readonly SnippetService _snippetService;

        public SnippetFunctions(ILogger<SnippetFunctions> logger, SnippetService snippetService)
        {
            _logger = logger;
            _snippetService = snippetService;
        }

        [Function("AddSnippet")]
        public async Task<HttpResponseData> AddSnippet(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "snippets/add")] HttpRequestData req)
        {
            _logger.LogInformation("Processing a request to add a code snippet.");

            bool isStaff = AuthHelper.IsUserAuthorized(req, Roles.SuperAdmin, Roles.Admin, Roles.Moderator);

            var requestBody = await req.ReadAsStringAsync();

            if (string.IsNullOrEmpty(requestBody))
            {
                return await req.CreateResponseAsync(HttpStatusCode.BadRequest, "Request body is empty.");
            }

            var snippetDto = Newtonsoft.Json.JsonConvert.DeserializeObject<CodeSnippet>(requestBody);
            if (snippetDto != null)
                snippetDto.Shown = isStaff;

            var (success, message) = await _snippetService.AddSnippetAsync(snippetDto!);

            return await req.CreateResponseAsync(success ? HttpStatusCode.OK : HttpStatusCode.BadRequest, message);
        }

        [Function("GetRandomSnippet")]
        public async Task<HttpResponseData> GetRandomSnippet(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "snippets/random")] HttpRequestData req)
        {
            _logger.LogInformation("Fetching a random code snippet.");

            var snippet = await _snippetService.GetRandomSnippetAsync();

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
            _logger.LogInformation("Processing a request to get shown snippets with pagination.");

            var queryParameters = System.Web.HttpUtility.ParseQueryString(req.Url.Query);
            int page = int.TryParse(queryParameters.Get("page"), out var parsedPage) ? parsedPage : 1;
            int pageSize = int.TryParse(queryParameters.Get("pageSize"), out var parsedPageSize) ? parsedPageSize : 10;
            int taskId = int.TryParse(queryParameters.Get("taskId"), out var parsedTaskId) ? parsedTaskId : 0;
            string? languageName = queryParameters.Get("languageName");

            var (snippets, totalPages) = await _snippetService.GetShownSnippetsWithPagingAsync(taskId, languageName, page, pageSize);

            var response = req.CreateResponse(HttpStatusCode.OK);
            await response.WriteAsJsonAsync(new
            {
                snippets,
                currentPage = page,
                pageSize,
                totalPages
            });

            return response;
        }

        [Function("GetRandomSnippetRequest")]
        public async Task<HttpResponseData> GetRandomSnippetRequest(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "snippets/randomRequest")] HttpRequestData req)
        {
            if (!AuthHelper.IsUserAuthorized(req, Roles.SuperAdmin, Roles.Admin, Roles.Moderator))
            {
                return await req.CreateResponseAsync(HttpStatusCode.Forbidden, "You do not have permission to perform this action.");
            }

            _logger.LogInformation("Processing a request to a get random snippet request.");

            var (success, message, snippet, task, creator) = await _snippetService.GetRandomSnippetRequestAsync();

            var response = req.CreateResponse(success ? HttpStatusCode.OK : HttpStatusCode.NotFound);
            await response.WriteAsJsonAsync(new
            {
                success,
                message,
                snippet,
                task,
                creator
            });

            return response;
        }

    }
}
