using CodetyperFunctionBackend.DTOs;
using CodetyperFunctionBackend.Services;
using CodetyperFunctionBackend.Utils;
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
                return await req.CreateResponseAsync(HttpStatusCode.BadRequest, "Request body is empty.");
            }

            var userDto = Newtonsoft.Json.JsonConvert.DeserializeObject<UserDto>(requestBody)!;

            try
            {
                var (success, message) = await _userService.RegisterUserAsync(userDto);

                HttpStatusCode statusCode = success ? HttpStatusCode.Created : HttpStatusCode.BadRequest;
                return await req.CreateResponseAsync(statusCode, message);
            }
            catch
            {
                var message = "Cannot access the database.";
                _logger.LogInformation(message);
                return await req.CreateResponseAsync(HttpStatusCode.GatewayTimeout, message);
            }
        }

        [Function("LoginUser")]
        public async Task<HttpResponseData> LoginUser([HttpTrigger(AuthorizationLevel.Anonymous, "post")] HttpRequestData req)
        {
            _logger.LogInformation("Processing user login.");

            var requestBody = await req.ReadAsStringAsync();

            if (string.IsNullOrEmpty(requestBody))
            {
                return await req.CreateResponseAsync(HttpStatusCode.BadRequest, "Request body is empty.");
            }

            var userDto = Newtonsoft.Json.JsonConvert.DeserializeObject<UserDto>(requestBody)!;

            try
            {
                var result = await _userService.LoginAsync(userDto.Username, userDto.Password!);

                if (!result.Success)
                {
                    return await req.CreateResponseAsync(HttpStatusCode.Unauthorized, result.Message);
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
                return await req.CreateResponseAsync(HttpStatusCode.GatewayTimeout, message);
            }
        }
    }
}
