using LamerHelper.Modules;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using iNKORE.UI.WPF.Modern.Controls;

namespace LamerHelper
{
    public partial class MainWindow : Window
    {
        // Словарь для хранения содержимого для каждой категории
        private readonly Dictionary<string, UIElement> _categoryContents = new();

        public MainWindow()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            LoadModules();

            if (NavigationViewRoot.MenuItems.Count <= 0)
                return;

            NavigationViewRoot.SelectedItem = NavigationViewRoot.MenuItems[0];
            if (NavigationViewRoot.MenuItems[0] is NavigationViewItem firstItem)
            {
                NavigationViewRoot.Content = _categoryContents[firstItem.Tag.ToString() ?? string.Empty];
            }
        }

        private void LoadModules()
        {
            string configPath = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "ModuleConfig.json");
            List<IModule> modules = ModuleLoader.LoadModules(configPath);

            IEnumerable<IGrouping<string?, IModule>> groupedModules = modules.GroupBy(m => m.Category);

            foreach (var group in groupedModules)
            {
                ScrollViewer scrollViewer = new()
                {
                    VerticalScrollBarVisibility = ScrollBarVisibility.Auto,
                };

                StackPanel stackPanel = new()
                {
                    Margin = new Thickness(20, 10, 20, 10),
                    Orientation = Orientation.Vertical
                };

                foreach (var module in group)
                {
                    // Создаем заголовок с Expander
                    Expander expander = new()
                    {
                        Header = new TextBlock
                        {
                            Text = module.DisplayName,
                            FontSize = 16,
                            FontWeight = FontWeights.SemiBold,
                            Margin = new Thickness(4),
                        },
                        IsExpanded = false,
                        Background = new SolidColorBrush(Color.FromRgb(34, 34, 34)),
                        BorderBrush = new SolidColorBrush(Color.FromRgb(55, 55, 55)),
                        BorderThickness = new Thickness(1),
                        Padding = new Thickness(10)
                    };

                    StackPanel contentPanel = new()
                    {
                        Orientation = Orientation.Vertical,
                        Margin = new Thickness(0)
                    };

                    if (!string.IsNullOrWhiteSpace(module.Description))
                    {
                        TextBlock description = new()
                        {
                            Text = module.Description,
                            FontSize = 14,
                            Foreground = new SolidColorBrush(Color.FromRgb(160, 160, 160)),
                            TextWrapping = TextWrapping.Wrap,
                            Margin = new Thickness(0, -12, 0, 12)
                        };
                        contentPanel.Children.Add(description);
                    }

                    UserControl moduleControl = module.GetModuleControl();
                    moduleControl.Margin = new Thickness(0, 0, 0, 4);
                    contentPanel.Children.Add(moduleControl);

                    expander.Content = contentPanel;

                    Border card = new()
                    {
                        Background = new SolidColorBrush(Color.FromRgb(28, 28, 28)),
                        CornerRadius = new CornerRadius(12),
                        BorderBrush = new SolidColorBrush(Color.FromRgb(64, 64, 64)),
                        BorderThickness = new Thickness(1),
                        Margin = new Thickness(0, 0, 0, 14),
                        Child = expander,
                        Padding = new Thickness(6)
                    };

                    stackPanel.Children.Add(card);
                }

                scrollViewer.Content = stackPanel;
                if (group.Key == null) continue;
                _categoryContents[group.Key] = scrollViewer;

                NavigationViewItem navItem = new()
                {
                    Content = group.Key,
                    Tag = group.Key
                };

                NavigationViewRoot.MenuItems.Add(navItem);
            }
        }

        private void NavigationView_SelectionChanged(object sender, NavigationViewSelectionChangedEventArgs args)
        {
            if (NavigationViewRoot.SelectedItem is NavigationViewItem navItem &&
                navItem.Tag is string category &&
                _categoryContents.TryGetValue(category, out UIElement? content))
            {
                NavigationViewRoot.Content = content;
            }
        }
    }
}
