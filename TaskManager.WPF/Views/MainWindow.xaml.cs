using System.Windows;
using System.Windows.Input;
using TaskManager.WPF.ViewModels;

namespace TaskManager.WPF.Views
{
    /// <summary>
    /// Code-behind главного окна.
    /// Основная логика вынесена в MainViewModel (MVVM).
    /// Code-behind содержит только обработчики событий UI, недоступных через команды.
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Двойной клик по строке DataGrid открывает диалог редактирования.
        /// Это стандартное UX-поведение, которое нельзя привязать через команду напрямую.
        /// </summary>
        private void DataGrid_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (DataContext is MainViewModel vm && vm.SelectedTask != null)
            {
                vm.EditTaskCommand.Execute(null);
            }
        }
    }
}
