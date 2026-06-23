using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace TaskManager.WPF.ViewModels
{
    /// <summary>
    /// Базовый класс для всех ViewModel.
    /// Реализует INotifyPropertyChanged — механизм оповещения UI об изменениях.
    /// </summary>
    public abstract class BaseViewModel : INotifyPropertyChanged
    {
        /// <summary>
        /// Событие, которое WPF слушает для обновления привязанных элементов
        /// </summary>
        public event PropertyChangedEventHandler? PropertyChanged;

        /// <summary>
        /// Вызывает событие PropertyChanged для указанного свойства.
        /// Атрибут [CallerMemberName] автоматически подставляет имя вызывающего свойства.
        /// </summary>
        protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        /// <summary>
        /// Устанавливает значение поля и уведомляет UI, если значение изменилось.
        /// Возвращает true, если значение действительно изменилось.
        /// </summary>
        protected bool SetProperty<T>(ref T field, T value,
            [CallerMemberName] string? propertyName = null)
        {
            if (EqualityComparer<T>.Default.Equals(field, value))
                return false; // Значение не изменилось — не нужно уведомлять

            field = value;
            OnPropertyChanged(propertyName);
            return true;
        }
    }
}
