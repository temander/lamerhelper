using System.IO;
using System.Windows;
using System.Windows.Controls;

namespace LamerHelper.Modules.Optimization
{
    public partial class ClearTempModule : UserControl, IModule
    {
        public ClearTempModule()
        {
            InitializeComponent();
            Loaded += ClearTempModule_Loaded;
        }

        public string ModuleName => "ClearTempModule";
        public string DisplayName => "Очистить папку Temp";
        public string Category => "Оптимизация";
        public string Description => "Очищает временную папку Temp, тем самым освобождая большое количество свободной памяти на диске.";
        public UserControl GetModuleControl() => this;

        private async void ClearTempModule_Loaded(object sender, RoutedEventArgs e)
        {
            await CalculateAndDisplayTempFolderSizeAsync();
        }

        private async Task CalculateAndDisplayTempFolderSizeAsync()
        {
            string tempPath = Path.GetTempPath();
            var progress = new Progress<long>(size => UpdateButtonText(size));
            await Task.Run(() => CalculateFolderSize(tempPath, progress));
        }

        private void CalculateFolderSize(string folderPath, IProgress<long> progress)
        {
            long totalSize = 0;
            try
            {
                var files = Directory.EnumerateFiles(folderPath, "*", SearchOption.AllDirectories);
                int count = 0;
                foreach (var file in files)
                {
                    try
                    {
                        FileInfo info = new FileInfo(file);
                        totalSize += info.Length;
                    }
                    catch (Exception)
                    {
                        // Если не удалось получить размер файла, пропускаем его
                    }
                    count++;

                    if (count % 10 == 0)
                    {
                        progress.Report(totalSize);
                    }
                }
            }
            catch (Exception)
            {
                // ignored
            }

            progress.Report(totalSize);
        }

        private void UpdateButtonText(long sizeInBytes)
        {
            double sizeInMb = sizeInBytes / (1024.0 * 1024.0);
            this.IconAndText.Content = $"Активировать ({sizeInMb:F1} MB)";
        }

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
                ClearTempModule_Loaded(sender, e);
                System.Windows.MessageBox.Show("Папка Temp очищена!", "Оптимизация", MessageBoxButton.OK, MessageBoxImage.Information);
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
                    // Обработка ошибки при удалении файла
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
                    // Обработка ошибки при удалении директории
                    _ = ex;
                }
            }
        }
    }
}
