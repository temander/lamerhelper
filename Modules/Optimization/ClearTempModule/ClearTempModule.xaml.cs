using System.Threading.Tasks;
using System;
using System.Windows;
using System.Windows.Controls;
using System.IO;
using LamerHelper.Modules;

namespace LamerHelper.Modules.Optimization
{
    public partial class ClearTempModule : UserControl, IModule
    {
        public ClearTempModule()
        {
            InitializeComponent();
        }

        public string ModuleName => "ClearTempModule";
        public string DisplayName => "Очистить папку Temp";
        public string Category => "Оптимизация";
        public string Description => "Удаляет кэш DNS, для получения новых данных. Может помочь при проблемах с доступам, таких как 'Невозможно установить соединение'. Также предотвращает возможные конфликты из-за различий DNS-информации от разных сетей.";
        public UserControl GetModuleControl() => this;

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            string tempPath = Path.GetTempPath();

            try
            {
                Task.WaitAll(
                    Task.Run(() => DeleteFiles(tempPath)),
                    Task.Run(() => DeleteDirectories(tempPath))
                );
            }
            finally
            {
                MessageBox.Show("Папка Temp очищена!", "Оптимизация", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        static void DeleteFiles(string path)
        {
            foreach (var file in Directory.EnumerateFiles(path))
            {
                try
                {
                File.Delete(file);
                }
                catch (Exception ex)
                {
                    // Обработка ошибки
                    _ = ex;
                }
            }
        }

        static void DeleteDirectories(string path)
        {
            foreach (var dir in Directory.EnumerateDirectories(path))
            {
                try
                {
                    Directory.Delete(dir, true);
                }
                catch (Exception ex)
                {
                    // Обработка ошибки
                    _ = ex;
                }
            }
        }
    }
}
