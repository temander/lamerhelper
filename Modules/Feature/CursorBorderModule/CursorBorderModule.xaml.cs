using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
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
                new() { Name = "Стандартный", StrokeColor = "0 120 215", BorderColor = "0 102 204" },
                new() { Name = "Красный", StrokeColor = "240 0 0", BorderColor = "255 0 0" },
                new() { Name = "Зелёный", StrokeColor = "0 240 0", BorderColor = "0 255 0" },
                new() { Name = "Синий", StrokeColor = "0 0 240", BorderColor = "0 0 255" },
                new() { Name = "Кастомный цвет", StrokeColor = "custom", BorderColor = "" }
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
            ComboBoxColorBorder? activeOption = colorComboBox.SelectedItem as ComboBoxColorBorder;

            string strokeColor = activeOption.StrokeColor;
            string borderColor = activeOption.BorderColor;

            if (activeOption.Name == "Кастомный цвет")
            {
                string customColor = customColorTextBox.Text;
                if (IsValidColor(customColor))
                {
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

        private static bool IsValidColor(string color)
        {
            try
            {
                var parts = color.Split([' ', ','], StringSplitOptions.RemoveEmptyEntries);
                if (parts.Length != 3) return false;

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

        private static string NormalizeColor(string color)
        {
            var parts = color.Split([' ', ','], StringSplitOptions.RemoveEmptyEntries);
            return string.Join(" ", parts);
        }

        private void UpdateColorPreview(string color)
        {
            try
            {
                if (IsValidColor(color))
                {
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
                    colorPreview.Background = Brushes.Transparent;
                }
            }
            catch
            {
                colorPreview.Background = Brushes.Transparent;
            }
        }

        // Изменение значения в реестре
        private static void ChangeValue(string valueName, string newValue)
        {
            try
            {
                using RegistryKey? key = Registry.CurrentUser.OpenSubKey(@"Control Panel\Colors", true);
                key?.SetValue(valueName, newValue);
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
