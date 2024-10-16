using Microsoft.Azure.Functions.Worker.Http;
using System.Net;

namespace CodetyperFunctionBackend.Utils
{
    internal static class HttpResponseExtensions
    {
        public static async Task<HttpResponseData> CreateResponseAsync(this HttpRequestData req, HttpStatusCode statusCode, string message)
        {
            var response = req.CreateResponse(statusCode);
            await response.WriteStringAsync(message);
            return response;
        }
    }
}
