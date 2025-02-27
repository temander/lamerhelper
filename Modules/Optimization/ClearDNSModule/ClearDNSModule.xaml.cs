using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using LamerHelper.Modules;

namespace LamerHelper.Modules.Optimization
{
    public partial class ClearDNSModule : UserControl, IModule
    {
        public ClearDNSModule()
        {
            InitializeComponent();
        }

        public string ModuleName => "ClearDNSModule";
        public string DisplayName => "Очистить DNS кэш";
        public string Category => "Оптимизация";
        public string Description => "Temp - это временные файлы, что создаются самой Windows и процессами по ходу работы компьютера. С течением времени они могут копиться и достигать нескольких ГБайт.";
        public UserControl GetModuleControl() => this;

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Process.Start("ipconfig", "/flushdns");
            }
            finally
            {
                MessageBox.Show("DNS кэш очищен!", "Оптимизация", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }
    }
}
