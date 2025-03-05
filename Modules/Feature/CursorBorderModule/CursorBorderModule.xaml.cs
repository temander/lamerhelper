using System;
using System.Windows;
using System.Windows.Controls;
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
            };
            colorComboBox.SelectedIndex = 0;
        }

        public string ModuleName => "CursorBorderModule";
        public string DisplayName => "Настройка цвета выделения мыши";
        public string Category => "Фишка";
        public string Description => "Позволяет изменить цвет панели выделения, что появляется при нажатии ЛКМ и перемещении мыши.";
        public UserControl GetModuleControl() => this;

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            ComboBoxColorBorder activeOption = colorComboBox.SelectedItem as ComboBoxColorBorder;

            ChangeValue("HotTrackingColor", activeOption.BorderColor); // панель
            ChangeValue("Hilight", activeOption.StrokeColor); // обводка

            MessageBox.Show("Новый цвет применился! Перезапустите Windows, чтобы изменения вступили в силу.", "Фишка", MessageBoxButton.OK, MessageBoxImage.Information);
        }

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

        private class ComboBoxColorBorder
        {
            public string Name { get; set; } = "";
            public string StrokeColor { get; set; } = "";
            public string BorderColor { get; set; } = "";

            public override string ToString() => $"{Name} ({StrokeColor})";
        }
    }
}
