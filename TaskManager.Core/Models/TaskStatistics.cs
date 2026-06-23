namespace TaskManager.Core.Models
{
    /// <summary>
    /// Статистика по всем задачам в системе
    /// </summary>
    public class TaskStatistics
    {
        /// <summary>Общее количество задач</summary>
        public int Total { get; set; }

        /// <summary>Количество завершённых задач</summary>
        public int Completed { get; set; }

        /// <summary>Количество просроченных задач (срок прошёл, не завершены)</summary>
        public int Overdue { get; set; }

        /// <summary>Количество задач в работе</summary>
        public int InProgress { get; set; }

        /// <summary>Количество новых задач</summary>
        public int New { get; set; }

        /// <summary>Количество важных задач</summary>
        public int Important { get; set; }

        /// <summary>
        /// Процент завершённых задач от общего числа
        /// </summary>
        public double CompletionRate =>
            Total > 0 ? Math.Round((double)Completed / Total * 100, 1) : 0;
    }
}
