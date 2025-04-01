using System.IO;
using Microsoft.Win32;
using System.Windows;
using System.Windows.Controls;

namespace LamerHelper.Modules.Feature
{
    public partial class FolderColorizerModule : UserControl, IModule
    {
        private const string ToggleStateRegistryKey = @"Software\LamerHelper\FolderColorizer";
        private const string ToggleStateRegistryValue = "ToggleState";
        private bool isInitializing;

        public FolderColorizerModule()
        {
            InitializeComponent();
            isInitializing = true;
            LoadToggleState();
            CheckCurrentState(); 
            isInitializing = false;
        }

        public string ModuleName => "FolderColorizerModule";
        public string DisplayName => "Изменение цвета папок";
        public string Category => "Фишка";
        public string Description => "Позволяет изменять цвет папок через контекстное меню Windows. Чтобы воспользоваться после активации – нажмите ПКМ по требуемой папке и выберите цвет.";
        public UserControl GetModuleControl() => this;

        private void CheckCurrentState()
        {
            string menuKey = @"Directory\shell\LamerHelper.ColorizeFolder";
            using var key = Registry.ClassesRoot.OpenSubKey(menuKey);
            toggleSwitch.IsOn = key != null;
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

        private void ToggleSwitch_Toggled(object sender, RoutedEventArgs e)
        {
            if (isInitializing) return;

            if (toggleSwitch.IsOn)
            {
                try
                {
                    string targetDirectory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "lamerhelper", "Folders");

                    if (!Directory.Exists(targetDirectory))
                        Directory.CreateDirectory(targetDirectory);

                    string[] iconNames = ["red.ico", "green.ico", "purple.ico", "blue.ico", "default.ico", "orange.ico"];

                    foreach (string iconName in iconNames)
                    {
                        string resourceFile = $"pack://application:,,,/Images/Folders/{iconName}";
                        Uri resourceUri = new(resourceFile, UriKind.Absolute);

                        StreamResourceInfo resourceStream = Application.GetResourceStream(resourceUri);
                        if (resourceStream == null) continue;

                        string destinationPath = Path.Combine(targetDirectory, iconName);
                        using FileStream fileStream = new(destinationPath, FileMode.Create, FileAccess.Write);
                        resourceStream.Stream.CopyTo(fileStream);
                    }

                    string scriptPath = Path.Combine(targetDirectory, "change_folder_icon.ps1");
                    string psScript = """
                                      param(
                                          [string]$iconColor,
                                          [string]$folderPath
                                      )
                                      $docPath = [Environment]::GetFolderPath("MyDocuments")
                                      $iconFolder = "$docPath\lamerhelper\Folders"
                                      $iconPath = Join-Path $iconFolder "$iconColor.ico"
                                      $desktopIniPath = Join-Path $folderPath "desktop.ini"
                                      if (Test-Path $desktopIniPath) {
                                          attrib -h -s $desktopIniPath
                                          Remove-Item $desktopIniPath -Force
                                      }
                                      @"
                                      [.ShellClassInfo]
                                      IconResource=$iconPath,0
                                      "@ | Out-File -FilePath $desktopIniPath -Encoding Unicode
                                      attrib +h +s $desktopIniPath
                                      attrib +r $folderPath
                                      Stop-Process -Name explorer -Force
                                      """;

                    File.WriteAllText(scriptPath, psScript, System.Text.Encoding.UTF8);

                    AddContextMenuEntry(targetDirectory);
                    SaveToggleState(true);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    toggleSwitch.IsOn = false;
                }
            }
            else
            {
                try
                {
                    string targetDirectory = Path.Combine(
                        Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
                        "lamerhelper",
                        "Folders");

                    RemoveContextMenuEntries();

                    if (Directory.Exists(targetDirectory))
                    {
                        string[] protectedFiles = Directory.GetFiles(targetDirectory, "desktop.ini", SearchOption.AllDirectories);
                        foreach (string file in protectedFiles)
                        {
                            File.SetAttributes(file, FileAttributes.Normal);
                        }

                        Directory.Delete(targetDirectory, true);
                    }

                    SaveToggleState(false);
                    MessageBox.Show("Функционал полностью удалён!", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка отключения: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    toggleSwitch.IsOn = true;
                }
            }
        }

        private static void AddContextMenuEntry(string targetDirectory)
        {
            try
            {
                string scriptPath = Path.Combine(targetDirectory, "change_folder_icon.ps1");

                string menuKey = @"Directory\shell\LamerHelper.ColorizeFolder";
                using (RegistryKey key = Registry.ClassesRoot.CreateSubKey(menuKey))
                {
                    key.SetValue("MUIVerb", "Изменить цвет папки");
                    key.SetValue("Icon", Path.Combine(targetDirectory, "default.ico"));
                    key.SetValue("Position", "Top");
                    key.SetValue("SubCommands", "");
                }

                string[] colors = ["red", "green", "purple", "blue", "default", "orange"];
                foreach (var color in colors)
                {
                    string colorKey = $@"{menuKey}\shell\{color}";
                    using (RegistryKey key = Registry.ClassesRoot.CreateSubKey(colorKey))
                    {
                        key.SetValue("MUIVerb", color);
                        key.SetValue("Icon", Path.Combine(targetDirectory, $"{color}.ico"));
                    }

                    string commandKey = $@"{colorKey}\command";
                    using (RegistryKey key = Registry.ClassesRoot.CreateSubKey(commandKey))
                    {
                        key.SetValue("", $"powershell.exe -ExecutionPolicy Bypass -File \"{scriptPath}\" \"{color}\" \"%1\"");
                    }
                }

                MessageBox.Show("Контекстное меню успешно добавлено!", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при добавлении записи в реестр: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private static void RemoveContextMenuEntries()
        {
            try
            {
                const string menuKey = @"Directory\shell\LamerHelper.ColorizeFolder";

                using (RegistryKey? key = Registry.ClassesRoot.OpenSubKey(menuKey, true))
                {
                    if (key != null)
                    {
                        Registry.ClassesRoot.DeleteSubKeyTree(menuKey);
                    }
                }

                using RegistryKey? shellIconCache = Registry.CurrentUser.OpenSubKey(
                    @"Software\Microsoft\Windows\CurrentVersion\Explorer\ShellIconOverlayIdentifiers",
                    true);
                if (shellIconCache == null) return;

                foreach (var subKey in shellIconCache.GetSubKeyNames()
                             .Where(name => name.StartsWith("LamerHelperIcon")))
                {
                    shellIconCache.DeleteSubKeyTree(subKey);
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Ошибка очистки реестра: {ex.Message}");
            }
        }
    }
}