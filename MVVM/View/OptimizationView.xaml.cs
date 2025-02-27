using System;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace LamerHelper.MVVM.View
{
    /// <summary>
    /// Логика взаимодействия для OptimizationView.xaml
    /// </summary>
    public partial class OptimizationView : UserControl
    {
        public OptimizationView()
        {
            InitializeComponent();
        }

        private void TempBtn_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            string tempPath = Path.GetTempPath();

            try
            {
                Task.WaitAll(
                    Task.Run(() => DeleteFiles(tempPath)),
                    Task.Run(() => DeleteDirectories(tempPath))
                );
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка при очистке папки %temp%: {ex.Message}");
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
                    Console.WriteLine($"Не удалось удалить файл {file}: {ex.Message}");
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
                    Console.WriteLine($"Не удалось удалить папку {dir}: {ex.Message}");
                }
            }
        }
    }
}
