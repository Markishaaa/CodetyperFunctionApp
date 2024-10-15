using CodetyperFunctionBackend.DTOs;
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

        public async Task<bool> IsUsernameTakenAsync(string username)
        {
            string checkUserQuery = $"SELECT COUNT(1) " +
                $"FROM {User.Fields.TableName} " +
                $"WHERE {User.Fields.Username} = @Username";

            return await _dbService.ExecuteAsync(async conn =>
            {
                var result = await conn.ExecuteScalarAsync<int>(checkUserQuery, new { Username = username });
                return result > 0;
            });
        }

        public async Task AddUserAsync(User user)
        {
            string insertUserQuery = $"INSERT INTO {User.Fields.TableName} ({User.Fields.UserId}, {User.Fields.Username}, " +
                $"{User.Fields.PasswordHash}, {User.Fields.Email}, {User.Fields.RoleName}, " +
                $"{User.Fields.CreatedAt}, {User.Fields.UpdatedAt}) " +
                "VALUES (@UserId, @Username, @PasswordHash, @Email, @RoleName, @CreatedAt, @UpdatedAt)";

            await _dbService.ExecuteAsync(async conn =>
            {
                await conn.ExecuteAsync(insertUserQuery, user);
            });
        }

        public async Task<UserDto?> GetUserByUsernameAsync(string username)
        {
            var query = @$"
                SELECT {User.Fields.PasswordHash}, {User.Fields.RoleName}, {User.Fields.UserId}
                FROM {User.Fields.TableName}  
                WHERE {User.Fields.Username} = @Username";

            var result = await _dbService.ExecuteAsync(async conn =>
            {
                var result = await conn.QuerySingleOrDefaultAsync(query, new { Username = username });
                return result;
            });

            if (result != null)
            {
                return new UserDto
                {
                    UserId = result.UserId,
                    Username = result.Username,
                    PasswordHash = result.PasswordHash,
                    RoleName = result.RoleName
                };
            }

            return null;
        }
    }
}
