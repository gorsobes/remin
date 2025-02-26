using System;
using System.Collections.Generic;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace remin
{
    public partial class MainWindow : Window
    {
        private const string FilePath = "reminders.txt";
        private const string HistoryFilePath = "history.txt";


        public MainWindow()
        {
            InitializeComponent(); // Инициализация компонентов
            InputTextBox.Focus();

            // Устанавливаем текущую дату по умолчанию
            ReminderDatePicker.SelectedDate = DateTime.Today;

            // Загружаем напоминания из файла
            LoadReminders();
        }

        private void AddButton_Click(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrWhiteSpace(InputTextBox.Text))
            {
                // Дата всегда текущая (по умолчанию)
                string date = ReminderDatePicker.SelectedDate?.ToShortDateString() ?? DateTime.Today.ToShortDateString();
                string time = (ReminderTimePicker.SelectedItem as ComboBoxItem)?.Content.ToString();

                // Если время не выбрано, оставляем его пустым
                if (time == "--:--")
                    time = null;

                // Формируем текст напоминания
                string reminderText = time == null ? $"{date} - {InputTextBox.Text}" : $"{date} {time} - {InputTextBox.Text}";
                RemindersList.Items.Add(reminderText);

                // Очищаем поле ввода и сбрасываем время
                InputTextBox.Clear();
                ReminderTimePicker.SelectedIndex = 0;
                AddButton.IsEnabled = false;

                // Сохраняем напоминания в файл
                SaveReminders();
            }
        }

        private void InputTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (InputTextBox != null && AddButton != null) // Проверяем, что элементы существуют
            {
                // Активируем кнопку "Добавить", если в поле ввода есть текст
                AddButton.IsEnabled = !string.IsNullOrWhiteSpace(InputTextBox.Text);
            }
        }

        private void InputTextBox_KeyDown(object sender, KeyEventArgs e)
        {
            // Добавляем напоминание по нажатию Enter
            if (e.Key == Key.Enter && AddButton.IsEnabled)
            {
                AddButton_Click(sender, e);
            }
        }

        private void RemindersList_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter && DeleteButton.IsEnabled)
            {
                DeleteButton_Click(sender, e);
            }
        }

        private void ReminderTimePicker_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // Активируем кнопку "Добавить", если в поле ввода есть текст
            if (InputTextBox != null && AddButton != null)
            {
                AddButton.IsEnabled = !string.IsNullOrWhiteSpace(InputTextBox.Text);
            }
        }

        private void RemindersList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // Активируем кнопку "Удалить", если выбрано напоминание
            if (DeleteButton != null)
            {
                DeleteButton.IsEnabled = RemindersList.SelectedItem != null;
            }
        }

        private void SaveReminders()
        {
            // Загружаем все напоминания из файла
            List<string> allReminders = File.Exists(FilePath) ? new List<string>(File.ReadAllLines(FilePath)) : new List<string>();

            // Удаляем старые напоминания для текущей даты
            DateTime selectedDate = ReminderDatePicker.SelectedDate ?? DateTime.Today;
            allReminders.RemoveAll(r => ExtractDate(r).Date == selectedDate.Date);

            // Добавляем новые напоминания
            foreach (var item in RemindersList.Items)
            {
                allReminders.Add(item.ToString());
            }

            // Сохраняем обновленный список в файл
            File.WriteAllLines(FilePath, allReminders);
        }





        private List<string> LoadReminders()
        {
            if (File.Exists(FilePath))
            {
                List<string> reminders = new List<string>(File.ReadAllLines(FilePath));
                reminders.Sort(CompareReminders); // Сортируем перед фильтрацией
                return reminders;
            }
            return new List<string>();
        }





        private int CompareReminders(string x, string y)
        {
            return ExtractDate(x).CompareTo(ExtractDate(y));
        }

        private DateTime ExtractDate(string reminder)
        {
            string[] parts = reminder.Split(new[] { ' ' }, 3); // Берем первые 2 части (дата + время)
            string dateString = parts[0]; // Дата
            string timeString = parts.Length > 1 && parts[1].Contains(":") ? parts[1] : "00:00"; // Время или 00:00

            if (DateTime.TryParse($"{dateString} {timeString}", out DateTime dateTime))
            {
                return dateTime;
            }

            return DateTime.MaxValue;
        }



        private void ApplyDateFilter(List<string> allReminders)
        {
            RemindersList.Items.Clear();
            DateTime selectedDate = ReminderDatePicker.SelectedDate ?? DateTime.Today;

            foreach (var reminder in allReminders)
            {
                if (ExtractDate(reminder).Date == selectedDate.Date) // Фильтруем по дате
                {
                    RemindersList.Items.Add(reminder);
                }
            }
        }


        private void ReminderDatePicker_SelectedDateChanged(object sender, SelectionChangedEventArgs e)
        {
            List<string> reminders = LoadReminders(); // Загружаем список из файла
            ApplyDateFilter(reminders); // Применяем фильтр по дате
        }



        private void ShowAllButton_Click(object sender, RoutedEventArgs e)
        {
            RemindersList.Items.Clear();
            List<string> reminders = LoadReminders();

            foreach (var reminder in reminders)
            {
                RemindersList.Items.Add(reminder);
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
                    RemindersList.Items.Remove(RemindersList.SelectedItem);
                    SaveReminders();
                    SaveToHistory(deletedReminder); // Сохраняем в историю
                }
            }
        }
        private void SaveToHistory(string reminder)
        {
            File.AppendAllText(HistoryFilePath, reminder + Environment.NewLine);
        }

        private void HistoryButton_Click(object sender, RoutedEventArgs e)
        {
            if (File.Exists(HistoryFilePath))
            {
                string history = File.ReadAllText(HistoryFilePath);
                MessageBox.Show(history, "История удаленных напоминаний", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            else
            {
                MessageBox.Show("История пуста.", "История", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

    }
}