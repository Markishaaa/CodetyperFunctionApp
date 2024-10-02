using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using System.Data.SqlClient;
using System.Net;

namespace CodetyperFunctionBackend.Functions
{
    internal class LanguageFunction
    {
        private readonly ILogger _logger;

        public LanguageFunction(ILoggerFactory loggerFactory)
        {
            _logger = loggerFactory.CreateLogger<LanguageFunction>();
        }

        [Function("AddLanguage")]
        public async Task<HttpResponseData> Run([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = null)] HttpRequestData req)
        {
            _logger.LogInformation("Processing a request to add a language.");

            if (!AuthHelper.IsUserAuthorized(req, "SuperAdmin", "Admin"))
            {
                var forbiddenResponse = req.CreateResponse(HttpStatusCode.Forbidden);
                await forbiddenResponse.WriteStringAsync("You do not have permission to perform this action.");
                return forbiddenResponse;
            }

            var requestBody = await req.ReadAsStringAsync();

            dynamic? data = Newtonsoft.Json.JsonConvert.DeserializeObject(requestBody);
            string? languageName = data?.name;

            if (string.IsNullOrEmpty(languageName))
            {
                var badRequestResponse = req.CreateResponse(HttpStatusCode.BadRequest);
                await badRequestResponse.WriteStringAsync("Please pass a language name in the request body.");
                return badRequestResponse;
            }

            string? connectionString = Environment.GetEnvironmentVariable("SqlConnectionString");

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();
                var text = "INSERT INTO Languages (name) VALUES (@name)";

                using (SqlCommand cmd = new SqlCommand(text, conn))
                {
                    cmd.Parameters.AddWithValue("@name", languageName);
                    cmd.ExecuteNonQuery();
                }
            }

            var response = req.CreateResponse(HttpStatusCode.OK);
            await response.WriteStringAsync($"Language {languageName} added to the database.");
            return response;
        }
    }
}
