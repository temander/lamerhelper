using System;
using System.IO;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Resources;
using LamerHelper.Modules;
using LamerHelper.Modules.Feature;
using Microsoft.Win32;

namespace LamerHelper.Modules.Feature
{
    public partial class FolderColorizerModule : UserControl, IModule
    {
        public FolderColorizerModule()
        {
            InitializeComponent();
        }

        public string ModuleName => "FolderColorizerModule";
        public string DisplayName => "Изменение цвета папок";
        public string Category => "Фишка";
        public string Description => "Позволяет изменять цвет папок в контекстном меню windows";
        public UserControl GetModuleControl() => this;

        private void Button_ClickEnable(object sender, RoutedEventArgs e)
        {
            try
            {
                string targetDirectory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "lamerhelper", "Folders");

                if (!Directory.Exists(targetDirectory))
                    Directory.CreateDirectory(targetDirectory);

                string[] iconNames = { "red.ico", "green.ico", "purple.ico", "blue.ico", "default.ico", "orange.ico" };

                foreach (string iconName in iconNames)
                {
                    string resourceFile = $"pack://application:,,,/Images/Folders/{iconName}";
                    Uri resourceUri = new Uri(resourceFile, UriKind.Absolute);

                    StreamResourceInfo resourceStream = Application.GetResourceStream(resourceUri);
                    if (resourceStream != null)
                    {
                        string destinationPath = Path.Combine(targetDirectory, iconName);
                        using (FileStream fileStream = new FileStream(destinationPath, FileMode.Create, FileAccess.Write))
                        {
                            resourceStream.Stream.CopyTo(fileStream);
                        }
                    }
                }

                string scriptPath = Path.Combine(targetDirectory, "change_folder_icon.ps1");
                string psScript = @"param(
    [string]$iconColor,
    [string]$folderPath
)

$docPath = [Environment]::GetFolderPath(""MyDocuments"")
$iconFolder = ""$docPath\lamerhelper\Folders""
$iconPath = Join-Path $iconFolder ""$iconColor.ico""
$desktopIniPath = Join-Path $folderPath ""desktop.ini""
$iniContent = ""[.ShellClassInfo]`nIconResource=$iconPath,0""
Set-Content -Path $desktopIniPath -Value $iniContent -Force
attrib +h +s $desktopIniPath
Stop-Process -Name explorer -Force
Start-Process explorer.exe";

                File.WriteAllText(scriptPath, psScript, System.Text.Encoding.UTF8);

                AddContextMenuEntry(targetDirectory);

                MessageBox.Show("Иконки и PowerShell-скрипт успешно созданы!", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void AddContextMenuEntry(string targetDirectory)
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

                string[] colors = { "red", "green", "purple", "blue", "default", "orange" };
                foreach (string color in colors)
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
    }
}
