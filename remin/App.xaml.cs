using System.Windows;
using System;

namespace remin
{
    public partial class App : Application
    {
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
    }
}