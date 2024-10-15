using CodetyperFunctionBackend.DTOs;
using CodetyperFunctionBackend.Services;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using System.Net;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace CodetyperFunctionBackend.Functions
{
    internal class AuthFunctions
    {
        private readonly ILogger _logger;
        private readonly UserService _userService;

        public AuthFunctions(ILoggerFactory loggerFactory, UserService userService)
        {
            _logger = loggerFactory.CreateLogger<AuthFunctions>();
            _userService = userService;
        }

        [Function("RegisterUser")]
        public async Task<HttpResponseData> RegisterUser([HttpTrigger(AuthorizationLevel.Anonymous, "post")] HttpRequestData req)
        {
            _logger.LogInformation("Processing user registration.");

            var requestBody = await req.ReadAsStringAsync();

            if (string.IsNullOrEmpty(requestBody))
            {
                var badRequestResponse = req.CreateResponse(HttpStatusCode.BadRequest);
                await badRequestResponse.WriteStringAsync("Request body is empty.");
                return badRequestResponse;
            }

            var userDto = Newtonsoft.Json.JsonConvert.DeserializeObject<UserDto>(requestBody)!;

            try
            {
                var (success, message) = await _userService.RegisterUserAsync(userDto);

                var response = req.CreateResponse(success ? HttpStatusCode.Created : HttpStatusCode.BadRequest);
                await response.WriteStringAsync(message);
                return response;
            }
            catch
            {
                var msg = "Cannot access the database.";
                _logger.LogInformation(msg);
                var timeoutResponse = req.CreateResponse(HttpStatusCode.GatewayTimeout);
                await timeoutResponse.WriteStringAsync(msg);
                return timeoutResponse;
            }
        }

        [Function("LoginUser")]
        public async Task<HttpResponseData> LoginUser([HttpTrigger(AuthorizationLevel.Anonymous, "post")] HttpRequestData req)
        {
            _logger.LogInformation("Processing user login.");

            var requestBody = await req.ReadAsStringAsync();

            if (string.IsNullOrEmpty(requestBody))
            {
                var badRequestResponse = req.CreateResponse(HttpStatusCode.BadRequest);
                await badRequestResponse.WriteStringAsync("Request body is empty.");
                return badRequestResponse;
            }

            var userDto = Newtonsoft.Json.JsonConvert.DeserializeObject<UserDto>(requestBody)!;

            try
            {
                var result = await _userService.LoginAsync(userDto.Username, userDto.Password!);

                if (!result.Success)
                {
                    var unauthorizedResponse = req.CreateResponse(HttpStatusCode.Unauthorized);
                    await unauthorizedResponse.WriteStringAsync(result.Message);
                    return unauthorizedResponse;
                }

                var successResponse = req.CreateResponse(HttpStatusCode.OK);
                successResponse.Headers.Add("Authorization", $"Bearer {result.Token}");
                successResponse.Headers.Add("Access-Control-Expose-Headers", "Authorization");
                await successResponse.WriteStringAsync("Login successful.");
                return successResponse;
            }
            catch
            {
                var message = "Cannot access the database.";
                _logger.LogInformation(message);
                var timeoutResponse = req.CreateResponse(HttpStatusCode.GatewayTimeout);
                await timeoutResponse.WriteStringAsync(message);
                return timeoutResponse;
            }
        }
    }
}
