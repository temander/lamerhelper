using LamerHelper.Modules;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
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

        private void TrayButton_Click(object sender, RoutedEventArgs e)
        {
            Hide();
        }

        private void Taskbar_Click(object sender, RoutedEventArgs e)
        {
            Show();
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void LoadModules()
        {
            string configPath = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "ModuleConfig.json");
            List<IModule> modules = ModuleLoader.LoadModules(configPath);

            // Группируем модули по категориям
            var groupedModules = modules.GroupBy(m => m.Category);
            foreach (var group in groupedModules)
            {
                ScrollViewer scrollViewer = new ScrollViewer
                {
                    VerticalScrollBarVisibility = ScrollBarVisibility.Auto
                };

                StackPanel stackPanel = new StackPanel { Margin = new Thickness(10) };

                foreach (var module in group)
                {
                    Grid grid = new Grid();
                    grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
                    grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });

                    TextBlock header = new TextBlock
                    {
                        Text = module.DisplayName,
                        FontSize = 16,
                        Foreground = System.Windows.Media.Brushes.White,
                        Margin = new Thickness(0, 0, 0, 6),
                        VerticalAlignment = VerticalAlignment.Center
                    };
                    Grid.SetColumn(header, 0);

                    Button info = new Button
                    {
                        Content = "?",
                        FontSize = 16,
                        Width = 24,
                        Foreground = Brushes.Gray,
                        VerticalAlignment = VerticalAlignment.Center,
                        HorizontalAlignment = HorizontalAlignment.Right,
                        Style = (Style)FindResource("CustomButtonStyle"),
                        Tag = module
                    };
                    info.Click += InfoButton_Click;
                    Grid.SetColumn(info, 1);

                    UserControl control = module.GetModuleControl();
                    control.Margin = new Thickness(0, 4, 0, 0);

                    grid.Children.Add(header);
                    grid.Children.Add(info);

                    StackPanel modulePanel = new StackPanel { Margin = new Thickness(8) };
                    modulePanel.Children.Add(grid);
                    modulePanel.Children.Add(control);

                    stackPanel.Children.Add(modulePanel);
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

        // Обработчик нажатия на кнопку информации
        private void InfoButton_Click(object sender, RoutedEventArgs e)
        {
            Button currentBtn = sender as Button;
            Grid parentBtn = currentBtn.Parent as Grid;
            StackPanel parentStack = parentBtn.Parent as StackPanel;
            IModule btnModule = parentStack.Children[1] as IModule;

            MessageBox.Show($"{btnModule.Description}", "Информация");
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
