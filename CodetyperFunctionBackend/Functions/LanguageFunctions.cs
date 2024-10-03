using CodetyperFunctionBackend.Model;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using System.Data.SqlClient;
using System.Net;

namespace CodetyperFunctionBackend.Functions
{
    internal class LanguageFunctions
    {
        private readonly ILogger _logger;
        private readonly string _connectionString;

        public LanguageFunctions(ILoggerFactory loggerFactory)
        {
            _logger = loggerFactory.CreateLogger<LanguageFunctions>();
            _connectionString = Environment.GetEnvironmentVariable("SqlConnectionString")!;
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

        [Function("GetAllLanguages")]
        public async Task<HttpResponseData> GetAllLanguages([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "languages/getAll")] HttpRequestData req)
        {
            var languages = new List<Language>();

            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                await conn.OpenAsync();
                string query = "SELECT Name FROM Languages";

                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    using (SqlDataReader reader = await cmd.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            var language = new Language
                            {
                                Name = reader["Name"]?.ToString()
                            };

                            if (!string.IsNullOrEmpty(language.Name))
                            {
                                languages.Add(language);
                            }
                        }
                    }
                }
            }

            var response = req.CreateResponse(HttpStatusCode.OK);
            await response.WriteAsJsonAsync(languages);
            return response;
        }

        [Function("GetLanguageByName")]
        public async Task<HttpResponseData> GetLanguageByName([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "languages/get/{languageName}")] HttpRequestData req, string languageName)
        {
            Language language = null;

            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                await conn.OpenAsync();
                string query = "SELECT Name FROM Languages WHERE Name = @Name";

                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@Name", languageName);

                    using (SqlDataReader reader = await cmd.ExecuteReaderAsync())
                    {
                        if (await reader.ReadAsync())
                        {
                            language = new Language
                            {
                                Name = reader["Name"].ToString(),
                            };
                        }
                    }
                }
            }

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
