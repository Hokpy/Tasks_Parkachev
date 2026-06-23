namespace TaskManager.Core.Enums
{
    /// <summary>
    /// Статус выполнения задачи.
    /// Переименован в WorkStatus, чтобы избежать конфликта
    /// с системным типом System.Threading.Tasks.TaskStatus.
    /// </summary>
    public enum WorkStatus
    {
        /// <summary>Новая задача</summary>
        New = 0,

        /// <summary>Задача в работе</summary>
        InProgress = 1,

        /// <summary>Задача завершена</summary>
        Completed = 2
    }
}
