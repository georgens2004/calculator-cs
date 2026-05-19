using System.Text.Json;

namespace TaskHubApp
{
    enum TaskPriority
    {
        Low,
        Medium,
        High
    }

    enum TaskStatus
    {
        New,
        InProgress,
        Done
    }

    class TaskItem
    {
        public int Id { get; set; }
        public string Title { get; set; } = "";
        public string Description { get; set; } = "";
        public TaskPriority Priority { get; set; }
        public DateTime Deadline { get; set; }
        public TaskStatus Status { get; set; }

        public bool IsOverdue()
        {
            return Status != TaskStatus.Done && Deadline < DateTime.Now;
        }

        public override string ToString()
        {
            return $"[{Id}] {Title} | {Description} | {Priority} | {Deadline:dd.MM.yyyy HH:mm} | {Status}";
        }
    }

    class Repository<T> where T : class
    {
        private readonly List<T> _items = new();

        public List<T> GetAll()
        {
            return new List<T>(_items);
        }

        public void Add(T item)
        {
            _items.Add(item);
        }

        public bool Remove(Predicate<T> predicate)
        {
            var item = _items.Find(predicate);
            if (item == null)
            {
                return false;
            }

            _items.Remove(item);
            return true;
        }

        public T? Find(Predicate<T> predicate)
        {
            return _items.Find(predicate);
        }

        public List<T> FindAll(Predicate<T> predicate)
        {
            return _items.FindAll(predicate);
        }

        public void ReplaceAll(IEnumerable<T> items)
        {
            _items.Clear();
            _items.AddRange(items);
        }
    }

    static class ConsoleHelper
    {
        public static void PrintHeader(string text)
        {
            Console.WriteLine();
            Console.WriteLine(text);
        }

        public static void PrintTasks(List<TaskItem> tasks)
        {
            if (tasks.Count == 0)
            {
                Console.WriteLine("No tasks found.");
                return;
            }

            foreach (var task in tasks)
            {
                Console.WriteLine(task);
            }
        }

        public static string ReadRequiredString(string prompt)
        {
            while (true)
            {
                Console.Write(prompt);
                var input = Console.ReadLine();
                if (!string.IsNullOrWhiteSpace(input))
                {
                    return input.Trim();
                }

                Console.WriteLine("Value cannot be empty.");
            }
        }

        public static int ReadInt(string prompt)
        {
            while (true)
            {
                Console.Write(prompt);
                if (int.TryParse(Console.ReadLine(), out int value))
                {
                    return value;
                }

                Console.WriteLine("Enter a valid number.");
            }
        }

        public static DateTime ReadDateTime(string prompt)
        {
            while (true)
            {
                Console.Write(prompt);
                if (DateTime.TryParse(Console.ReadLine(), out DateTime value))
                {
                    return value;
                }

                Console.WriteLine("Enter a valid date.");
            }
        }

        public static TEnum ReadEnum<TEnum>(string prompt) where TEnum : struct, Enum
        {
            while (true)
            {
                Console.Write($"{prompt} ({string.Join(", ", Enum.GetNames(typeof(TEnum)))}): ");
                var input = Console.ReadLine();
                if (Enum.TryParse<TEnum>(input, true, out var value))
                {
                    return value;
                }

                Console.WriteLine("Enter one of the available values.");
            }
        }
    }

    class TaskStatistics
    {
        public int TotalCount { get; set; }
        public int DoneCount { get; set; }
        public int OverdueCount { get; set; }
        public Dictionary<TaskPriority, int> PriorityCounts { get; set; } = new();
    }

    class TaskStorage
    {
        public async Task SaveAsync(string path, List<TaskItem> tasks)
        {
            await using var stream = File.Create(path);
            await JsonSerializer.SerializeAsync(stream, tasks, new JsonSerializerOptions { WriteIndented = true });
        }

        public async Task<List<TaskItem>> LoadAsync(string path)
        {
            await using var stream = File.OpenRead(path);
            var tasks = await JsonSerializer.DeserializeAsync<List<TaskItem>>(stream);
            return tasks ?? new List<TaskItem>();
        }
    }

    class DeadlineMonitor : IDisposable
    {
        private readonly Func<List<TaskItem>> _tasksProvider;
        private readonly CancellationTokenSource _cancellationTokenSource = new();
        private readonly Task _backgroundTask;

        public DeadlineMonitor(Func<List<TaskItem>> tasksProvider)
        {
            _tasksProvider = tasksProvider;
            _backgroundTask = Task.Run(CheckDeadlinesAsync);
        }

        private async Task CheckDeadlinesAsync()
        {
            while (!_cancellationTokenSource.Token.IsCancellationRequested)
            {
                var tasks = _tasksProvider();
                var overdueTasks = tasks.FindAll(task => task.IsOverdue());
                foreach (var task in overdueTasks)
                {
                    Console.WriteLine();
                    Console.WriteLine($"Notification: task \"{task.Title}\" is overdue.");
                }

                try
                {
                    await Task.Delay(5000, _cancellationTokenSource.Token);
                }
                catch (TaskCanceledException)
                {
                    break;
                }
            }
        }

        public void Dispose()
        {
            _cancellationTokenSource.Cancel();
            try
            {
                _backgroundTask.Wait();
            }
            catch (AggregateException)
            {
            }
            _cancellationTokenSource.Dispose();
        }
    }

    class TaskManager
    {
        private readonly Repository<TaskItem> _repository = new();
        private int _nextId = 1;

        public List<TaskItem> GetAllTasks()
        {
            return _repository.GetAll();
        }

        public void CreateTask(string title, string description, TaskPriority priority, DateTime deadline, TaskStatus status)
        {
            _repository.Add(new TaskItem
            {
                Id = _nextId++,
                Title = title,
                Description = description,
                Priority = priority,
                Deadline = deadline,
                Status = status
            });
        }

        public bool DeleteTask(int id)
        {
            return _repository.Remove(task => task.Id == id);
        }

        public TaskItem? GetTaskById(int id)
        {
            return _repository.Find(task => task.Id == id);
        }

        public List<TaskItem> Search(Predicate<TaskItem> predicate)
        {
            return _repository.FindAll(predicate);
        }

        public TaskStatistics GetStatistics()
        {
            var tasks = _repository.GetAll();
            var statistics = new TaskStatistics
            {
                TotalCount = tasks.Count,
                DoneCount = tasks.Count(task => task.Status == TaskStatus.Done),
                OverdueCount = tasks.Count(task => task.IsOverdue())
            };

            foreach (TaskPriority priority in Enum.GetValues(typeof(TaskPriority)))
            {
                statistics.PriorityCounts[priority] = tasks.Count(task => task.Priority == priority);
            }

            return statistics;
        }

        public void ReplaceTasks(List<TaskItem> tasks)
        {
            _repository.ReplaceAll(tasks);
            _nextId = tasks.Count == 0 ? 1 : tasks.Max(task => task.Id) + 1;
        }
    }

    class Program
    {
        static async Task Main()
        {
            var manager = new TaskManager();
            var storage = new TaskStorage();
            using var monitor = new DeadlineMonitor(manager.GetAllTasks);
            bool running = true;

            while (running)
            {
                try
                {
                    PrintMenu();
                    int choice = ConsoleHelper.ReadInt("Choose an action: ");
                    switch (choice)
                    {
                        case 1:
                            CreateTask(manager);
                            break;
                        case 2:
                            ShowTasks(manager);
                            break;
                        case 3:
                            EditTask(manager);
                            break;
                        case 4:
                            DeleteTask(manager);
                            break;
                        case 5:
                            SearchTasks(manager);
                            break;
                        case 6:
                            ShowStatistics(manager);
                            break;
                        case 7:
                            await SaveTasksAsync(manager, storage);
                            break;
                        case 8:
                            await LoadTasksAsync(manager, storage);
                            break;
                        case 0:
                            running = false;
                            break;
                        default:
                            Console.WriteLine("Unknown menu item.");
                            break;
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error: {ex.Message}");
                }
            }
        }

        static void PrintMenu()
        {
            ConsoleHelper.PrintHeader("TaskHub");
            Console.WriteLine("1. Create task");
            Console.WriteLine("2. View tasks");
            Console.WriteLine("3. Edit task");
            Console.WriteLine("4. Delete task");
            Console.WriteLine("5. Search tasks");
            Console.WriteLine("6. Statistics");
            Console.WriteLine("7. Save to file");
            Console.WriteLine("8. Load from file");
            Console.WriteLine("0. Exit");
        }

        static void CreateTask(TaskManager manager)
        {
            ConsoleHelper.PrintHeader("Create Task");
            var title = ConsoleHelper.ReadRequiredString("Title: ");
            var description = ConsoleHelper.ReadRequiredString("Description: ");
            var priority = ConsoleHelper.ReadEnum<TaskPriority>("Priority");
            var deadline = ConsoleHelper.ReadDateTime("Deadline: ");
            var status = ConsoleHelper.ReadEnum<TaskStatus>("Status");
            manager.CreateTask(title, description, priority, deadline, status);
            Console.WriteLine("Task created.");
        }

        static void ShowTasks(TaskManager manager)
        {
            ConsoleHelper.PrintHeader("View Tasks");
            Console.WriteLine("1. All tasks");
            Console.WriteLine("2. Completed");
            Console.WriteLine("3. Not completed");
            Console.WriteLine("4. High priority");
            int choice = ConsoleHelper.ReadInt("Choose a filter: ");
            List<TaskItem> tasks = choice switch
            {
                1 => manager.GetAllTasks(),
                2 => manager.Search(task => task.Status == TaskStatus.Done),
                3 => manager.Search(task => task.Status != TaskStatus.Done),
                4 => manager.Search(task => task.Priority == TaskPriority.High),
                _ => new List<TaskItem>()
            };
            ConsoleHelper.PrintTasks(tasks);
        }

        static void EditTask(TaskManager manager)
        {
            ConsoleHelper.PrintHeader("Edit Task");
            int id = ConsoleHelper.ReadInt("Enter task id: ");
            var task = manager.GetTaskById(id);
            if (task == null)
            {
                Console.WriteLine("Task not found.");
                return;
            }

            task.Title = ConsoleHelper.ReadRequiredString("New title: ");
            task.Description = ConsoleHelper.ReadRequiredString("New description: ");
            task.Priority = ConsoleHelper.ReadEnum<TaskPriority>("New priority");
            task.Status = ConsoleHelper.ReadEnum<TaskStatus>("New status");
            Console.WriteLine("Task updated.");
        }

        static void DeleteTask(TaskManager manager)
        {
            ConsoleHelper.PrintHeader("Delete Task");
            int id = ConsoleHelper.ReadInt("Enter task id: ");
            Console.WriteLine(manager.DeleteTask(id) ? "Task deleted." : "Task not found.");
        }

        static void SearchTasks(TaskManager manager)
        {
            ConsoleHelper.PrintHeader("Search Tasks");
            Console.WriteLine("1. By title");
            Console.WriteLine("2. By status");
            Console.WriteLine("3. By priority");
            int choice = ConsoleHelper.ReadInt("Choose search type: ");
            List<TaskItem> tasks = new();
            switch (choice)
            {
                case 1:
                    var title = ConsoleHelper.ReadRequiredString("Enter part of the title: ");
                    tasks = manager.Search(task => task.Title.Contains(title, StringComparison.OrdinalIgnoreCase));
                    break;
                case 2:
                    var status = ConsoleHelper.ReadEnum<TaskStatus>("Status");
                    tasks = manager.Search(task => task.Status == status);
                    break;
                case 3:
                    var priority = ConsoleHelper.ReadEnum<TaskPriority>("Priority");
                    tasks = manager.Search(task => task.Priority == priority);
                    break;
                default:
                    Console.WriteLine("Unknown search type.");
                    return;
            }
            ConsoleHelper.PrintTasks(tasks);
        }

        static void ShowStatistics(TaskManager manager)
        {
            ConsoleHelper.PrintHeader("Statistics");
            var statistics = manager.GetStatistics();
            Console.WriteLine($"Total tasks: {statistics.TotalCount}");
            Console.WriteLine($"Completed: {statistics.DoneCount}");
            Console.WriteLine($"Overdue: {statistics.OverdueCount}");
            foreach (var pair in statistics.PriorityCounts)
            {
                Console.WriteLine($"{pair.Key}: {pair.Value}");
            }
        }

        static async Task SaveTasksAsync(TaskManager manager, TaskStorage storage)
        {
            ConsoleHelper.PrintHeader("Save");
            var path = ConsoleHelper.ReadRequiredString("Enter file name: ");
            await storage.SaveAsync(path, manager.GetAllTasks());
            Console.WriteLine("Tasks saved.");
        }

        static async Task LoadTasksAsync(TaskManager manager, TaskStorage storage)
        {
            ConsoleHelper.PrintHeader("Load");
            var path = ConsoleHelper.ReadRequiredString("Enter file name: ");
            if (!File.Exists(path))
            {
                Console.WriteLine("File not found.");
                return;
            }

            var tasks = await storage.LoadAsync(path);
            manager.ReplaceTasks(tasks);
            Console.WriteLine("Tasks loaded.");
        }
    }
}
