using LamerHelper.Modules;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Animation;

namespace LamerHelper
{
    public partial class MainWindow : Window
    {
        // Словарь для хранения содержимого для каждой категории
        private Dictionary<string, UIElement> _categoryContents = new Dictionary<string, UIElement>();

        public MainWindow()
        {
            InitializeComponent();
            LoadModules();
        }

        private void TitleBar_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
                DragMove();
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void LoadModules()
        {
            string configPath = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Modules", "ModuleConfig.json");
            List<IModule> modules = ModuleLoader.LoadModules(configPath);

            // Группируем модули по категориям
            var groupedModules = modules.GroupBy(m => m.Category);
            foreach (var group in groupedModules)
            {
                ScrollViewer scrollViewer = new ScrollViewer
                {
                    VerticalScrollBarVisibility = ScrollBarVisibility.Auto,
                    Background = (System.Windows.Media.Brush)FindResource("PrimaryBackground")
                };

                StackPanel stackPanel = new StackPanel { Margin = new Thickness(10) };

                foreach (var module in group)
                {
                    Border border = new Border
                    {
                        Background = (System.Windows.Media.Brush)FindResource("SecondaryBackground"),
                        Margin = new Thickness(5),
                        Padding = new Thickness(10),
                        CornerRadius = new CornerRadius(8)
                    };

                    TextBlock header = new TextBlock
                    {
                        Text = module.DisplayName,
                        FontSize = 16,
                        Foreground = System.Windows.Media.Brushes.White,
                        Margin = new Thickness(0, 0, 0, 5)
                    };

                    UserControl control = module.GetModuleControl();
                    StackPanel modulePanel = new StackPanel();
                    modulePanel.Children.Add(header);
                    modulePanel.Children.Add(control);

                    border.Child = modulePanel;
                    stackPanel.Children.Add(border);
                }
                scrollViewer.Content = stackPanel;

                _categoryContents[group.Key] = scrollViewer;

                NavigationListBox.Items.Add(group.Key);
            }

            if (NavigationListBox.Items.Count > 0)
            {
                NavigationListBox.SelectedIndex = 0;
            }
        }

        // Обработчик переключения категорий в навигации с анимацией
        private void NavigationListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (NavigationListBox.SelectedItem != null)
            {
                string selectedCategory = NavigationListBox.SelectedItem.ToString();
                if (_categoryContents.ContainsKey(selectedCategory))
                {
                    var newContent = _categoryContents[selectedCategory];

                    // Если уже что-то отображается, выполняем анимацию исчезновения
                    if (ContentArea.Content is UIElement currentContent)
                    {
                        var fadeOut = new DoubleAnimation(1, 0, new Duration(TimeSpan.FromMilliseconds(200)));
                        fadeOut.Completed += (s, a) =>
                        {
                            ContentArea.Content = newContent;
                            newContent.Opacity = 0;
                            var fadeIn = new DoubleAnimation(0, 1, new Duration(TimeSpan.FromMilliseconds(200)));
                            newContent.BeginAnimation(UIElement.OpacityProperty, fadeIn);
                        };
                        currentContent.BeginAnimation(UIElement.OpacityProperty, fadeOut);
                    }
                    else
                    {
                        ContentArea.Content = newContent;
                        newContent.Opacity = 0;
                        var fadeIn = new DoubleAnimation(0, 1, new Duration(TimeSpan.FromMilliseconds(200)));
                        newContent.BeginAnimation(UIElement.OpacityProperty, fadeIn);
                    }
                }
            }
        }
    }
}
