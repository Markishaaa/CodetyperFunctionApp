using CodetyperFunctionBackend.Model;
using CodetyperFunctionBackend.Repositories;

namespace CodetyperFunctionBackend.Services
{
    internal class TaskService
    {
        private readonly TaskRepository _taskRepository;

        public TaskService(TaskRepository taskRepository)
        {
            _taskRepository = taskRepository;
        }

        public async Task<(bool success, string message)> AddTaskAsync(CodingTask task)
        {
            if (task == null)
            {
                return (false, "Task cannot be null.");
            }
            else if (string.IsNullOrEmpty(task.Name) || string.IsNullOrEmpty(task.Description))
            {
                return (false, "Task name and description cannot be null or empty.");
            }
            else if (string.IsNullOrEmpty(task.CreatorId))
            {
                return (false, "Creator id cannot be null or empty.");
            }

            await _taskRepository.AddTaskAsync(task.Name, task.Description, task.Shown, task.CreatorId);

            string message = task.Shown ? $"Task '{task.Name}' added." : $"Request to add task '{task.Name}' sent.";
            return (true, message);
        }

        public async Task<(IEnumerable<CodingTask> taks, int totalPages)> GetShownTasksWithPagingAsync(int page, int pageSize)
        {
            if (page <= 0) page = 1;
            if (pageSize <= 0) pageSize = 15;

            int offset = (page - 1) * pageSize;

            // getting totalTasks only to calculate totalPages.. can be passed in response if needed
            int totalTasks = await _taskRepository.CountTasksAsync(true);
            int totalPages = (int)Math.Ceiling((double)totalTasks / pageSize);

            var tasks = await _taskRepository.GetShownTasksWithPagingAsync(offset, pageSize);
            return (tasks, totalPages);
        }

        public async Task<(CodingTask? task, User? creator, int taskCount)> GetRandomTaskRequestAndCountAsync()
        {
            int count = await _taskRepository.CountTasksAsync(false);

            if (count == 0)
                // no tasks found
                return (null, null, count);

            var task = await _taskRepository.GetRandomTaskRequestAsync();
            if (task == null)
                return (null, null, count);

            var creator = await _taskRepository.GetTaskCreatorAsync(task.CreatorId);

            return (task, creator, count);
        }
    }
}
