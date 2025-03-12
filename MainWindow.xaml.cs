using LamerHelper.Modules;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using iNKORE.UI.WPF.Modern.Controls;

namespace LamerHelper
{
    public partial class MainWindow : Window
    {
        // Словарь для хранения содержимого для каждой категории
        private readonly Dictionary<string, UIElement> _categoryContents = [];

        public MainWindow()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            LoadModules();

            if (NavigationView_Root.MenuItems.Count > 0)
            {
                NavigationView_Root.SelectedItem = NavigationView_Root.MenuItems[0];
                if (NavigationView_Root.MenuItems[0] is NavigationViewItem firstItem)
                {
                    NavigationView_Root.Content = _categoryContents[firstItem.Tag.ToString()];
                }
            }
        }

        private void LoadModules()
        {
            string configPath = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "ModuleConfig.json");
            List<IModule> modules = ModuleLoader.LoadModules(configPath);

            var groupedModules = modules.GroupBy(m => m.Category);
            foreach (var group in groupedModules)
            {
                ScrollViewer scrollViewer = new()
                {
                    VerticalScrollBarVisibility = ScrollBarVisibility.Auto,
                };

                StackPanel stackPanel = new() { Margin = new Thickness(10) };

                foreach (var module in group)
                {
                    TextBlock headerText = new()
                    {
                        Text = module.DisplayName,
                        FontSize = 16,
                        FontWeight = FontWeights.Bold,
                        Margin = new Thickness(6, 0, 0, 6)
                    };

                    UserControl moduleControl = module.GetModuleControl();
                    moduleControl.Margin = new Thickness(4);

                    StackPanel modulePanel = new();
                    modulePanel.Children.Add(headerText);
                    modulePanel.Children.Add(moduleControl);

                    Border border = new()
                    {
                        BorderBrush = Brushes.Gray,
                        BorderThickness = new Thickness(1),
                        CornerRadius = new CornerRadius(5),
                        Margin = new Thickness(0, 0, 0, 10),
                        Padding = new Thickness(5, 6, 5, 5),
                        Child = modulePanel
                    };

                    stackPanel.Children.Add(border);
                }

                scrollViewer.Content = stackPanel;
                _categoryContents[group.Key] = scrollViewer;

                NavigationViewItem navItem = new()
                {
                    Content = group.Key,
                    Tag = group.Key
                };

                NavigationView_Root.MenuItems.Add(navItem);
            }
        }

        private void NavigationView_SelectionChanged(object sender, NavigationViewSelectionChangedEventArgs args)
        {
            if (NavigationView_Root.SelectedItem is NavigationViewItem navItem &&
                navItem.Tag is string category &&
                _categoryContents.TryGetValue(category, out UIElement? content))
            {
                NavigationView_Root.Content = content;
            }
        }
    }
}
