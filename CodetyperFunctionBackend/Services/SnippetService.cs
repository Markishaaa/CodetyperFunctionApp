using CodetyperFunctionBackend.Model;
using CodetyperFunctionBackend.Repositories;

namespace CodetyperFunctionBackend.Services
{
    internal class SnippetService
    {
        private readonly SnippetRepository _snippetRepository;
        private readonly UserRepository _userRepository;
        private readonly TaskRepository _taskRepository;

        public SnippetService(SnippetRepository snippetRepository, UserRepository userRepository, TaskRepository taskRepository)
        {
            _snippetRepository = snippetRepository;
            _userRepository = userRepository;
            _taskRepository = taskRepository;
        }

        public async Task<(bool success, string message)> AddSnippetAsync(CodeSnippet snippet)
        {
            if (snippet == null)
            {
                return (false, "Snippet cannot be null.");
            }
            else if (string.IsNullOrEmpty(snippet.Content) || string.IsNullOrEmpty(snippet.LanguageName) || string.IsNullOrEmpty(snippet.CreatorId))
            {
                return (false, "Please provide valid snippet content, language, task ID, and creator ID.");
            }

            await _snippetRepository.AddSnippetAsync(snippet);

            string message = snippet.Shown ? $"Snippet added." : $"Request to add snippet sent.";
            return (true, message);
        }

        public async Task<CodeSnippet?> GetRandomSnippetAsync()
        {
            var snippet = await _snippetRepository.GetRandomSnippetAsync(true);

            if (snippet == null)
            {
                return null;
            }

            return snippet;
        }

        public async Task<(IEnumerable<CodeSnippet> snippets, int totalPages)> GetShownSnippetsWithPagingAsync(int taskId, string? languageName, int page, int pageSize)
        {
            if (page <= 0) page = 1;
            if (pageSize <= 0) pageSize = 10;

            int offset = (page - 1) * pageSize;

            // get total snippets count for pagination
            int totalSnippets = await _snippetRepository.CountShownSnippetsAsync(taskId, languageName);
            int totalPages = (int)Math.Ceiling((double)totalSnippets / pageSize);

            var snippets = await _snippetRepository.GetShownSnippetsWithPagingAsync(taskId, languageName, offset, pageSize);

            return (snippets, totalPages);
        }

        public async Task<(bool success, string message, CodeSnippet? snippet, CodingTask? task, User? creator)> GetRandomSnippetRequestAsync()
        {
            var snippet = await _snippetRepository.GetRandomSnippetAsync(false);
            if (snippet == null)
            {
                return (false, "No snippet requests found.", null, null, null);
            }

            var task = await _taskRepository.GetTaskByIdAsync(snippet.TaskId);
            var creator = await _userRepository.GetUserByIdAsync(snippet.CreatorId);

            return (true, "Random snippet request retrieved successfully.", snippet, task, creator);
        }
    }
}
