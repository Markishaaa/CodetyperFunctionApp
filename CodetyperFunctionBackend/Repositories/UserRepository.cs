using CodetyperFunctionBackend.Model;
using CodetyperFunctionBackend.Services;
using Dapper;

namespace CodetyperFunctionBackend.Repositories
{
    internal class UserRepository
    {
        private readonly DatabaseService _dbService;

        public UserRepository(DatabaseService dbService)
        {
            _dbService = dbService;
        }

        public async Task<User?> GetUserByIdAsync(string creatorId)
        {
            string query = $@"
                SELECT {User.Fields.UserId}, {User.Fields.Username}, {User.Fields.RoleName}
                FROM {User.Fields.TableName}
                WHERE UserId = @CreatorId";

            return await _dbService.ExecuteAsync(async conn =>
            {
                return await conn.QueryFirstOrDefaultAsync<User>(query, new { CreatorId = creatorId });
            });
        }
    }
}
