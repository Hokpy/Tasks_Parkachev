using System.Text.Json;
using TaskManager.Core.Enums;
using TaskManager.Core.Models;

namespace TaskManager.Core.Services
{
    /// <summary>
    /// Сервис управления задачами.
    /// Содержит всю бизнес-логику: CRUD, поиск, фильтрация, сортировка,
    /// сохранение/загрузка в JSON, статистика.
    /// </summary>
    public class TaskService
    {
        // Внутреннее хранилище задач
        private readonly List<TaskItem> _tasks = new();

        // Настройки сериализатора JSON: читаемый формат, русские перечисления как строки
        private static readonly JsonSerializerOptions _jsonOptions = new()
        {
            WriteIndented = true,
            Converters = { new System.Text.Json.Serialization.JsonStringEnumConverter() }
        };

        // ─────────────────────────────────────────────
        //  CRUD-операции
        // ─────────────────────────────────────────────

        /// <summary>
        /// Добавляет новую задачу в список.
        /// Если Id не задан — генерируется автоматически.
        /// </summary>
        /// <param name="task">Задача для добавления</param>
        /// <exception cref="ArgumentNullException">Если task == null</exception>
        /// <exception cref="ArgumentException">Если Title пустой</exception>
        public void AddTask(TaskItem task)
        {
            if (task is null)
                throw new ArgumentNullException(nameof(task), "Задача не может быть null.");

            if (string.IsNullOrWhiteSpace(task.Title))
                throw new ArgumentException("Название задачи не может быть пустым.", nameof(task));

            // Гарантируем уникальный Id
            if (task.Id == Guid.Empty)
                task.Id = Guid.NewGuid();

            _tasks.Add(task);
        }

        /// <summary>
        /// Возвращает копию списка всех задач.
        /// </summary>
        public IReadOnlyList<TaskItem> GetAllTasks() => _tasks.AsReadOnly();

        /// <summary>
        /// Обновляет существующую задачу по Id.
        /// </summary>
        /// <param name="task">Обновлённые данные задачи</param>
        /// <exception cref="ArgumentNullException">Если task == null</exception>
        /// <exception cref="KeyNotFoundException">Если задача с таким Id не найдена</exception>
        public void UpdateTask(TaskItem task)
        {
            if (task is null)
                throw new ArgumentNullException(nameof(task));

            int index = _tasks.FindIndex(t => t.Id == task.Id);
            if (index == -1)
                throw new KeyNotFoundException($"Задача с Id={task.Id} не найдена.");

            _tasks[index] = task;
        }

        /// <summary>
        /// Удаляет задачу по уникальному идентификатору.
        /// </summary>
        /// <param name="id">Id задачи</param>
        /// <exception cref="KeyNotFoundException">Если задача не найдена</exception>
        public void DeleteTask(Guid id)
        {
            var task = _tasks.FirstOrDefault(t => t.Id == id)
                       ?? throw new KeyNotFoundException($"Задача с Id={id} не найдена.");
            _tasks.Remove(task);
        }

        /// <summary>
        /// Ищет задачу по Id и возвращает её, или null если не найдена.
        /// </summary>
        public TaskItem? GetById(Guid id) => _tasks.FirstOrDefault(t => t.Id == id);

        // ─────────────────────────────────────────────
        //  Фильтрация и поиск (LINQ)
        // ─────────────────────────────────────────────

        /// <summary>
        /// Фильтрует задачи по статусу.
        /// </summary>
        /// <param name="status">Нужный статус</param>
        public IEnumerable<TaskItem> FilterByStatus(Enums.WorkStatus status) =>
            _tasks.Where(t => t.Status == status);

        /// <summary>
        /// Ищет задачи по подстроке в названии или описании (без учёта регистра).
        /// </summary>
        /// <param name="query">Поисковый запрос</param>
        public IEnumerable<TaskItem> SearchTasks(string query)
        {
            if (string.IsNullOrWhiteSpace(query))
                return _tasks.AsReadOnly();

            // Приводим запрос к нижнему регистру для сравнения без учёта регистра
            string lowerQuery = query.ToLowerInvariant();

            return _tasks.Where(t =>
                t.Title.ToLowerInvariant().Contains(lowerQuery) ||
                t.Description.ToLowerInvariant().Contains(lowerQuery));
        }

        /// <summary>
        /// Возвращает только важные задачи.
        /// </summary>
        public IEnumerable<TaskItem> GetImportantTasks() =>
            _tasks.Where(t => t.IsImportant);

        /// <summary>
        /// Возвращает просроченные задачи (срок прошёл, статус не Completed).
        /// </summary>
        public IEnumerable<TaskItem> GetOverdueTasks() =>
            _tasks.Where(t => t.IsOverdue);

        // ─────────────────────────────────────────────
        //  Сортировка
        // ─────────────────────────────────────────────

        /// <summary>
        /// Сортирует задачи по приоритету (High → Medium → Low).
        /// </summary>
        public IEnumerable<TaskItem> SortByPriority() =>
            _tasks.OrderByDescending(t => t.Priority);

        /// <summary>
        /// Сортирует задачи по сроку выполнения (ближайшие — первые).
        /// </summary>
        public IEnumerable<TaskItem> SortByDueDate() =>
            _tasks.OrderBy(t => t.DueDate);

        // ─────────────────────────────────────────────
        //  Статистика
        // ─────────────────────────────────────────────

        /// <summary>
        /// Вычисляет и возвращает статистику по всем задачам.
        /// </summary>
        public TaskStatistics GetStatistics() => new TaskStatistics
        {
            Total      = _tasks.Count,
            Completed  = _tasks.Count(t => t.Status == Enums.WorkStatus.Completed),
            InProgress = _tasks.Count(t => t.Status == Enums.WorkStatus.InProgress),
            New        = _tasks.Count(t => t.Status == Enums.WorkStatus.New),
            Overdue    = _tasks.Count(t => t.IsOverdue),
            Important  = _tasks.Count(t => t.IsImportant)
        };

        // ─────────────────────────────────────────────
        //  Сохранение и загрузка (JSON)
        // ─────────────────────────────────────────────

        /// <summary>
        /// Сохраняет все задачи в JSON-файл по указанному пути.
        /// </summary>
        /// <param name="path">Полный путь к файлу (напр. "tasks.json")</param>
        /// <exception cref="ArgumentException">Если путь пустой</exception>
        public void SaveToFile(string path)
        {
            if (string.IsNullOrWhiteSpace(path))
                throw new ArgumentException("Путь к файлу не может быть пустым.", nameof(path));

            // Сериализуем список задач в JSON и записываем файл
            string json = JsonSerializer.Serialize(_tasks, _jsonOptions);
            File.WriteAllText(path, json);
        }

        /// <summary>
        /// Загружает задачи из JSON-файла, заменяя текущий список.
        /// </summary>
        /// <param name="path">Полный путь к файлу</param>
        /// <exception cref="ArgumentException">Если путь пустой</exception>
        /// <exception cref="FileNotFoundException">Если файл не существует</exception>
        /// <exception cref="InvalidDataException">Если файл содержит некорректный JSON</exception>
        public void LoadFromFile(string path)
        {
            if (string.IsNullOrWhiteSpace(path))
                throw new ArgumentException("Путь к файлу не может быть пустым.", nameof(path));

            if (!File.Exists(path))
                throw new FileNotFoundException($"Файл не найден: {path}");

            string json = File.ReadAllText(path);

            // Десериализуем JSON в список задач
            var loaded = JsonSerializer.Deserialize<List<TaskItem>>(json, _jsonOptions)
                         ?? throw new InvalidDataException("Не удалось прочитать данные из файла.");

            // Заменяем текущий список загруженными задачами
            _tasks.Clear();
            _tasks.AddRange(loaded);
        }

        /// <summary>
        /// Очищает весь список задач (используется для тестов).
        /// </summary>
        public void Clear() => _tasks.Clear();
    }
}
