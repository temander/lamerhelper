using System.Windows;
using System.Windows.Controls;
using LamerHelper.Modules;

namespace LamerHelper.Modules.Feature
{
    public partial class CursorBorderModule : UserControl, IModule
    {
        public CursorBorderModule()
        {
            InitializeComponent();
        }

        public string ModuleName => "CursorBorderModule";
        public string DisplayName => "Настройка цвета выделения мыши";
        public string Category => "Фишка";
        public UserControl GetModuleControl() => this;

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Открыт диалог настройки выделения мыши", "Фишка", MessageBoxButton.OK, MessageBoxImage.Information);
        }
    }
}
