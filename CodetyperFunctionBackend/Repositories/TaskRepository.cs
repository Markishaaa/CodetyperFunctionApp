using CodetyperFunctionBackend.Model;
using CodetyperFunctionBackend.Services;
using Dapper;

namespace CodetyperFunctionBackend.Repositories
{
    internal class TaskRepository
    {
        private readonly DatabaseService _dbService;

        public TaskRepository(DatabaseService dbService)
        {
            _dbService = dbService;
        }

        public async Task AddTaskAsync(string name, string description, bool shown, string creatorId)
        {
            var addQuery = $"INSERT INTO {CodingTask.Fields.TableName} " +
                $"({CodingTask.Fields.Name}, {CodingTask.Fields.Description}, {CodingTask.Fields.Shown}, {CodingTask.Fields.CreatorId}) " +
                $"VALUES (@Name, @Description, @Shown, @CreatorId)";

            var parameters = new
            {
                Name = name,
                Description = description,
                Shown = shown,
                CreatorId = creatorId
            };

            await _dbService.ExecuteAsync(conn => conn.ExecuteAsync(addQuery, parameters));
        }

        public async Task<int> CountTasksAsync(bool shown)
        {
            int showTask = shown ? 1 : 0;
            string countQuery = $"SELECT COUNT(*) " +
                $"FROM {CodingTask.Fields.TableName} " +
                $"WHERE {CodingTask.Fields.Shown} = {showTask}";

            return await _dbService.ExecuteAsync(async conn =>
            {
                return await conn.ExecuteScalarAsync<int>(countQuery);
            });
        }

        public async Task<IEnumerable<CodingTask>> GetShownTasksWithPagingAsync(int offset, int pageSize)
        {
            string query = $"SELECT {CodingTask.Fields.Id}, {CodingTask.Fields.Name}, " +
                $"{CodingTask.Fields.Description}, {CodingTask.Fields.CreatorId} " +
                $"FROM {CodingTask.Fields.TableName} " +
                $"WHERE {CodingTask.Fields.Shown} = 1 " +
                $"ORDER BY {CodingTask.Fields.Id} " +
                $"OFFSET @Offset ROWS " +
                $"FETCH NEXT @PageSize ROWS ONLY";

            return await _dbService.ExecuteAsync(async conn =>
            {
                return await conn.QueryAsync<CodingTask>(query, new { Offset = offset, PageSize = pageSize });
            });
        }

        public async Task<CodingTask?> GetRandomTaskRequestAsync()
        {
            string randomQuery = @$"
                SELECT TOP 1 
                    {CodingTask.Fields.Id}, 
                    {CodingTask.Fields.Name}, 
                    {CodingTask.Fields.Description}, 
                    {CodingTask.Fields.CreatorId} 
                FROM {CodingTask.Fields.TableName} 
                WHERE {CodingTask.Fields.Shown} = 0 
                ORDER BY NEWID()";

            return await _dbService.ExecuteAsync(async conn =>
            {
                return await conn.QueryFirstOrDefaultAsync<CodingTask>(randomQuery);
            });
        }

        public async Task<User?> GetTaskCreatorAsync(string creatorId)
        {
            string creatorQuery = @$"SELECT 
                    {User.Fields.UserId}, {User.Fields.Username}, {User.Fields.Email} 
                FROM {User.Fields.TableName} 
                WHERE {User.Fields.UserId} = @CreatorId";

            return await _dbService.ExecuteAsync(async conn =>
            {
                return await conn.QueryFirstOrDefaultAsync<User>(creatorQuery, new { CreatorId = creatorId });
            });
        }
    }
}
