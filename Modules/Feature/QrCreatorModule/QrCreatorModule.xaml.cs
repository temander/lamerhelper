using System.Net.Http;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using Microsoft.Win32;

namespace LamerHelper.Modules.Feature
{
    public partial class QrCreatorModule : UserControl, IModule
    {
        public QrCreatorModule()
        {
            InitializeComponent();
        }

        public string ModuleName => "QrCreatorModule";
        public string DisplayName => "Создание QR-кода из любого текста";
        public string Category => "Фишка";
        public string Description => "Позволяет создать QR-код из любого текста";
        public UserControl GetModuleControl() => this;

        private async void ButtonGenerate_Click(object sender, RoutedEventArgs e)
        {
            string inputText = textBoxInput.Text;

            if (string.IsNullOrEmpty(inputText))
            {
                MessageBox.Show("Введите текст или ссылку!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            string apiUrl = $"https://api.qrserver.com/v1/create-qr-code/?size=150x150&data={Uri.EscapeDataString(inputText)}";

            using (var client = new HttpClient())
            {
                try
                {
                    HttpResponseMessage response = await client.GetAsync(apiUrl);

                    if (response.IsSuccessStatusCode)
                    {
                        byte[] imageBytes = await response.Content.ReadAsByteArrayAsync();

                        var bitmapImage = new BitmapImage();
                        bitmapImage.BeginInit();
                        bitmapImage.StreamSource = new System.IO.MemoryStream(imageBytes);
                        bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
                        bitmapImage.EndInit();

                        imageQR.Source = bitmapImage;
                        imageQR.Visibility = Visibility.Visible;
                        buttonSave.Visibility = Visibility.Visible;
                        buttonCopy.Visibility = Visibility.Visible;

                        MessageBox.Show("QR-код успешно создан!", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                    else
                    {
                        MessageBox.Show("Ошибка при создании QR-кода: " + response.StatusCode, "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Произошла ошибка: " + ex.Message, "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void ButtonSave_Click(object sender, RoutedEventArgs e)
        {
            if (imageQR.Source == null) return;
            
            SaveFileDialog saveDialog = new SaveFileDialog
            {
                Filter = "PNG Image|*.png",
                FileName = "qrcode.png"
            };

            if (saveDialog.ShowDialog() != true) return;
            
            var encoder = new PngBitmapEncoder();
            encoder.Frames.Add(BitmapFrame.Create((BitmapSource)imageQR.Source));

            using (var stream = saveDialog.OpenFile())
            {
                encoder.Save(stream);
            }

            MessageBox.Show("QR-код сохранен!", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void ButtonCopy_Click(object sender, RoutedEventArgs e)
        {
            if (imageQR.Source == null) return;
            BitmapSource bitmapSource = (BitmapSource)imageQR.Source;
            Clipboard.SetImage(bitmapSource);
            MessageBox.Show("QR-код скопирован в буфер обмена!", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
        }
    }
}