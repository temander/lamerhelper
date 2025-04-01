using Microsoft.Win32;
using System.Diagnostics;
using System.Security;
using System.Windows;
using System.Windows.Controls;

namespace LamerHelper.Modules.Feature
{
    public partial class ContextMenuSwitcherModule : UserControl, IModule
    {
        private const string RegistryKeyPath = @"Software\Classes\CLSID\{86ca1aa0-34aa-4e8b-a509-50c905bae2a2}\InprocServer32";
        private const string ToggleStateRegistryKey = @"Software\LamerHelper\ContextMenuSwitcher";
        private const string ToggleStateRegistryValue = "ToggleState";
        private bool isInitializing;

        public ContextMenuSwitcherModule()
        {
            InitializeComponent();
            isInitializing = true;
            LoadToggleState();
            UpdateStatus();
            isInitializing = false;
        }

        public string ModuleName => "ContextMenuSwitcherModule";
        public string DisplayName => "Переключатель контекстного меню";
        public string Category => "Фишка";
        public string Description => "Переключает между контекстным меню Windows 10 и Windows 11";
        public UserControl GetModuleControl() => this;

        private void UpdateStatus()
        {
            try
            {
                using var key = Registry.CurrentUser.OpenSubKey(RegistryKeyPath);
                bool isWin10Style = key != null && string.IsNullOrEmpty(key.GetValue("")?.ToString());
                toggleSwitch.IsOn = isWin10Style;
                txtStatus.Text = $"Текущий стиль: Windows {(isWin10Style ? "10" : "11")}";
            }
            catch (Exception ex) when (ex is SecurityException or UnauthorizedAccessException)
            {
                MessageBox.Show("Требуются права администратора!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ToggleSwitch_Toggled(object sender, RoutedEventArgs e)
        {
            if (isInitializing) return;

            try
            {
                if (toggleSwitch.IsOn)
                {
                    using var key = Registry.CurrentUser.CreateSubKey(RegistryKeyPath, RegistryKeyPermissionCheck.ReadWriteSubTree);
                    key.SetValue("", "", RegistryValueKind.String);
                }
                else
                {
                    Registry.CurrentUser.DeleteSubKeyTree(RegistryKeyPath);
                }

                Process[] explorer = Process.GetProcessesByName("explorer");
                foreach (var process in explorer)
                {
                    process.Kill();
                }

                SaveToggleState(toggleSwitch.IsOn);
                UpdateStatus();
                MessageBox.Show("Контекстное меню изменено!", "Информация", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex) when (ex is SecurityException or UnauthorizedAccessException)
            {
                MessageBox.Show("Отказано в доступе! Запустите программу от администратора.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                toggleSwitch.IsOn = !toggleSwitch.IsOn;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                toggleSwitch.IsOn = !toggleSwitch.IsOn;
            }
        }

        private void SaveToggleState(bool state)
        {
            try
            {
                using var key = Registry.CurrentUser.CreateSubKey(ToggleStateRegistryKey);
                key.SetValue(ToggleStateRegistryValue, state ? 1 : 0, RegistryValueKind.DWord);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка сохранения состояния: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void LoadToggleState()
        {
            try
            {
                using var key = Registry.CurrentUser.OpenSubKey(ToggleStateRegistryKey);
                if (key != null)
                {
                    object value = key.GetValue(ToggleStateRegistryValue, 0);
                    toggleSwitch.IsOn = (int)value == 1;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки состояния: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}