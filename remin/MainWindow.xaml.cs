using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows;

namespace remin
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            InputTextBox.Focus();
        }
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            // Устанавливаем текст в TextBlock из TextBox
            OutputTextBlock.Text = "Вы ввели: " + InputTextBox.Text;
        }
        private void InputTextBox_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            // Активируем кнопку, если в TextBox есть текст, иначе отключаем
            ShowTextButton.IsEnabled = !string.IsNullOrWhiteSpace(InputTextBox.Text);
        }
        private void InputTextBox_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == Key.Enter && ShowTextButton.IsEnabled)
            {
                Button_Click(sender, e); // Запускаем обработчик кнопки
            }
        }


    }
}

