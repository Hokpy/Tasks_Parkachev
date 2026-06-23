using System.Windows.Input;

namespace TaskManager.WPF.ViewModels
{
    /// <summary>
    /// Универсальная реализация команды ICommand для MVVM.
    /// Позволяет привязывать методы ViewModel к кнопкам XAML.
    /// </summary>
    public class RelayCommand : ICommand
    {
        // Делегат, который будет вызван при выполнении команды
        private readonly Action<object?> _execute;

        // Делегат, определяющий доступность команды (может быть null)
        private readonly Func<object?, bool>? _canExecute;

        /// <summary>
        /// Создаёт команду с действием и необязательным условием активности.
        /// </summary>
        /// <param name="execute">Действие при выполнении команды</param>
        /// <param name="canExecute">Условие активности команды (опционально)</param>
        public RelayCommand(Action<object?> execute, Func<object?, bool>? canExecute = null)
        {
            _execute    = execute    ?? throw new ArgumentNullException(nameof(execute));
            _canExecute = canExecute;
        }

        /// <summary>
        /// Событие, уведомляющее WPF о необходимости перепроверить CanExecute
        /// </summary>
        public event EventHandler? CanExecuteChanged
        {
            add    => CommandManager.RequerySuggested += value;
            remove => CommandManager.RequerySuggested -= value;
        }

        /// <summary>Определяет, можно ли выполнить команду сейчас</summary>
        public bool CanExecute(object? parameter) =>
            _canExecute == null || _canExecute(parameter);

        /// <summary>Выполняет команду</summary>
        public void Execute(object? parameter) => _execute(parameter);

        /// <summary>Вручную уведомляет WPF о смене состояния CanExecute</summary>
        public void RaiseCanExecuteChanged() =>
            CommandManager.InvalidateRequerySuggested();
    }
}
