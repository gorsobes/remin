using System;
using System.Windows;
using Hardcodet.Wpf.TaskbarNotification;

namespace remin
{
    public partial class App : Application
    {
        private TaskbarIcon _taskbarIcon; // Иконка в трее

        [STAThread] // Указывает, что приложение использует однопоточную модель (STA)
        public static void Main()
        {
            // Создаем экземпляр приложения
            App app = new App();

            // Инициализируем компоненты (например, загружаем XAML)
            app.InitializeComponent();

            // Запускаем приложение
            app.Run();
        }

        // Обработчик запуска приложения
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            // Создаем главное окно
            MainWindow mainWindow = new MainWindow();

            // Инициализируем иконку в трее
            _taskbarIcon = (TaskbarIcon)mainWindow.FindResource("TrayIcon");

            // Показываем главное окно
            mainWindow.Show();
        }

        // Обработчик завершения приложения
        protected override void OnExit(ExitEventArgs e)
        {
            // Убедимся, что иконка в трее корректно удаляется
            if (_taskbarIcon != null)
            {
                _taskbarIcon.Dispose();
            }

            base.OnExit(e);
        }
    }
}