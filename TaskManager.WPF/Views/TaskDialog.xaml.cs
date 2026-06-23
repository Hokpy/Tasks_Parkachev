using System.Windows;
using TaskManager.Core.Enums;
using TaskManager.Core.Models;

namespace TaskManager.WPF.Views
{
    /// <summary>
    /// Диалоговое окно для добавления и редактирования задачи.
    /// Работает в двух режимах: создание новой задачи и редактирование существующей.
    /// </summary>
    public partial class TaskDialog : Window
    {
        /// <summary>Результат диалога — заполненная задача или null если отменено</summary>
        public TaskItem? Result { get; private set; }

        // Исходная задача, переданная в конструктор
        private readonly TaskItem _original;

        public TaskDialog(TaskItem task)
        {
            InitializeComponent();
            _original = task;

            // Режим определяем по наличию Title
            bool isNew = string.IsNullOrEmpty(task.Title);
            DialogTitle.Text = isNew ? "Новая задача" : "Редактировать задачу";

            // Заполняем ComboBox приоритетов
            PriorityBox.ItemsSource = new[]
            {
                new ComboItem { Display = "Низкий",   Value = TaskPriority.Low },
                new ComboItem { Display = "Средний",  Value = TaskPriority.Medium },
                new ComboItem { Display = "Высокий",  Value = TaskPriority.High }
            };
            PriorityBox.DisplayMemberPath = "Display";

            // Заполняем ComboBox статусов
            StatusBox.ItemsSource = new[]
            {
                new ComboItem2 { Display = "Новая",     Value = Core.Enums.WorkStatus.New },
                new ComboItem2 { Display = "В работе",  Value = Core.Enums.WorkStatus.InProgress },
                new ComboItem2 { Display = "Завершена", Value = Core.Enums.WorkStatus.Completed }
            };
            StatusBox.DisplayMemberPath = "Display";

            // Устанавливаем значения из переданной задачи
            TitleBox.Text         = task.Title;
            DescriptionBox.Text   = task.Description;
            // DueDate всегда имеет значение (TaskItem инициализирует его в Today+1)
            DueDatePicker.SelectedDate = task.DueDate;
            IsImportantBox.IsChecked   = task.IsImportant;

            // Выбираем текущий приоритет в ComboBox
            PriorityBox.SelectedIndex = (int)task.Priority;

            // Выбираем текущий статус в ComboBox
            StatusBox.SelectedIndex = (int)task.Status;

            // Устанавливаем фокус на поле названия
            Loaded += (_, _) => TitleBox.Focus();
        }

        /// <summary>Обработчик кнопки "Сохранить" — валидирует и сохраняет данные</summary>
        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            // Валидация: название обязательно
            if (string.IsNullOrWhiteSpace(TitleBox.Text))
            {
                MessageBox.Show("Введите название задачи.", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                TitleBox.Focus();
                return;
            }

            // Валидация: срок выполнения
            if (DueDatePicker.SelectedDate == null)
            {
                MessageBox.Show("Укажите срок выполнения.", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // Получаем выбранные значения из ComboBox
            var priority = ((ComboItem?)PriorityBox.SelectedItem)?.Value ?? TaskPriority.Medium;
            var status   = ((ComboItem2?)StatusBox.SelectedItem)?.Value ?? Core.Enums.WorkStatus.New;

            // Заполняем результат
            Result = new TaskItem
            {
                Id          = _original.Id,             // Сохраняем оригинальный Id
                Title       = TitleBox.Text.Trim(),
                Description = DescriptionBox.Text.Trim(),
                Priority    = priority,
                DueDate     = DueDatePicker.SelectedDate.Value,
                Status      = status,
                IsImportant = IsImportantBox.IsChecked == true,
                CreatedAt   = _original.CreatedAt  // Сохраняем исходную дату создания
            };

            DialogResult = true; // Закрывает диалог с результатом true
            Close();
        }

        /// <summary>Обработчик кнопки "Отмена"</summary>
        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }

        // Вспомогательные классы для ComboBox
        private class ComboItem
        {
            public string Display { get; set; } = "";
            public TaskPriority Value { get; set; }
        }

        private class ComboItem2
        {
            public string Display { get; set; } = "";
            public Core.Enums.WorkStatus Value { get; set; }
        }
    }
}
