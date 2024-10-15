using System.Data.SqlClient;

namespace CodetyperFunctionBackend.Services
{
    internal class DatabaseService
    {
        private readonly string _connectionString;

        public DatabaseService()
        {
            _connectionString = Environment.GetEnvironmentVariable("SqlConnectionString")!;
        }

        public async Task<TResult> ExecuteAsync<TResult>(Func<SqlConnection, Task<TResult>> action)
        {
            using (var conn = new SqlConnection(_connectionString))
            {
                await conn.OpenAsync();
                return await action(conn);
            }
        }

        public async Task ExecuteAsync(Func<SqlConnection, Task> action)
        {
            using (var conn = new SqlConnection(_connectionString))
            {
                await conn.OpenAsync();
                await action(conn);
            }
        }
    }
}
