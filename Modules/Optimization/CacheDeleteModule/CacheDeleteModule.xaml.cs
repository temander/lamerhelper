using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using LamerHelper.Modules;

namespace LamerHelper.Modules.Optimization
{
    public partial class CacheDeleteModule : UserControl, IModule
    {
        public CacheDeleteModule()
        {
            InitializeComponent();
        }

        public string ModuleName => "CacheDeleteModule";
        public string DisplayName => "Очистить кэш обновлений";
        public string Category => "Оптимизация";
        public string Description => "Очищяет кэш, который накапливается после обновлений.";
        public UserControl GetModuleControl() => this;

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Process.Start("net", "stop wuauserv");
                Process.Start("cmd.exe", "/c cd C:\\Windows\\SoftwareDistribution & del /f /s /q Download");
                Process.Start("net", "start wuauserv");
                MessageBox.Show("Кэш обновлений Windows успешно очищен!", "Оптимизация", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Произошла ошибка: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}