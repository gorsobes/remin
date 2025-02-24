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

        public MainWindow()
        {
            InitializeComponent();
            InputTextBox.Focus();
            ReminderDatePicker.SelectedDate = DateTime.Today;
            LoadReminders();
        }

        private void AddButton_Click(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrWhiteSpace(InputTextBox.Text) && ReminderDatePicker.SelectedDate.HasValue)
            {
                string reminder = $"{ReminderDatePicker.SelectedDate.Value.ToShortDateString()} - {InputTextBox.Text}";
                RemindersList.Items.Add(reminder);
                InputTextBox.Clear();
                ReminderDatePicker.SelectedDate = null;
                AddButton.IsEnabled = false;
                SaveReminders();
            }
        }

        private void InputTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            AddButton.IsEnabled = !string.IsNullOrWhiteSpace(InputTextBox.Text) && ReminderDatePicker.SelectedDate.HasValue;
        }

        private void ReminderDatePicker_SelectedDateChanged(object sender, SelectionChangedEventArgs e)
        {
            AddButton.IsEnabled = !string.IsNullOrWhiteSpace(InputTextBox.Text) && ReminderDatePicker.SelectedDate.HasValue;
        }

        private void InputTextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter && AddButton.IsEnabled)
            {
                AddButton_Click(sender, e);
            }
        }

        private void RemindersList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            DeleteButton.IsEnabled = RemindersList.SelectedItem != null;
        }

        private void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            if (RemindersList.SelectedItem != null)
            {
                MessageBoxResult result = MessageBox.Show("Точно УДАЛИТЬ?", "Подтверждение", MessageBoxButton.YesNo, MessageBoxImage.Warning);
                if (result == MessageBoxResult.Yes)
                {
                    RemindersList.Items.Remove(RemindersList.SelectedItem);
                    DeleteButton.IsEnabled = false;
                    SaveReminders();
                }
            }
        }

        private void SaveReminders()
        {
            List<string> reminders = new List<string>();
            foreach (var item in RemindersList.Items)
            {
                reminders.Add(item.ToString());
            }
            File.WriteAllLines(FilePath, reminders);
        }

        private void LoadReminders()
        {
            if (File.Exists(FilePath))
            {
                foreach (var line in File.ReadAllLines(FilePath))
                {
                    RemindersList.Items.Add(line);
                }
            }
        }
    }
}
