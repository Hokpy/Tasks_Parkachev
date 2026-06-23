using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Input;
using Microsoft.Win32;
using TaskManager.Core.Enums;
using TaskManager.Core.Models;
using TaskManager.Core.Services;
using TaskManager.WPF.Views;

namespace TaskManager.WPF.ViewModels
{
    /// <summary>
    /// Главная ViewModel приложения.
    /// Связывает бизнес-логику (TaskService) с пользовательским интерфейсом (MainWindow).
    /// </summary>
    public class MainViewModel : BaseViewModel
    {
        // Сервис бизнес-логики
        private readonly TaskService _taskService = new();

        // ─────────────────────────────────────────────
        //  Свойства для привязки к UI
        // ─────────────────────────────────────────────

        /// <summary>Коллекция задач, отображаемых в DataGrid</summary>
        private ObservableCollection<TaskItem> _tasks = new();
        public ObservableCollection<TaskItem> Tasks
        {
            get => _tasks;
            set => SetProperty(ref _tasks, value);
        }

        /// <summary>Выбранная задача в DataGrid</summary>
        private TaskItem? _selectedTask;
        public TaskItem? SelectedTask
        {
            get => _selectedTask;
            set => SetProperty(ref _selectedTask, value);
        }

        /// <summary>Текст поискового запроса</summary>
        private string _searchText = string.Empty;
        public string SearchText
        {
            get => _searchText;
            set
            {
                SetProperty(ref _searchText, value);
                RefreshTaskList(); // Автоматическая фильтрация при изменении запроса
            }
        }

        /// <summary>Фильтр по статусу (null = все задачи)</summary>
        private Core.Enums.WorkStatus? _statusFilter;
        public Core.Enums.WorkStatus? StatusFilter
        {
            get => _statusFilter;
            set
            {
                SetProperty(ref _statusFilter, value);
                RefreshTaskList();
            }
        }

        /// <summary>Текущий режим сортировки</summary>
        private string _sortMode = "None";
        public string SortMode
        {
            get => _sortMode;
            set => SetProperty(ref _sortMode, value);
        }

        // ─────────────────────────────────────────────
        //  Статистика (привязана к панели внизу окна)
        // ─────────────────────────────────────────────

        private int _totalCount;
        public int TotalCount { get => _totalCount; set => SetProperty(ref _totalCount, value); }

        private int _completedCount;
        public int CompletedCount { get => _completedCount; set => SetProperty(ref _completedCount, value); }

        private int _overdueCount;
        public int OverdueCount { get => _overdueCount; set => SetProperty(ref _overdueCount, value); }

        private int _importantCount;
        public int ImportantCount { get => _importantCount; set => SetProperty(ref _importantCount, value); }

        private double _completionRate;
        public double CompletionRate { get => _completionRate; set => SetProperty(ref _completionRate, value); }

        // ─────────────────────────────────────────────
        //  Список вариантов фильтра статуса
        // ─────────────────────────────────────────────

        /// <summary>Элементы выпадающего списка фильтрации</summary>
        public List<StatusFilterItem> StatusFilterItems { get; } = new()
        {
            new StatusFilterItem { Display = "Все задачи",    Value = null },
            new StatusFilterItem { Display = "Новые",         Value = Core.Enums.WorkStatus.New },
            new StatusFilterItem { Display = "В работе",      Value = Core.Enums.WorkStatus.InProgress },
            new StatusFilterItem { Display = "Завершённые",   Value = Core.Enums.WorkStatus.Completed }
        };

        // Инициализируем значением «по умолчанию», чтобы поле никогда не было null
        private StatusFilterItem _selectedStatusFilter = new StatusFilterItem { Display = "Все задачи", Value = null };
        public StatusFilterItem SelectedStatusFilter
        {
            get => _selectedStatusFilter;
            set
            {
                SetProperty(ref _selectedStatusFilter, value);
                StatusFilter = value?.Value;
            }
        }

        // ─────────────────────────────────────────────
        //  Команды (привязываются к кнопкам в XAML)
        // ─────────────────────────────────────────────

        public ICommand AddTaskCommand    { get; }
        public ICommand EditTaskCommand   { get; }
        public ICommand DeleteTaskCommand { get; }
        public ICommand SaveCommand       { get; }
        public ICommand LoadCommand       { get; }
        public ICommand SortByPriorityCommand { get; }
        public ICommand SortByDueDateCommand  { get; }
        public ICommand ClearSortCommand      { get; }

        // ─────────────────────────────────────────────
        //  Конструктор
        // ─────────────────────────────────────────────

        public MainViewModel()
        {
            // Синхронизируем поле с реальным первым элементом списка (создаётся выше)
            _selectedStatusFilter = StatusFilterItems[0];

            // Регистрация команд
            AddTaskCommand    = new RelayCommand(_ => AddTask());
            EditTaskCommand   = new RelayCommand(_ => EditTask(),   _ => SelectedTask != null);
            DeleteTaskCommand = new RelayCommand(_ => DeleteTask(), _ => SelectedTask != null);
            SaveCommand       = new RelayCommand(_ => SaveToFile());
            LoadCommand       = new RelayCommand(_ => LoadFromFile());
            SortByPriorityCommand = new RelayCommand(_ => { SortMode = "Priority"; RefreshTaskList(); });
            SortByDueDateCommand  = new RelayCommand(_ => { SortMode = "DueDate";  RefreshTaskList(); });
            ClearSortCommand      = new RelayCommand(_ => { SortMode = "None";     RefreshTaskList(); });

            // Загружаем демонстрационные данные при старте
            LoadDemoData();
            RefreshTaskList();
        }

        // ─────────────────────────────────────────────
        //  Методы команд
        // ─────────────────────────────────────────────

        /// <summary>Открывает диалог добавления новой задачи</summary>
        private void AddTask()
        {
            var dialog = new TaskDialog(new TaskItem())
            {
                // Owner обязателен: даёт доступ к ресурсам App.xaml и центрирует диалог
                Owner = System.Windows.Application.Current.MainWindow
            };
            if (dialog.ShowDialog() == true && dialog.Result != null)
            {
                _taskService.AddTask(dialog.Result);
                RefreshTaskList();
            }
        }

        /// <summary>Открывает диалог редактирования выбранной задачи</summary>
        private void EditTask()
        {
            if (SelectedTask == null) return;

            // Передаём копию задачи, чтобы отмена не изменила оригинал
            var copy   = CloneTask(SelectedTask);
            var dialog = new TaskDialog(copy)
            {
                Owner = System.Windows.Application.Current.MainWindow
            };

            if (dialog.ShowDialog() == true && dialog.Result != null)
            {
                _taskService.UpdateTask(dialog.Result);
                RefreshTaskList();
            }
        }

        /// <summary>Удаляет выбранную задачу после подтверждения</summary>
        private void DeleteTask()
        {
            if (SelectedTask == null) return;

            var result = MessageBox.Show(
                $"Удалить задачу «{SelectedTask.Title}»?",
                "Подтверждение удаления",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                _taskService.DeleteTask(SelectedTask.Id);
                SelectedTask = null;
                RefreshTaskList();
            }
        }

        /// <summary>Сохраняет задачи в JSON через диалог выбора файла</summary>
        private void SaveToFile()
        {
            var dialog = new SaveFileDialog
            {
                Filter      = "JSON файлы (*.json)|*.json|Все файлы (*.*)|*.*",
                DefaultExt  = "json",
                FileName    = "tasks"
            };

            if (dialog.ShowDialog() == true)
            {
                try
                {
                    _taskService.SaveToFile(dialog.FileName);
                    MessageBox.Show("Задачи успешно сохранены!", "Сохранение",
                        MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка сохранения: {ex.Message}", "Ошибка",
                        MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        /// <summary>Загружает задачи из JSON через диалог выбора файла</summary>
        private void LoadFromFile()
        {
            var dialog = new OpenFileDialog
            {
                Filter = "JSON файлы (*.json)|*.json|Все файлы (*.*)|*.*"
            };

            if (dialog.ShowDialog() == true)
            {
                try
                {
                    _taskService.LoadFromFile(dialog.FileName);
                    RefreshTaskList();
                    MessageBox.Show("Задачи успешно загружены!", "Загрузка",
                        MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка загрузки: {ex.Message}", "Ошибка",
                        MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        // ─────────────────────────────────────────────
        //  Обновление списка и статистики
        // ─────────────────────────────────────────────

        /// <summary>
        /// Применяет поиск, фильтр и сортировку, обновляет Tasks и статистику
        /// </summary>
        private void RefreshTaskList()
        {
            // Начинаем с полного списка
            IEnumerable<TaskItem> result = _taskService.GetAllTasks();

            // Применяем фильтр по статусу
            if (StatusFilter.HasValue)
                result = result.Where(t => t.Status == StatusFilter.Value);

            // Применяем текстовый поиск
            if (!string.IsNullOrWhiteSpace(SearchText))
            {
                string q = SearchText.ToLowerInvariant();
                result = result.Where(t =>
                    t.Title.ToLowerInvariant().Contains(q) ||
                    t.Description.ToLowerInvariant().Contains(q));
            }

            // Применяем сортировку
            result = SortMode switch
            {
                "Priority" => result.OrderByDescending(t => t.Priority),
                "DueDate"  => result.OrderBy(t => t.DueDate),
                _          => result.OrderByDescending(t => t.CreatedAt)
            };

            // Обновляем коллекцию (DataGrid перерисуется автоматически)
            Tasks = new ObservableCollection<TaskItem>(result);

            // Обновляем статистику
            UpdateStatistics();
        }

        /// <summary>Пересчитывает и обновляет поля статистики</summary>
        private void UpdateStatistics()
        {
            var stats = _taskService.GetStatistics();
            TotalCount     = stats.Total;
            CompletedCount = stats.Completed;
            OverdueCount   = stats.Overdue;
            ImportantCount = stats.Important;
            CompletionRate = stats.CompletionRate;
        }

        // ─────────────────────────────────────────────
        //  Вспомогательные методы
        // ─────────────────────────────────────────────

        /// <summary>Создаёт поверхностную копию задачи для редактирования</summary>
        private static TaskItem CloneTask(TaskItem src) => new TaskItem
        {
            Id          = src.Id,
            Title       = src.Title,
            Description = src.Description,
            Priority    = src.Priority,
            DueDate     = src.DueDate,
            Status      = src.Status,
            IsImportant = src.IsImportant,
            CreatedAt   = src.CreatedAt
        };

        /// <summary>Добавляет несколько задач для демонстрации работы приложения</summary>
        private void LoadDemoData()
        {
            _taskService.AddTask(new TaskItem
            {
                Title       = "Подготовить отчёт по практике",
                Description = "Написать пояснительную записку и оформить по стандарту техникума",
                Priority    = TaskPriority.High,
                DueDate     = DateTime.Today.AddDays(3),
                Status      = Core.Enums.WorkStatus.InProgress,
                IsImportant = true
            });
            _taskService.AddTask(new TaskItem
            {
                Title       = "Сдать курсовую работу",
                Description = "Защита курсового проекта по дисциплине ПМ.01",
                Priority    = TaskPriority.High,
                DueDate     = DateTime.Today.AddDays(-1), // Просрочена
                Status      = Core.Enums.WorkStatus.New,
                IsImportant = true
            });
            _taskService.AddTask(new TaskItem
            {
                Title       = "Изучить LINQ в C#",
                Description = "Пройти примеры с фильтрацией, сортировкой и агрегацией",
                Priority    = TaskPriority.Medium,
                DueDate     = DateTime.Today.AddDays(7),
                Status      = Core.Enums.WorkStatus.InProgress,
                IsImportant = false
            });
            _taskService.AddTask(new TaskItem
            {
                Title       = "Прочитать книгу по WPF",
                Description = "Pro WPF in C# — главы 1-5 по основам MVVM и привязке данных",
                Priority    = TaskPriority.Low,
                DueDate     = DateTime.Today.AddDays(14),
                Status      = Core.Enums.WorkStatus.New,
                IsImportant = false
            });
            _taskService.AddTask(new TaskItem
            {
                Title       = "Установить Visual Studio 2022",
                Description = "Скачать и настроить IDE с компонентами .NET и WPF",
                Priority    = TaskPriority.Low,
                DueDate     = DateTime.Today.AddDays(-5),
                Status      = Core.Enums.WorkStatus.Completed,
                IsImportant = false
            });
        }
    }

    /// <summary>
    /// Элемент списка фильтра по статусу (для ComboBox)
    /// </summary>
    public class StatusFilterItem
    {
        public string Display { get; set; } = string.Empty;
        public Core.Enums.WorkStatus? Value { get; set; }
    }
}
