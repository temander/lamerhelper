using Microsoft.Win32;
using System;
using System.ComponentModel;
using System.Security;
using System.Windows;
using System.Windows.Controls;

namespace LamerHelper.Modules.Feature
{
    public partial class ContextMenuSwitcherModule : UserControl, IModule
    {
        private const string RegistryKeyPath = @"Software\Classes\CLSID\{86ca1aa0-34aa-4e8b-a509-50c905bae2a2}\InprocServer32";

        public ContextMenuSwitcherModule()
        {
            InitializeComponent();
            UpdateStatus();
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
                using (var key = Registry.CurrentUser.OpenSubKey(RegistryKeyPath))
                {
                    bool isWin10Style = key != null && string.IsNullOrEmpty(key.GetValue("")?.ToString());
                    txtStatus.Text = isWin10Style
                        ? "Текущий стиль: Windows 10"
                        : "Текущий стиль: Windows 11";
                }
            }
            catch (Exception ex) when (ex is SecurityException || ex is UnauthorizedAccessException)
            {
                MessageBox.Show("Требуются права администратора!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ToggleContextMenu(bool win10Style)
        {
            try
            {
                if (win10Style)
                {
                    using (var key = Registry.CurrentUser.CreateSubKey(RegistryKeyPath, RegistryKeyPermissionCheck.ReadWriteSubTree))
                    {
                        key.SetValue("", "", RegistryValueKind.String);
                    }
                }
                else
                {
                    Registry.CurrentUser.DeleteSubKeyTree(RegistryKeyPath);
                }

                UpdateStatus();
                MessageBox.Show("Контекстное меню изменено!", "Информация", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex) when (ex is SecurityException || ex is UnauthorizedAccessException)
            {
                MessageBox.Show("Отказано в доступе! Запустите программу от администратора.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void btnWin10_Click(object sender, RoutedEventArgs e) => ToggleContextMenu(true);
        private void btnWin11_Click(object sender, RoutedEventArgs e) => ToggleContextMenu(false);
    }
}