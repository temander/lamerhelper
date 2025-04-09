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

            ColorComboBox.ItemsSource = new ComboBoxColorBorder[]
            {
                new() { Name = "Стандартный", StrokeColor = "0 120 215", BorderColor = "0 102 204" },
                new() { Name = "Красный", StrokeColor = "240 0 0", BorderColor = "255 0 0" },
                new() { Name = "Зелёный", StrokeColor = "0 240 0", BorderColor = "0 255 0" },
                new() { Name = "Синий", StrokeColor = "0 0 240", BorderColor = "0 0 255" },
                new() { Name = "Кастомный цвет", StrokeColor = "custom", BorderColor = "" }
            };
            ColorComboBox.SelectedIndex = 0;
        }

        public string ModuleName => "CursorBorderModule";
        public string DisplayName => "Настройка цвета выделения мыши";
        public string Category => "Фишка";
        public string Description => "Позволяет изменить цвет панели выделения, что появляется при нажатии ЛКМ и перемещении мыши.";
        public UserControl GetModuleControl() => this;

        private void ColorComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ColorComboBox.SelectedItem is ComboBoxColorBorder selectedItem && selectedItem.Name == "Кастомный цвет")
            {
                CustomColorTextBox.Visibility = Visibility.Visible;
                UpdateColorPreview(CustomColorTextBox.Text);
            }
            else
            {
                CustomColorTextBox.Visibility = Visibility.Collapsed;
                var strokeColor = (ColorComboBox.SelectedItem as ComboBoxColorBorder)?.StrokeColor;
                if (strokeColor != null)
                    UpdateColorPreview(strokeColor);
            }
        }

        private void CustomColorTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            UpdateColorPreview(CustomColorTextBox.Text);
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            ComboBoxColorBorder? activeOption = ColorComboBox.SelectedItem as ComboBoxColorBorder;

            string? strokeColor = activeOption?.StrokeColor;
            string? borderColor = activeOption?.BorderColor;

            if (activeOption?.Name == "Кастомный цвет")
            {
                string customColor = CustomColorTextBox.Text;
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

            if (borderColor != null) ChangeValue("HotTrackingColor", borderColor); // Панель
            if (strokeColor != null) ChangeValue("Hilight", strokeColor); // Обводка

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

        private static string? NormalizeColor(string color)
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
                    string? normalizedColor = NormalizeColor(color);
                    string?[]? parts = normalizedColor?.Split(' ');
                    byte r = byte.Parse(parts?[0] ?? string.Empty);
                    byte g = byte.Parse(parts?[1] ?? string.Empty);
                    byte b = byte.Parse(parts?[2] ?? string.Empty);

                    var brush = new SolidColorBrush(Color.FromRgb(r, g, b));
                    ColorPreview.Background = brush;
                }
                else
                {
                    ColorPreview.Background = Brushes.Transparent;
                }
            }
            catch
            {
                ColorPreview.Background = Brushes.Transparent;
            }
        }

        // Изменение значения в реестре
        private static void ChangeValue(string valueName, string? newValue)
        {
            try
            {
                using var key = Registry.CurrentUser.OpenSubKey(@"Control Panel\Colors", true);
                if (newValue != null) key?.SetValue(valueName, newValue);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при изменении реестра: {ex.Message}", "Фишка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // Класс для хранения данных о цветах
        private class ComboBoxColorBorder
        {
            public string Name { get; init; } = "";
            public string? StrokeColor { get; init; } = "";
            public string? BorderColor { get; init; } = "";

            public override string ToString() => $"{Name} (rgb {StrokeColor})";
        }
    }
}
