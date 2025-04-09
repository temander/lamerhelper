using System.Diagnostics;
using System.IO;
using System.Security.Cryptography;
using System.Windows;
using System.Windows.Controls;

namespace LamerHelper.Modules.Feature
{
    public partial class SSHGeneratorModule : UserControl, IModule
    {
        private readonly string _privateKeyPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".ssh", "id_rsa");
        private readonly string _publicKeyPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".ssh", "id_rsa.pub");

        public SSHGeneratorModule()
        {
            InitializeComponent();
        }

        public string ModuleName => "SSHGeneratorModule";
        public string DisplayName => "Генерация SSH ключей";
        public string Category => "Фишка";
        public string Description => "Генерирует SSH ключи для аутентификации, например, для GitHub.";
        public UserControl GetModuleControl() => this;

        private void ButtonGenerate_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                using var rsa = new RSACryptoServiceProvider(2048);
                
                rsa.PersistKeyInCsp = false;
                string privateKey = ConvertPrivateKeyToPEM(rsa);
                string publicKey = ConvertPublicKeyToOpenSSH(rsa);

                TextBoxPrivateKey.Text = privateKey;
                textBoxPublicKey.Text = publicKey;

                TextBoxPrivateKey.Visibility = Visibility.Visible;
                textBoxPublicKey.Visibility = Visibility.Visible;
                ButtonCopyPrivate.Visibility = Visibility.Visible;
                ButtonCopyPublic.Visibility = Visibility.Visible;
                ButtonSavePrivate.Visibility = Visibility.Visible;

                MessageBox.Show("SSH ключи успешно сгенерированы!", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка генерации SSH ключей: " + ex.Message, "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private static string ConvertPrivateKeyToPEM(RSACryptoServiceProvider rsa)
        {
            var privateKeyBytes = rsa.ExportRSAPrivateKey();
            return Convert.ToBase64String(privateKeyBytes, Base64FormattingOptions.InsertLineBreaks);
        }

        private static string ConvertPublicKeyToOpenSSH(RSACryptoServiceProvider rsa)
        {
            byte[] publicKeyBytes = rsa.ExportRSAPublicKey();
            return "ssh-rsa " + Convert.ToBase64String(publicKeyBytes);
        }

        private void ButtonCopyPublic_Click(object sender, RoutedEventArgs e)
        {
            Clipboard.SetText(textBoxPublicKey.Text);
            MessageBox.Show("Публичный ключ скопирован!", "Готово", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void ButtonCopyPrivate_Click(object sender, RoutedEventArgs e)
        {
            Clipboard.SetText(TextBoxPrivateKey.Text);
            MessageBox.Show("Приватный ключ скопирован!", "Готово", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void ButtonSavePrivate_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string sshFolderPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".ssh");
                if (!Directory.Exists(sshFolderPath))
                {
                    Directory.CreateDirectory(sshFolderPath);
                }

                File.WriteAllText(_privateKeyPath, TextBoxPrivateKey.Text);
                File.WriteAllText(_publicKeyPath, textBoxPublicKey.Text);

                // Настраиваем права доступа (важно для корректной работы SSH)
                File.SetAttributes(_privateKeyPath, FileAttributes.Normal);
                File.SetAttributes(_publicKeyPath, FileAttributes.Normal);

                MessageBox.Show("SSH ключи сохранены! Вы можете использовать их, например, для GitHub.", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);

                // Добавляем в ssh-agent
                Process.Start(new ProcessStartInfo
                {
                    FileName = "cmd.exe",
                    Arguments = $"/c ssh-add \"{_privateKeyPath}\"",
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                });

            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка при сохранении ключей: " + ex.Message, "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
