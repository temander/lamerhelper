using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using LamerHelper.Modules;
using Microsoft.Win32;

namespace LamerHelper.Modules.Feature
{
    public partial class CursorBorderModule : UserControl, IModule
    {
        public CursorBorderModule()
        {
            InitializeComponent();

            colorComboBox.ItemsSource = new ComboBoxColorBorder[]
            {
                new ComboBoxColorBorder { Name = "Стандартный", StrokeColor = "0 120 215", BorderColor = "0 102 204" },
                new ComboBoxColorBorder { Name = "Красный", StrokeColor = "210 0 0", BorderColor = "255 0 0" },
                new ComboBoxColorBorder { Name = "Зелёный", StrokeColor = "0 210 0", BorderColor = "0 255 0" },
                new ComboBoxColorBorder { Name = "Синий", StrokeColor = "0 0 210", BorderColor = "0 0 255" },
                new ComboBoxColorBorder { Name = "Кастомный цвет", StrokeColor = "custom", BorderColor = "" }
            };
            colorComboBox.SelectedIndex = 0;
        }

        public string ModuleName => "CursorBorderModule";
        public string DisplayName => "Настройка цвета выделения мыши";
        public string Category => "Фишка";
        public string Description => "Позволяет изменить цвет панели выделения, что появляется при нажатии ЛКМ и перемещении мыши.";
        public UserControl GetModuleControl() => this;

        private void ColorComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (colorComboBox.SelectedItem is ComboBoxColorBorder selectedItem && selectedItem.Name == "Кастомный цвет")
            {
                customColorTextBox.Visibility = Visibility.Visible;
                UpdateColorPreview(customColorTextBox.Text);
            }
            else
            {
                customColorTextBox.Visibility = Visibility.Collapsed;
                UpdateColorPreview((colorComboBox.SelectedItem as ComboBoxColorBorder)?.StrokeColor);
            }
        }

        private void CustomColorTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            UpdateColorPreview(customColorTextBox.Text);
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            ComboBoxColorBorder activeOption = colorComboBox.SelectedItem as ComboBoxColorBorder;

            string strokeColor = activeOption.StrokeColor;
            string borderColor = activeOption.BorderColor;

            if (activeOption.Name == "Кастомный цвет")
            {
                string customColor = customColorTextBox.Text;
                if (IsValidColor(customColor))
                {
                    // Преобразуем цвет в нужный формат (без запятых и с одним пробелом между числами)
                    strokeColor = NormalizeColor(customColor);
                    borderColor = strokeColor;
                }
                else
                {
                    MessageBox.Show("Некорректный формат цвета. Введите цвет в формате R G B или R, G, B.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }
            }

            ChangeValue("HotTrackingColor", borderColor); // Панель
            ChangeValue("Hilight", strokeColor); // Обводка

            MessageBox.Show("Новый цвет применился! Перезапустите Windows, чтобы изменения вступили в силу.", "Фишка", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        // Проверка корректности цвета
        private bool IsValidColor(string color)
        {
            try
            {
                // Разделяем строку на числа, игнорируя пробелы и запятые
                var parts = color.Split(new[] { ' ', ',' }, StringSplitOptions.RemoveEmptyEntries);
                if (parts.Length != 3) return false;

                // Парсим числа и проверяем диапазон
                int r = int.Parse(parts[0]);
                int g = int.Parse(parts[1]);
                int b = int.Parse(parts[2]);

                return r >= 0 && r <= 255 && g >= 0 && g <= 255 && b >= 0 && b <= 255;
            }
            catch
            {
                return false;
            }
        }

        // Преобразование цвета в нужный формат (без запятых и с одним пробелом между числами)
        private string NormalizeColor(string color)
        {
            var parts = color.Split(new[] { ' ', ',' }, StringSplitOptions.RemoveEmptyEntries);
            return string.Join(" ", parts);
        }

        // Обновление отображения цвета
        private void UpdateColorPreview(string color)
        {
            try
            {
                if (IsValidColor(color))
                {
                    // Преобразуем цвет в нужный формат для отображения
                    string normalizedColor = NormalizeColor(color);
                    var parts = normalizedColor.Split(' ');
                    byte r = byte.Parse(parts[0]);
                    byte g = byte.Parse(parts[1]);
                    byte b = byte.Parse(parts[2]);

                    var brush = new SolidColorBrush(Color.FromRgb(r, g, b));
                    colorPreview.Background = brush;
                }
                else
                {
                    // Если цвет некорректен, сбрасываем отображение
                    colorPreview.Background = Brushes.Transparent;
                }
            }
            catch
            {
                // В случае ошибки сбрасываем отображение
                colorPreview.Background = Brushes.Transparent;
            }
        }

        // Изменение значения в реестре
        private void ChangeValue(string valueName, string newValue)
        {
            try
            {
                using (RegistryKey key = Registry.CurrentUser.OpenSubKey(@"Control Panel\Colors", true))
                {
                    if (key != null)
                    {
                        key.SetValue(valueName, newValue);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при изменении реестра: {ex.Message}", "Фишка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // Класс для хранения данных о цветах
        private class ComboBoxColorBorder
        {
            public string Name { get; set; } = "";
            public string StrokeColor { get; set; } = "";
            public string BorderColor { get; set; } = "";

            public override string ToString() => $"{Name} (rgb {StrokeColor})";
        }
    }
}