using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;
using TaskManager.Core.Enums;

// Именованное пространство совпадает с xmlns:local в XAML
namespace TaskManager.WPF.Views
{
    // ─────────────────────────────────────────────────────────────────
    //  Конвертеры значений для привязок данных (Value Converters)
    //  Используются в DataGrid для визуального оформления строк
    // ─────────────────────────────────────────────────────────────────

    /// <summary>Переводит bool IsImportant → звёздочку ★ или пустую строку</summary>
    public class BoolToStarConverter : IValueConverter
    {
        public static readonly BoolToStarConverter Instance = new();
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
            => value is true ? "★" : "";
        public object ConvertBack(object v, Type t, object p, CultureInfo c)
            => throw new NotImplementedException();
    }

    /// <summary>Переводит bool → Visibility (true → Visible, false → Collapsed)</summary>
    public class BoolToVisibilityConverter : IValueConverter
    {
        public static readonly BoolToVisibilityConverter Instance = new();
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
            => value is true ? Visibility.Visible : Visibility.Collapsed;
        public object ConvertBack(object v, Type t, object p, CultureInfo c)
            => throw new NotImplementedException();
    }

    /// <summary>Переводит TaskPriority → локализованный текст на русском</summary>
    public class PriorityToTextConverter : IValueConverter
    {
        public static readonly PriorityToTextConverter Instance = new();
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
            => value switch
            {
                TaskPriority.Low    => "Низкий",
                TaskPriority.Medium => "Средний",
                TaskPriority.High   => "Высокий",
                _ => value?.ToString() ?? ""
            };
        public object ConvertBack(object v, Type t, object p, CultureInfo c)
            => throw new NotImplementedException();
    }

    /// <summary>Переводит TaskPriority → цвет фона бейджа</summary>
    public class PriorityToBrushConverter : IValueConverter
    {
        public static readonly PriorityToBrushConverter Instance = new();
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
            => value switch
            {
                TaskPriority.Low    => new SolidColorBrush(Color.FromRgb(0x27, 0xAE, 0x60)), // Зелёный
                TaskPriority.Medium => new SolidColorBrush(Color.FromRgb(0xF3, 0x9C, 0x12)), // Жёлтый
                TaskPriority.High   => new SolidColorBrush(Color.FromRgb(0xE7, 0x4C, 0x3C)), // Красный
                _ => Brushes.Gray
            };
        public object ConvertBack(object v, Type t, object p, CultureInfo c)
            => throw new NotImplementedException();
    }

    /// <summary>Переводит TaskStatus → локализованный текст</summary>
    public class StatusToTextConverter : IValueConverter
    {
        public static readonly StatusToTextConverter Instance = new();
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
            => value switch
            {
                Core.Enums.WorkStatus.New        => "Новая",
                Core.Enums.WorkStatus.InProgress => "В работе",
                Core.Enums.WorkStatus.Completed  => "Завершена",
                _ => value?.ToString() ?? ""
            };
        public object ConvertBack(object v, Type t, object p, CultureInfo c)
            => throw new NotImplementedException();
    }

    /// <summary>Переводит TaskStatus → цвет фона бейджа</summary>
    public class StatusToBrushConverter : IValueConverter
    {
        public static readonly StatusToBrushConverter Instance = new();
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
            => value switch
            {
                Core.Enums.WorkStatus.New        => new SolidColorBrush(Color.FromRgb(0x95, 0xA5, 0xA6)),
                Core.Enums.WorkStatus.InProgress => new SolidColorBrush(Color.FromRgb(0x34, 0x98, 0xDB)),
                Core.Enums.WorkStatus.Completed  => new SolidColorBrush(Color.FromRgb(0x27, 0xAE, 0x60)),
                _ => Brushes.Gray
            };
        public object ConvertBack(object v, Type t, object p, CultureInfo c)
            => throw new NotImplementedException();
    }
}
