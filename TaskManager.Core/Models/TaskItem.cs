using TaskManager.Core.Enums;

namespace TaskManager.Core.Models
{
    /// <summary>
    /// Модель задачи — основная единица данных приложения
    /// </summary>
    public class TaskItem
    {
        /// <summary>Уникальный идентификатор задачи</summary>
        public Guid Id { get; set; } = Guid.NewGuid();

        /// <summary>Название задачи (обязательное поле)</summary>
        public string Title { get; set; } = string.Empty;

        /// <summary>Подробное описание задачи</summary>
        public string Description { get; set; } = string.Empty;

        /// <summary>Приоритет задачи: Low, Medium, High</summary>
        public TaskPriority Priority { get; set; } = TaskPriority.Medium;

        /// <summary>Срок выполнения задачи</summary>
        public DateTime DueDate { get; set; } = DateTime.Today.AddDays(1);

        /// <summary>Текущий статус задачи: New, InProgress, Completed</summary>
        public Enums.WorkStatus Status { get; set; } = Enums.WorkStatus.New;

        /// <summary>Признак важности — задача выделяется в интерфейсе</summary>
        public bool IsImportant { get; set; } = false;

        /// <summary>Дата создания задачи (устанавливается автоматически)</summary>
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        /// <summary>
        /// Проверяет, является ли задача просроченной:
        /// срок прошёл, а задача не завершена
        /// </summary>
        public bool IsOverdue => DueDate.Date < DateTime.Today
                                 && Status != Enums.WorkStatus.Completed;

        /// <summary>
        /// Возвращает строковое представление задачи для отладки
        /// </summary>
        public override string ToString() =>
            $"[{Priority}] {Title} — {Status} (до {DueDate:dd.MM.yyyy})";
    }
}
