using CodetyperFunctionBackend.Model;
using CodetyperFunctionBackend.Services;
using CodetyperFunctionBackend.Utils;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using System.Net;

namespace CodetyperFunctionBackend.Functions
{
    internal class LanguageFunctions
    {
        private readonly ILogger _logger;
        private readonly LanguageService _languageService;

        public LanguageFunctions(ILoggerFactory loggerFactory, LanguageService languageService)
        {
            _logger = loggerFactory.CreateLogger<LanguageFunctions>();
            _languageService = languageService;
        }

        [Function("AddLanguage")]
        public async Task<HttpResponseData> AddLanguage([HttpTrigger(AuthorizationLevel.User, "post", Route = null)] HttpRequestData req)
        {
            _logger.LogInformation("Processing a request to add a language.");

            if (!AuthHelper.IsUserAuthorized(req, Roles.SuperAdmin, Roles.Admin))
            {
                return await req.CreateResponseAsync(HttpStatusCode.Forbidden, "You do not have permission to perform this action.");
            }

            var requestBody = await req.ReadAsStringAsync();

            if (string.IsNullOrEmpty(requestBody))
            {
                return await req.CreateResponseAsync(HttpStatusCode.BadRequest, "Request body is empty.");
            }

            var languageDto = Newtonsoft.Json.JsonConvert.DeserializeObject<Language>(requestBody);

            var (success, message) = await _languageService.AddLanguageAsync(languageDto!);

            HttpStatusCode statusCode = success ? HttpStatusCode.Created : HttpStatusCode.BadRequest;
            return await req.CreateResponseAsync(statusCode, message);
        }

        [Function("GetAllLanguages")]
        public async Task<HttpResponseData> GetAllLanguages([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "languages/getAll")] HttpRequestData req)
        {
            _logger.LogInformation("Processing a request to get all languages.");

            var languages = await _languageService.GetAllLanguagesAsync();

            var response = req.CreateResponse(HttpStatusCode.OK);

            if (!languages.Any())
            {
                await response.WriteAsJsonAsync(new { message = "No languages found", data = languages });
                return response;
            }

            await response.WriteAsJsonAsync(languages);
            return response;
        }

        [Function("GetLanguageByName")]
        public async Task<HttpResponseData> GetLanguageByName([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "languages/get/{languageName}")] HttpRequestData req, string languageName)
        {
            _logger.LogInformation($"Processing a request to get language by name {languageName}.");

            var language = await _languageService.GetLanguageByNameAsync(languageName);

            var response = req.CreateResponse();

            if (language == null)
            {
                response.StatusCode = HttpStatusCode.NotFound;
                await response.WriteStringAsync($"Language with name '{languageName}' not found.");
            }
            else
            {
                await response.WriteAsJsonAsync(language, HttpStatusCode.OK);
            }

            return response;
        }
    }
}
