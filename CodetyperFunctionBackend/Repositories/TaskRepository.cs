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

        public async Task AddTaskAsync(CodingTask task)
        {
            var addQuery = $"INSERT INTO {CodingTask.Fields.TableName} " +
                $"({CodingTask.Fields.Name}, {CodingTask.Fields.Description}, {CodingTask.Fields.Shown}, {CodingTask.Fields.CreatorId}) " +
                $"VALUES (@Name, @Description, @Shown, @CreatorId)";

            await _dbService.ExecuteAsync(conn => conn.ExecuteAsync(addQuery, task));
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

        public async Task<CodingTask?> GetTaskByIdAsync(int taskId)
        {
            string query = @$"SELECT
                    {CodingTask.Fields.Name}, {CodingTask.Fields.Description}, {CodingTask.Fields.CreatorId} 
                FROM {CodingTask.Fields.TableName}
                WHERE Id = @TaskId";

            return await _dbService.ExecuteAsync(async conn =>
            {
                return await conn.QueryFirstOrDefaultAsync<CodingTask>(query, new { TaskId = taskId });
            });
        }

        public async Task<int> AcceptTaskAsync(int id)
        {
            var query = $"UPDATE {CodingTask.Fields.TableName} " +
                $"SET {CodingTask.Fields.Shown} = @shown " +
                $"WHERE {CodingTask.Fields.Id} = @id";

            return await _dbService.ExecuteAsync(async conn =>
            {
                return await conn.ExecuteAsync(query, new { id, shown = true });
            });
        }

        public async Task ArchiveTaskAsync(ArchivedTask archivedTask)
        {
            var query = $@"
                INSERT INTO {ArchivedTask.Fields.TableName} (
                    {ArchivedTask.Fields.Name}, {ArchivedTask.Fields.Description}, {ArchivedTask.Fields.DeniedAt}, 
                    {ArchivedTask.Fields.Reason}, {ArchivedTask.Fields.CreatorId}, {ArchivedTask.Fields.StaffId}) 
                VALUES (@Name, @Description, @DeniedAt, @Reason, @CreatorId, @StaffId)";

            await _dbService.ExecuteAsync(async conn =>
            {
                await conn.ExecuteAsync(query, archivedTask);
            });
        }

        public async Task DeleteTaskAsync(int id)
        {
            var query = $"DELETE FROM {CodingTask.Fields.TableName} WHERE {CodingTask.Fields.Id} = @id";

            await _dbService.ExecuteAsync(async conn =>
            {
                await conn.ExecuteAsync(query, new { id });
            });
        }
    }
}
