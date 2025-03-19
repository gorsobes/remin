using Hardcodet.Wpf.TaskbarNotification;
using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;
using static System.Net.Mime.MediaTypeNames;
using System.Drawing;
using System.Windows.Media.Imaging;

namespace remin
{
    public partial class MainWindow : Window
    {
        private const string FilePath = "reminders.txt";
        private const string HistoryFilePath = "history.txt";

        public ObservableCollection<string> Reminders { get; set; } = new ObservableCollection<string>();

        private DispatcherTimer reminderTimer = new DispatcherTimer();


        public MainWindow()
        {
            InitializeComponent();

            // Добавляем обработчик двойного клика по иконке в трее
            var taskbarIcon = (TaskbarIcon)this.FindResource("TrayIcon");

            // Проверяем, что иконка загружена
            if (taskbarIcon != null)
            {
                try
                {
                    Uri iconUri = new Uri("icon.ico", UriKind.RelativeOrAbsolute);
                    taskbarIcon.IconSource = BitmapFrame.Create(iconUri);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка загрузки иконки: {ex.Message}");
                }
            }
        
            // Показ окна по двойному клику
            

        //Uri iconUri = new Uri("icon.ico", UriKind.RelativeOrAbsolute);
        // taskbarIcon.IconSource = BitmapFrame.Create(iconUri);

        reminderTimer.Interval = TimeSpan.FromMinutes(1); // Проверять каждую минуту
            reminderTimer.Tick += ReminderTimer_Tick;
            reminderTimer.Start();

            Reminders = new ObservableCollection<string>(); // Инициализация коллекции
            DataContext = this; // Установка контекста данных
            ReminderDatePicker.SelectedDate = DateTime.Today; // Устанавливаем текущую дату
            LoadReminders(); // Загружаем напоминания
            ApplyDateFilter(); // Фильтруем сразу после загрузки
        }

        private void TaskbarIcon_TrayMouseDoubleClick(object sender, RoutedEventArgs e)
        {
            this.Show();
            this.WindowState = WindowState.Normal;
            this.Activate();
            this.Topmost = true;
            this.Topmost = false;
            this.Focus();
        }

        // Закрытие окна — сворачиваем в трей
        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            e.Cancel = true; // Отменяем стандартное закрытие
            this.Hide();     // Скрываем окно
                             // Показываем уведомление в трее (опционально)
            var taskbarIcon = (TaskbarIcon)this.FindResource("TrayIcon");
            taskbarIcon.ShowBalloonTip("Свернуто в трей", "Приложение продолжает работать в фоновом режиме.", BalloonIcon.Info);
        }

        // Показ окна при клике на пункт "Открыть"
        private void ShowWindow_Click(object sender, RoutedEventArgs e)
        {
            if (this.WindowState == WindowState.Minimized || !this.IsVisible)
            {
                this.Show();
                this.WindowState = WindowState.Normal;
                this.Activate(); // Окно становится активным
                this.Topmost = true;
                this.Topmost = false; // Снимаем принудительный Topmost
                this.Focus(); // Перевод фокуса на окно
            }
            else
            {
                this.Activate(); // Если окно уже открыто, просто активируем его
            }
        }


        // Завершение приложения при клике на "Выход"
        private void Exit_Click(object sender, RoutedEventArgs e)
        {
            System.Windows.Application.Current.Shutdown(); // Полное завершение
        }

        // Сворачивание окна в трей при минимизации
        protected override void OnStateChanged(EventArgs e)
        {
            if (WindowState == WindowState.Minimized)
            { 
            this.Hide(); // Скрываем окно
            }
            base.OnStateChanged(e);
        }


        private void MinimizeButton_Click(object sender, RoutedEventArgs e)
        {
            this.WindowState = WindowState.Minimized; // Сворачиваем окно
        }

        private void MaximizeButton_Click(object sender, RoutedEventArgs e)
        {
            if (this.WindowState == WindowState.Maximized)
            {
                this.WindowState = WindowState.Normal; // Восстанавливаем размер
            }
            else
            {
                this.WindowState = WindowState.Maximized; // Разворачиваем на весь экран
            }
        }

        private void ReminderTimer_Tick(object sender, EventArgs e)
        {
            string currentTime = DateTime.Now.ToString("HH:mm"); // Текущее время

            foreach (var reminder in Reminders.ToList()) // Перебираем все задания
            {
                string[] parts = reminder.Split(' ');
                
                if (parts.Length >= 3)
                {
                    string reminderTime = parts[1]; // Время напоминания
                    if (reminderTime == currentTime && reminderTime != "00:00")
                    {
                        ShowReminderWindow(reminder);
                    }
                }
            }
        } 


        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
             if (e.Key == Key.Enter && DeleteButton.IsEnabled) // Проверяем, активна ли кнопка
             {
                DeleteButton.RaiseEvent(new RoutedEventArgs(System.Windows.Controls.Primitives.ButtonBase.ClickEvent)); // Вызываем клик
             }
         }


    private void RemindersList_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (sender is TextBox textBox && textBox.DataContext is string reminder)
            {
                RemindersList.SelectedItem = reminder; // Устанавливаем выбранный элемент
                DeleteButton.IsEnabled = true; // Активируем кнопку "Удалить"
            }
        }


        private void ShowReminderWindow(string reminderText)
        {
            Window reminderWindow = new Window
            {
                Title = "Напоминание!",
                Width = 350,
                Height = 450,
                WindowStartupLocation = WindowStartupLocation.CenterScreen,
                Topmost = true // Поверх всех окон
            };

            StackPanel panel = new StackPanel { Margin = new Thickness(10) };
            panel.Children.Add(new TextBlock { Text = reminderText, TextWrapping = TextWrapping.Wrap });

            Button closeButton = new Button { Content = "Закрыть", Margin = new Thickness(5) };
            closeButton.Click += (s, e) => reminderWindow.Close();

            Button snoozeButton = new Button { Content = "Отложить на 15 минут", Margin = new Thickness(5) };
            snoozeButton.Click += (s, e) =>
            {
                reminderWindow.Close();
                SnoozeReminder(reminderText);
            };

            panel.Children.Add(closeButton);
            panel.Children.Add(snoozeButton);
            reminderWindow.Content = panel;

            reminderWindow.ShowDialog();
        }

        private void SnoozeReminder(string reminderText)
        {
            string[] parts = reminderText.Split(' ');
            if (parts.Length >= 3 && DateTime.TryParse(parts[1], out DateTime originalTime))
            {
                DateTime newTime = originalTime.AddMinutes(15);
                string newReminder = $"{parts[0]} {newTime:HH:mm} {string.Join(" ", parts.Skip(2))}";

                Reminders.Add(newReminder);
                SaveReminders();
            }
        }


        private void AddButton_Click(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrWhiteSpace(InputTextBox.Text))
            {
                string text = InputTextBox.Text;
                string date = ReminderDatePicker.SelectedDate?.ToString("dd.MM.yyyy") ?? DateTime.Today.ToString("dd.MM.yyyy");
                string time = ReminderTimePicker.SelectedItem is ComboBoxItem selectedItem && selectedItem.Content.ToString() != "--:--"
                      ? selectedItem.Content.ToString()
                      : null; // Убираем "00:00", если время не выбрано

                string reminder = time != null ? $"{date} {time} {text}" : $"{date} {text}";


                Reminders.Add(reminder);

                // Обновляем список (чтобы данные добавились в коллекцию)
                RemindersList.ItemsSource = null;
                RemindersList.ItemsSource = Reminders;

                // Применяем фильтр, чтобы показывались только задачи на выбранную дату
                ApplyDateFilter();

                InputTextBox.Clear();
                AddButton.IsEnabled = false;
                SaveReminders();
            }
        }

        private void Border_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                this.DragMove(); // Перетаскивание окна
            }
        }


        private void ReminderTimePicker_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            AddButton.IsEnabled = !string.IsNullOrWhiteSpace(InputTextBox.Text);
        }



        private void HistoryButton_Click(object sender, RoutedEventArgs e)
        {
            if (File.Exists(HistoryFilePath))
            {
                string historyContent = File.ReadAllText(HistoryFilePath);
                MessageBox.Show(historyContent, "История удаленных напоминаний", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            else
            {
                MessageBox.Show("История пуста.", "История", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }


        private void InputTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            AddButton.IsEnabled = !string.IsNullOrWhiteSpace(InputTextBox.Text);
        }

        private void InputTextBox_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == System.Windows.Input.Key.Enter && AddButton.IsEnabled)
            {
                AddButton_Click(sender, e);
            }
        }


        private void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            if (RemindersList.SelectedItem != null)
            {
                MessageBoxResult result = MessageBox.Show("Точно удалить?", "Подтверждение", MessageBoxButton.YesNo, MessageBoxImage.Warning);
                if (result == MessageBoxResult.Yes)
                {
                    string deletedReminder = RemindersList.SelectedItem.ToString();

                    // Удаляем из ObservableCollection
                    Reminders.Remove(deletedReminder);

                    // Сохраняем новый список в файл
                    SaveReminders();

                    // Добавляем в историю
                    SaveToHistory(deletedReminder);

                    // Применяем фильтр, чтобы обновился список
                    ApplyDateFilter();
                }
            }
        }



        private void SaveReminders() => File.WriteAllLines(FilePath, Reminders);

        private void LoadReminders()
        {
            if (File.Exists(FilePath))
            {
                Reminders.Clear();
                var reminders = File.ReadAllLines(FilePath);
                foreach (var reminder in reminders)
                {
                    Reminders.Add(reminder);
                }

                // Обновляем список
                RemindersList.ItemsSource = null;
                RemindersList.ItemsSource = Reminders;
            }
        }



        private void SaveToHistory(string reminder) => File.AppendAllText(HistoryFilePath, reminder + Environment.NewLine);

        private void ShowAllButton_Click(object sender, RoutedEventArgs e)
        {
            ReminderDatePicker.SelectedDate = null; // Сбрасываем дату, показываем все записи
            RemindersList.ItemsSource = Reminders;
        }


        private void ReminderDatePicker_SelectedDateChanged(object sender, SelectionChangedEventArgs e)
        {
            ApplyDateFilter();
        }

        private void ApplyDateFilter()
        {
            var selectedDate = ReminderDatePicker.SelectedDate?.ToString("dd.MM.yyyy");
            RemindersList.ItemsSource = selectedDate == null ? Reminders : new ObservableCollection<string>(Reminders.Where(r => r.StartsWith(selectedDate)));
        }

        private void RemindersList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            DeleteButton.IsEnabled = RemindersList.SelectedItem != null;
        }

        private void ClearHistoryButton_Click(object sender, RoutedEventArgs e)
        {
            if (File.Exists(HistoryFilePath))
            {
                MessageBoxResult result = MessageBox.Show(
                    "Вы уверены, что хотите очистить историю?",
                    "Подтверждение",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Warning);

                if (result == MessageBoxResult.Yes)
                {
                    File.WriteAllText(HistoryFilePath, string.Empty); // Очищаем файл истории
                    MessageBox.Show("История удалена.", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            else
            {
                MessageBox.Show("История уже пуста.", "Информация", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

    }
}
