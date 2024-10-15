using CodetyperFunctionBackend.Model;
using CodetyperFunctionBackend.Services;
using Dapper;

namespace CodetyperFunctionBackend.Repositories
{
    internal class LanguageRepository
    {
        private readonly DatabaseService _dbService;

        public LanguageRepository(DatabaseService dbService)
        {
            _dbService = dbService;
        }

        public async Task<bool> LanguageExistsAsync(string languageName)
        {
            string checkQuery = $"SELECT COUNT(1) " +
                $"FROM {Language.Fields.TableName} " +
                $"WHERE LOWER({Language.Fields.Name}) = LOWER(@Name)";

            return await _dbService.ExecuteAsync(async conn =>
            {
                var result = await conn.ExecuteScalarAsync<int>(checkQuery, new { Name = languageName });
                return result > 0;
            });
        }

        public async Task AddLanguageAsync(string languageName)
        {
            var addQuery = $"INSERT INTO {Language.Fields.TableName} ({Language.Fields.Name}) VALUES (@Name)";

            await _dbService.ExecuteAsync(async conn =>
            {
                await conn.ExecuteAsync(addQuery, new { Name = languageName });
            });
        }

        public async Task<IEnumerable<Language>> GetAllLanguagesAsync()
        {
            string getAllQuery = $"SELECT {Language.Fields.Name} FROM {Language.Fields.TableName}";

            return await _dbService.ExecuteAsync(async conn =>
            {
                var result = await conn.QueryAsync<Language>(getAllQuery);
                return result;
            });
        }

        public async Task<Language?> getLanguageByNameAsync(string languageName)
        {
            string getByNameQuery = $"SELECT {Language.Fields.Name} " +
                $"FROM {Language.Fields.TableName} " +
                $"WHERE LOWER({Language.Fields.Name}) = LOWER(@Name)";

            return await _dbService.ExecuteAsync(async conn =>
            {
                var result = await conn.QueryFirstOrDefaultAsync<Language>(getByNameQuery, new { Name = languageName });
                return result;
            });
        }
    }
}
