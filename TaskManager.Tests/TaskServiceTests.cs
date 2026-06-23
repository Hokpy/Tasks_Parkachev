using TaskManager.Core.Enums;
using TaskManager.Core.Models;
using TaskManager.Core.Services;
using Xunit;

namespace TaskManager.Tests
{
    /// <summary>
    /// Модульные тесты для класса TaskService.
    /// Каждый тест проверяет один конкретный сценарий поведения.
    /// </summary>
    public class TaskServiceTests
    {
        // ─────────────────────────────────────────────
        //  Вспомогательный метод: создаёт чистый сервис
        // ─────────────────────────────────────────────

        /// <summary>Фабричный метод — возвращает новый пустой TaskService</summary>
        private static TaskService CreateService() => new TaskService();

        /// <summary>Создаёт тестовую задачу с заданным названием</summary>
        private static TaskItem MakeTask(string title = "Тест",
                                         TaskPriority priority = TaskPriority.Medium,
                                         Core.Enums.WorkStatus status = Core.Enums.WorkStatus.New,
                                         bool isImportant = false,
                                         DateTime? dueDate = null) => new TaskItem
        {
            Title       = title,
            Description = $"Описание для «{title}»",
            Priority    = priority,
            Status      = status,
            IsImportant = isImportant,
            DueDate     = dueDate ?? DateTime.Today.AddDays(3)
        };

        // ─────────────────────────────────────────────
        //  1. Добавление задач
        // ─────────────────────────────────────────────

        [Fact(DisplayName = "Добавление задачи увеличивает счётчик на 1")]
        public void AddTask_IncreasesCount()
        {
            var service = CreateService();
            service.AddTask(MakeTask("Задача 1"));
            Assert.Single(service.GetAllTasks());
        }

        [Fact(DisplayName = "Добавление нескольких задач сохраняет все")]
        public void AddTask_MultipleTasksAreAllStored()
        {
            var service = CreateService();
            service.AddTask(MakeTask("Задача А"));
            service.AddTask(MakeTask("Задача Б"));
            service.AddTask(MakeTask("Задача В"));
            Assert.Equal(3, service.GetAllTasks().Count);
        }

        [Fact(DisplayName = "Добавление задачи с пустым названием бросает исключение")]
        public void AddTask_EmptyTitle_ThrowsArgumentException()
        {
            var service = CreateService();
            var task = MakeTask("");
            Assert.Throws<ArgumentException>(() => service.AddTask(task));
        }

        [Fact(DisplayName = "Добавление null бросает ArgumentNullException")]
        public void AddTask_Null_ThrowsArgumentNullException()
        {
            var service = CreateService();
            Assert.Throws<ArgumentNullException>(() => service.AddTask(null!));
        }

        [Fact(DisplayName = "Задаче автоматически присваивается уникальный Id")]
        public void AddTask_AutoAssignsId()
        {
            var service = CreateService();
            var task = new TaskItem { Title = "Без Id", Id = Guid.Empty };
            service.AddTask(task);
            Assert.NotEqual(Guid.Empty, service.GetAllTasks()[0].Id);
        }

        // ─────────────────────────────────────────────
        //  2. Удаление задач
        // ─────────────────────────────────────────────

        [Fact(DisplayName = "Удаление задачи по Id уменьшает список")]
        public void DeleteTask_RemovesCorrectTask()
        {
            var service = CreateService();
            var task1 = MakeTask("Оставить");
            var task2 = MakeTask("Удалить");
            service.AddTask(task1);
            service.AddTask(task2);

            service.DeleteTask(task2.Id);

            var all = service.GetAllTasks();
            Assert.Single(all);
            Assert.Equal("Оставить", all[0].Title);
        }

        [Fact(DisplayName = "Удаление несуществующей задачи бросает KeyNotFoundException")]
        public void DeleteTask_NonExisting_ThrowsKeyNotFoundException()
        {
            var service = CreateService();
            Assert.Throws<KeyNotFoundException>(() => service.DeleteTask(Guid.NewGuid()));
        }

        [Fact(DisplayName = "После удаления единственной задачи список пуст")]
        public void DeleteTask_LastTask_ListBecomesEmpty()
        {
            var service = CreateService();
            var task = MakeTask();
            service.AddTask(task);
            service.DeleteTask(task.Id);
            Assert.Empty(service.GetAllTasks());
        }

        // ─────────────────────────────────────────────
        //  3. Обновление задач
        // ─────────────────────────────────────────────

        [Fact(DisplayName = "UpdateTask изменяет поля существующей задачи")]
        public void UpdateTask_ChangesFields()
        {
            var service = CreateService();
            var task = MakeTask("Старое название");
            service.AddTask(task);

            task.Title    = "Новое название";
            task.Priority = TaskPriority.High;
            service.UpdateTask(task);

            var updated = service.GetById(task.Id);
            Assert.NotNull(updated);
            Assert.Equal("Новое название", updated!.Title);
            Assert.Equal(TaskPriority.High, updated.Priority);
        }

        [Fact(DisplayName = "UpdateTask для несуществующей задачи бросает KeyNotFoundException")]
        public void UpdateTask_NonExisting_ThrowsKeyNotFoundException()
        {
            var service = CreateService();
            var task = MakeTask();
            // Не добавляем задачу в сервис — сразу пытаемся обновить
            Assert.Throws<KeyNotFoundException>(() => service.UpdateTask(task));
        }

        // ─────────────────────────────────────────────
        //  4. Поиск задач
        // ─────────────────────────────────────────────

        [Fact(DisplayName = "SearchTasks находит задачи по части названия")]
        public void SearchTasks_ByTitle_ReturnsMatching()
        {
            var service = CreateService();
            service.AddTask(MakeTask("Купить молоко"));
            service.AddTask(MakeTask("Позвонить маме"));
            service.AddTask(MakeTask("Купить хлеб"));

            var result = service.SearchTasks("купить").ToList();

            Assert.Equal(2, result.Count);
            Assert.All(result, t => Assert.Contains("Купить", t.Title,
                StringComparison.OrdinalIgnoreCase));
        }

        [Fact(DisplayName = "SearchTasks находит задачи по части описания")]
        public void SearchTasks_ByDescription_ReturnsMatching()
        {
            var service = CreateService();
            var task = new TaskItem
            {
                Title       = "Задача 1",
                Description = "Срочно нужно доделать проект"
            };
            service.AddTask(task);
            service.AddTask(MakeTask("Задача 2")); // Другое описание

            var result = service.SearchTasks("проект").ToList();

            Assert.Single(result);
            Assert.Equal("Задача 1", result[0].Title);
        }

        [Fact(DisplayName = "SearchTasks без учёта регистра")]
        public void SearchTasks_CaseInsensitive()
        {
            var service = CreateService();
            service.AddTask(MakeTask("ВАЖНАЯ ЗАДАЧА"));

            var result = service.SearchTasks("важная").ToList();
            Assert.Single(result);
        }

        [Fact(DisplayName = "SearchTasks с пустым запросом возвращает все задачи")]
        public void SearchTasks_EmptyQuery_ReturnsAll()
        {
            var service = CreateService();
            service.AddTask(MakeTask("Задача 1"));
            service.AddTask(MakeTask("Задача 2"));

            var result = service.SearchTasks("").ToList();
            Assert.Equal(2, result.Count);
        }

        [Fact(DisplayName = "SearchTasks без совпадений возвращает пустой список")]
        public void SearchTasks_NoMatch_ReturnsEmpty()
        {
            var service = CreateService();
            service.AddTask(MakeTask("Купить молоко"));

            var result = service.SearchTasks("космос").ToList();
            Assert.Empty(result);
        }

        // ─────────────────────────────────────────────
        //  5. Фильтрация по статусу
        // ─────────────────────────────────────────────

        [Fact(DisplayName = "FilterByStatus возвращает только задачи с нужным статусом")]
        public void FilterByStatus_ReturnsOnlyMatchingStatus()
        {
            var service = CreateService();
            service.AddTask(MakeTask("Новая 1", status: Core.Enums.WorkStatus.New));
            service.AddTask(MakeTask("В работе", status: Core.Enums.WorkStatus.InProgress));
            service.AddTask(MakeTask("Новая 2", status: Core.Enums.WorkStatus.New));
            service.AddTask(MakeTask("Готова",   status: Core.Enums.WorkStatus.Completed));

            var result = service.FilterByStatus(Core.Enums.WorkStatus.New).ToList();

            Assert.Equal(2, result.Count);
            Assert.All(result, t =>
                Assert.Equal(Core.Enums.WorkStatus.New, t.Status));
        }

        [Fact(DisplayName = "FilterByStatus по Completed возвращает только завершённые")]
        public void FilterByStatus_Completed_OnlyCompleted()
        {
            var service = CreateService();
            service.AddTask(MakeTask("А", status: Core.Enums.WorkStatus.Completed));
            service.AddTask(MakeTask("Б", status: Core.Enums.WorkStatus.New));

            var result = service.FilterByStatus(Core.Enums.WorkStatus.Completed).ToList();
            Assert.Single(result);
            Assert.Equal("А", result[0].Title);
        }

        // ─────────────────────────────────────────────
        //  6. Сортировка
        // ─────────────────────────────────────────────

        [Fact(DisplayName = "SortByPriority: High задачи идут первыми")]
        public void SortByPriority_HighFirst()
        {
            var service = CreateService();
            service.AddTask(MakeTask("Низкий",   priority: TaskPriority.Low));
            service.AddTask(MakeTask("Высокий",  priority: TaskPriority.High));
            service.AddTask(MakeTask("Средний",  priority: TaskPriority.Medium));

            var sorted = service.SortByPriority().ToList();

            Assert.Equal(TaskPriority.High,   sorted[0].Priority);
            Assert.Equal(TaskPriority.Medium, sorted[1].Priority);
            Assert.Equal(TaskPriority.Low,    sorted[2].Priority);
        }

        [Fact(DisplayName = "SortByDueDate: ближайший срок — первый")]
        public void SortByDueDate_EarliestFirst()
        {
            var service = CreateService();
            service.AddTask(MakeTask("Через месяц",  dueDate: DateTime.Today.AddDays(30)));
            service.AddTask(MakeTask("Завтра",       dueDate: DateTime.Today.AddDays(1)));
            service.AddTask(MakeTask("Через неделю", dueDate: DateTime.Today.AddDays(7)));

            var sorted = service.SortByDueDate().ToList();

            Assert.Equal("Завтра",       sorted[0].Title);
            Assert.Equal("Через неделю", sorted[1].Title);
            Assert.Equal("Через месяц",  sorted[2].Title);
        }

        // ─────────────────────────────────────────────
        //  7. Статистика
        // ─────────────────────────────────────────────

        [Fact(DisplayName = "GetStatistics возвращает корректное общее количество")]
        public void GetStatistics_TotalCount_IsCorrect()
        {
            var service = CreateService();
            service.AddTask(MakeTask("1"));
            service.AddTask(MakeTask("2"));
            service.AddTask(MakeTask("3"));

            var stats = service.GetStatistics();
            Assert.Equal(3, stats.Total);
        }

        [Fact(DisplayName = "GetStatistics считает завершённые задачи")]
        public void GetStatistics_CompletedCount_IsCorrect()
        {
            var service = CreateService();
            service.AddTask(MakeTask("А", status: Core.Enums.WorkStatus.Completed));
            service.AddTask(MakeTask("Б", status: Core.Enums.WorkStatus.Completed));
            service.AddTask(MakeTask("В", status: Core.Enums.WorkStatus.New));

            var stats = service.GetStatistics();
            Assert.Equal(2, stats.Completed);
        }

        [Fact(DisplayName = "GetStatistics считает просроченные задачи")]
        public void GetStatistics_OverdueCount_IsCorrect()
        {
            var service = CreateService();
            // Просрочена: срок вчера, статус не Completed
            service.AddTask(MakeTask("Просрочена",
                status: Core.Enums.WorkStatus.New,
                dueDate: DateTime.Today.AddDays(-1)));
            // Не просрочена: завершена
            service.AddTask(MakeTask("Завершена вовремя",
                status: Core.Enums.WorkStatus.Completed,
                dueDate: DateTime.Today.AddDays(-1)));
            // Не просрочена: срок в будущем
            service.AddTask(MakeTask("Нормальная",
                status: Core.Enums.WorkStatus.New,
                dueDate: DateTime.Today.AddDays(5)));

            var stats = service.GetStatistics();
            Assert.Equal(1, stats.Overdue);
        }

        [Fact(DisplayName = "GetStatistics: CompletionRate 100% когда все завершены")]
        public void GetStatistics_CompletionRate_AllCompleted()
        {
            var service = CreateService();
            service.AddTask(MakeTask("1", status: Core.Enums.WorkStatus.Completed));
            service.AddTask(MakeTask("2", status: Core.Enums.WorkStatus.Completed));

            var stats = service.GetStatistics();
            Assert.Equal(100.0, stats.CompletionRate);
        }

        [Fact(DisplayName = "GetStatistics: CompletionRate 0% для пустого списка")]
        public void GetStatistics_CompletionRate_EmptyList()
        {
            var service = CreateService();
            var stats = service.GetStatistics();
            Assert.Equal(0.0, stats.CompletionRate);
        }

        [Fact(DisplayName = "GetStatistics считает важные задачи")]
        public void GetStatistics_ImportantCount_IsCorrect()
        {
            var service = CreateService();
            service.AddTask(MakeTask("Важная 1", isImportant: true));
            service.AddTask(MakeTask("Важная 2", isImportant: true));
            service.AddTask(MakeTask("Обычная",  isImportant: false));

            var stats = service.GetStatistics();
            Assert.Equal(2, stats.Important);
        }

        // ─────────────────────────────────────────────
        //  8. Сохранение и загрузка JSON
        // ─────────────────────────────────────────────

        [Fact(DisplayName = "SaveToFile создаёт файл на диске")]
        public void SaveToFile_CreatesFile()
        {
            var service = CreateService();
            service.AddTask(MakeTask("Сохранить меня"));

            string path = Path.GetTempFileName();
            try
            {
                service.SaveToFile(path);
                Assert.True(File.Exists(path));
            }
            finally
            {
                File.Delete(path); // Убираем временный файл
            }
        }

        [Fact(DisplayName = "LoadFromFile восстанавливает ранее сохранённые задачи")]
        public void LoadFromFile_RestoresTasks()
        {
            var original = CreateService();
            original.AddTask(MakeTask("Задача для сохранения",
                priority: TaskPriority.High,
                status: Core.Enums.WorkStatus.InProgress,
                isImportant: true));

            string path = Path.GetTempFileName();
            try
            {
                original.SaveToFile(path);

                var loaded = CreateService();
                loaded.LoadFromFile(path);

                var tasks = loaded.GetAllTasks();
                Assert.Single(tasks);
                Assert.Equal("Задача для сохранения", tasks[0].Title);
                Assert.Equal(TaskPriority.High,               tasks[0].Priority);
                Assert.Equal(Core.Enums.WorkStatus.InProgress, tasks[0].Status);
                Assert.True(tasks[0].IsImportant);
            }
            finally
            {
                File.Delete(path);
            }
        }

        [Fact(DisplayName = "SaveToFile/LoadFromFile сохраняет несколько задач")]
        public void SaveAndLoad_MultipleTasks()
        {
            var original = CreateService();
            for (int i = 1; i <= 5; i++)
                original.AddTask(MakeTask($"Задача {i}"));

            string path = Path.GetTempFileName();
            try
            {
                original.SaveToFile(path);

                var loaded = CreateService();
                loaded.LoadFromFile(path);

                Assert.Equal(5, loaded.GetAllTasks().Count);
            }
            finally
            {
                File.Delete(path);
            }
        }

        [Fact(DisplayName = "LoadFromFile с несуществующим файлом бросает FileNotFoundException")]
        public void LoadFromFile_NonExistingFile_ThrowsFileNotFoundException()
        {
            var service = CreateService();
            Assert.Throws<FileNotFoundException>(() =>
                service.LoadFromFile("C:/does_not_exist_12345.json"));
        }

        [Fact(DisplayName = "SaveToFile с пустым путём бросает ArgumentException")]
        public void SaveToFile_EmptyPath_ThrowsArgumentException()
        {
            var service = CreateService();
            Assert.Throws<ArgumentException>(() => service.SaveToFile(""));
        }

        [Fact(DisplayName = "LoadFromFile заменяет существующие задачи новыми")]
        public void LoadFromFile_ReplacesExistingTasks()
        {
            var service = CreateService();
            service.AddTask(MakeTask("Старая задача"));

            // Сохраняем другой набор задач
            var other = CreateService();
            other.AddTask(MakeTask("Новая задача A"));
            other.AddTask(MakeTask("Новая задача B"));

            string path = Path.GetTempFileName();
            try
            {
                other.SaveToFile(path);
                service.LoadFromFile(path); // Загружаем поверх старого

                var tasks = service.GetAllTasks();
                Assert.Equal(2, tasks.Count);
                Assert.DoesNotContain(tasks, t => t.Title == "Старая задача");
            }
            finally
            {
                File.Delete(path);
            }
        }

        // ─────────────────────────────────────────────
        //  9. Просроченность задач
        // ─────────────────────────────────────────────

        [Fact(DisplayName = "IsOverdue = true если срок прошёл и задача не завершена")]
        public void TaskItem_IsOverdue_True_WhenPastDueAndNotCompleted()
        {
            var task = MakeTask(dueDate: DateTime.Today.AddDays(-1),
                                status: Core.Enums.WorkStatus.New);
            Assert.True(task.IsOverdue);
        }

        [Fact(DisplayName = "IsOverdue = false если задача завершена (даже если срок прошёл)")]
        public void TaskItem_IsOverdue_False_WhenCompleted()
        {
            var task = MakeTask(dueDate: DateTime.Today.AddDays(-1),
                                status: Core.Enums.WorkStatus.Completed);
            Assert.False(task.IsOverdue);
        }

        [Fact(DisplayName = "IsOverdue = false если срок ещё не наступил")]
        public void TaskItem_IsOverdue_False_WhenFutureDueDate()
        {
            var task = MakeTask(dueDate: DateTime.Today.AddDays(5));
            Assert.False(task.IsOverdue);
        }
    }
}
