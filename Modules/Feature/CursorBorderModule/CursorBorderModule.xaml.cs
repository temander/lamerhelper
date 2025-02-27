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
        }

        public string ModuleName => "CursorBorderModule";
        public string DisplayName => "Настройка цвета выделения мыши";
        public string Category => "Фишка";
        public string Description => "Позволяет изменить цвет панели выделения, что появляется при нажатии ЛКМ и перемещении мыши.";
        public UserControl GetModuleControl() => this;

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            // TODO: сделать выбор цвета
            ChangeValue("HotTrackingColor", "255 0 0"); // обводка
            ChangeValue("Hilight", "210 0 0"); // панель

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
                MessageBox.Show("Ошибка при изменении реестра: {ex.Message}", "Фишка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
