using CodetyperFunctionBackend.Model;
using CodetyperFunctionBackend.Services;
using Dapper;

namespace CodetyperFunctionBackend.Repositories
{
    internal class SnippetRepository
    {
        private readonly DatabaseService _dbService;

        public SnippetRepository(DatabaseService dbService)
        {
            _dbService = dbService;
        }

        public async Task AddSnippetAsync(CodeSnippet snippet)
        {
            var addQuery = $"INSERT INTO {CodeSnippet.Fields.TableName} " +
                $"({CodeSnippet.Fields.Content}, {CodeSnippet.Fields.Shown}, " +
                $"{CodeSnippet.Fields.LanguageName}, {CodeSnippet.Fields.TaskId}, {CodeSnippet.Fields.CreatorId}) " +
                $"VALUES (@Content, @Shown, @LanguageName, @TaskId, @CreatorId)";

            await _dbService.ExecuteAsync(conn => conn.ExecuteAsync(addQuery, snippet));
        }

        public async Task<CodeSnippet?> GetRandomSnippetAsync(bool shown)
        {
            int isStaff = shown ? 1 : 0;
            string query = $"SELECT TOP 1 {CodeSnippet.Fields.Id}, {CodeSnippet.Fields.Content}, {CodeSnippet.Fields.Shown}, " +
                $"{CodeSnippet.Fields.LanguageName}, {CodeSnippet.Fields.TaskId}, {CodeSnippet.Fields.CreatorId} " +
                $"FROM {CodeSnippet.Fields.TableName} " +
                $"WHERE {CodeSnippet.Fields.Shown} = {isStaff} " +
                $"ORDER BY NEWID()";

            return await _dbService.ExecuteAsync(async conn =>
            {
                var snippet = await conn.QueryFirstOrDefaultAsync<CodeSnippet>(query);
                return snippet;
            });
        }

        public async Task<IEnumerable<CodeSnippet>> GetShownSnippetsWithPagingAsync(int taskId, string? languageName, int offset, int pageSize)
        {
            string query = $@"
                SELECT {CodeSnippet.Fields.Id}, {CodeSnippet.Fields.Content}, {CodeSnippet.Fields.CreatorId}, {CodeSnippet.Fields.LanguageName}
                FROM {CodeSnippet.Fields.TableName}
                WHERE {CodeSnippet.Fields.Shown} = 1 AND {CodeSnippet.Fields.TaskId} = @TaskId";

            if (!string.IsNullOrEmpty(languageName))
            {
                query += $" AND {CodeSnippet.Fields.LanguageName} = @LanguageName";
            }

            query += $" ORDER BY {CodeSnippet.Fields.Id} OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY";

            return await _dbService.ExecuteAsync(async conn =>
            {
                return await conn.QueryAsync<CodeSnippet>(query, new { TaskId = taskId, LanguageName = languageName, Offset = offset, PageSize = pageSize });
            });
        }

        public async Task<int> CountShownSnippetsAsync(int taskId, string? languageName)
        {
            string countQuery = $@"
            SELECT COUNT(*) 
            FROM {CodeSnippet.Fields.TableName}
            WHERE {CodeSnippet.Fields.Shown} = 1 AND {CodeSnippet.Fields.TaskId} = @TaskId";

            if (!string.IsNullOrEmpty(languageName))
            {
                countQuery += $" AND {CodeSnippet.Fields.LanguageName} = @LanguageName";
            }

            return await _dbService.ExecuteAsync(async conn =>
            {
                return await conn.ExecuteScalarAsync<int>(countQuery, new { TaskId = taskId, LanguageName = languageName });
            });
        }

        public async Task<int> AcceptSnippetAsync(int snippetId)
        {
            string query = $"UPDATE {CodeSnippet.Fields.TableName} " +
                $"SET {CodeSnippet.Fields.Shown} = @shown " +
                $"WHERE {CodeSnippet.Fields.Id} = @Id";

            return await _dbService.ExecuteAsync(async conn =>
            {
                return await conn.ExecuteAsync(query, new { shown = true, Id = snippetId });
            });
        }

        public async Task<CodeSnippet?> GetSnippetByIdAsync(int snippetId)
        {
            string query = @$"SELECT
                    {CodeSnippet.Fields.Content}, {CodeSnippet.Fields.LanguageName}, 
                    {CodeSnippet.Fields.TaskId}, {CodeSnippet.Fields.CreatorId} 
                FROM {CodeSnippet.Fields.TableName}
                WHERE Id = @SnippetId";

            return await _dbService.ExecuteAsync(async conn =>
            {
                return await conn.QueryFirstOrDefaultAsync<CodeSnippet>(query, new { SnippetId = snippetId });
            });
        }

        public async Task ArchiveSnippetAsync(ArchivedSnippet archivedSnippet)
        {
            string archiveQuery = @$"
                INSERT INTO {ArchivedSnippet.Fields.TableName} (
                    {ArchivedSnippet.Fields.Content}, {ArchivedSnippet.Fields.DeniedAt}, {ArchivedSnippet.Fields.LanguageName}, 
                    {ArchivedSnippet.Fields.TaskId}, {ArchivedSnippet.Fields.CreatorId}, {ArchivedSnippet.Fields.StaffId})
                VALUES (@Content, @DeniedAt, @LanguageName, @TaskId, @CreatorId, @StaffId)";

            await _dbService.ExecuteAsync(async conn =>
            {
                await conn.ExecuteAsync(archiveQuery, archivedSnippet);
            });
        }

        public async Task<bool> DeleteSnippetAsync(int snippetId)
        {
            string deleteQuery = $"DELETE FROM {CodeSnippet.Fields.TableName} " +
                $"WHERE {CodeSnippet.Fields.Id} = @id";

            return await _dbService.ExecuteAsync(async conn =>
            {
                var result = await conn.ExecuteAsync(deleteQuery, new { id = snippetId });
                return result > 0;
            });
        }
    }
}
