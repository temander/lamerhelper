using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;
using System.Windows.Interop;


namespace LamerHelper.Modules.Feature
{
    public partial class ColorPickerModule : UserControl, IModule
    {
        // Для всплывающего окна с информацией о цвете
        private Window _colorInfoPopup;
        private TextBlock _popupHexText;
        private TextBlock _popupRgbText;
        private Border _popupColorSample;
        
        // Флаг состояния выбора цвета
        private bool _isPickingColor;
        
        // Таймер для обновления информации о цвете
        private DispatcherTimer _colorUpdateTimer;
        
        // P/Invoke для работы с Windows API
        [DllImport("user32.dll")]
        private static extern bool GetCursorPos(out POINT lpPoint);
        
        [DllImport("gdi32.dll", CharSet = CharSet.Auto, SetLastError = true, ExactSpelling = true)]
        private static extern int GetPixel(IntPtr hDC, int x, int y);
        
        [DllImport("user32.dll")]
        private static extern IntPtr GetDC(IntPtr hWnd);
        
        [DllImport("user32.dll")]
        private static extern int ReleaseDC(IntPtr hWnd, IntPtr hDC);
        
        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool SetForegroundWindow(IntPtr hWnd);
        
        [DllImport("user32.dll")]
        private static extern IntPtr GetForegroundWindow();
        
        [DllImport("user32.dll")]
        private static extern IntPtr WindowFromPoint(int xPoint, int yPoint);
        
        private IntPtr _mainWindowHandle;
        private bool _isApplicationActive = true;
        
        [StructLayout(LayoutKind.Sequential)]
        private struct POINT
        {
            public int X;
            public int Y;
        }
        
        public ColorPickerModule()
        {
            InitializeComponent();
            InitializePopup();
            InitializeColorUpdateTimer();
            
            if (Application.Current.MainWindow != null)
            {
                _mainWindowHandle = new WindowInteropHelper(Application.Current.MainWindow).Handle;
            }
            
            // Событие для отслеживания состояния активности приложения
            Application.Current.Activated += (s, e) => _isApplicationActive = true;
            Application.Current.Deactivated += (s, e) => _isApplicationActive = false;
        }
        
        // Свойства интерфейса IModule
        public string ModuleName => "ColorPickerModule";
        public string DisplayName => "Цветоподборщик";
        public string Category => "Фишка";
        public string Description => "Позволяет выбрать цвет с любой точки экрана и скопировать его значение";
        public UserControl GetModuleControl() => this;
        
        private void InitializePopup()
        {
            _colorInfoPopup = new Window
            {
                Width = 200,
                Height = 100,
                WindowStyle = WindowStyle.None,
                AllowsTransparency = true,
                Background = Brushes.Transparent,
                Topmost = true,
                ShowInTaskbar = false,
                ResizeMode = ResizeMode.NoResize,
                SizeToContent = SizeToContent.WidthAndHeight,
                Owner = Application.Current.MainWindow // Устанавливаем владельца для корректного поведения
            };
            
            // Создание содержимого всплывающего окна
            var popupGrid = new Grid();
            var popupBorder = new Border
            {
                Background = new SolidColorBrush(Color.FromArgb(230, 245, 245, 245)),
                BorderBrush = Brushes.Gray,
                BorderThickness = new Thickness(1),
                CornerRadius = new CornerRadius(3),
                Padding = new Thickness(10)
            };
            
            var popupStackPanel = new StackPanel { Margin = new Thickness(5) };
            
            // Образец цвета
            _popupColorSample = new Border
            {
                Height = 30,
                BorderBrush = Brushes.Black,
                BorderThickness = new Thickness(1),
                Margin = new Thickness(0, 0, 0, 5)
            };
            
            _popupHexText = new TextBlock { Margin = new Thickness(0, 2, 0, 2) };
            _popupRgbText = new TextBlock { Margin = new Thickness(0, 2, 0, 2) };
            
            popupStackPanel.Children.Add(_popupColorSample);
            popupStackPanel.Children.Add(_popupHexText);
            popupStackPanel.Children.Add(_popupRgbText);
            
            popupBorder.Child = popupStackPanel;
            popupGrid.Children.Add(popupBorder);
            
            _colorInfoPopup.Content = popupGrid;
        }
        
        private void InitializeColorUpdateTimer()
        {
            _colorUpdateTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromMilliseconds(50)
            };
            
            _colorUpdateTimer.Tick += (sender, e) =>
            {
                if (_isPickingColor)
                {
                    UpdateColorFromCursorPosition();
                }
            };
        }
        
        private void ButtonPickColor_Click(object sender, RoutedEventArgs e)
        {
            _isPickingColor = true;
            _colorUpdateTimer.Start();
            
            // Скрыть информацию о прошлом выборе
            colorInfoPanel.Visibility = Visibility.Collapsed;
            
            // Изменение курсора для индикации режима выбора цвета
            Mouse.OverrideCursor = Cursors.Cross;
            
            // Подписка на событие нажатия мыши - исправлено, убран третий параметр
            Mouse.AddMouseDownHandler(Application.Current.MainWindow, MainWindow_MouseLeftButtonDown);
            Mouse.AddMouseDownHandler(Application.Current.MainWindow, MainWindow_MouseRightButtonDown);
            
            // Обработка глобальных событий мыши через событие PreviewMouseDown окна
            var mainWindow = Application.Current.MainWindow;
            if (mainWindow != null)
            {
                mainWindow.PreviewMouseDown += MainWindow_PreviewMouseDown;
            }
            
            // Показать всплывающее окно
            _colorInfoPopup.Show();
        }
        
        // Новый метод вместо использования SystemEvents
        private void MainWindow_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (_isPickingColor && !_isApplicationActive)
            {
                StopColorPicking();
                
                // Возвращаем фокус главному окну
                if (_mainWindowHandle != IntPtr.Zero)
                {
                    SetForegroundWindow(_mainWindowHandle);
                }
            }
        }
        
        private void MainWindow_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (_isPickingColor)
            {
                StopColorPicking();
                
                // Получить цвет и показать информацию
                Color pickedColor = GetColorAtCursorPosition();
                UpdateColorDisplay(pickedColor);
                colorInfoPanel.Visibility = Visibility.Visible;
            }
        }
        
        private void MainWindow_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (_isPickingColor)
            {
                StopColorPicking();
            }
        }
        
        private void StopColorPicking()
        {
            _isPickingColor = false;
            _colorUpdateTimer.Stop();
            Mouse.OverrideCursor = null;
            
            Mouse.RemoveMouseDownHandler(Application.Current.MainWindow, MainWindow_MouseLeftButtonDown);
            Mouse.RemoveMouseDownHandler(Application.Current.MainWindow, MainWindow_MouseRightButtonDown);
            
            // Отписываемся от события PreviewMouseDown
            var mainWindow = Application.Current.MainWindow;
            if (mainWindow != null)
            {
                mainWindow.PreviewMouseDown -= MainWindow_PreviewMouseDown;
            }
            
            _colorInfoPopup.Hide();
        }
        
        private void UpdateColorFromCursorPosition()
        {
            Color currentColor = GetColorAtCursorPosition();
            UpdatePopupInfo(currentColor);
            
            // Обновляем позицию всплывающего окна с учетом DPI и разрешений экрана
            if (GetCursorPos(out POINT p))
            {
                // Преобразование позиции курсора с учетом масштабирования
                Point cursorPoint = new Point(p.X, p.Y);
                
                // Получаем дескриптор окна, на котором находится курсор
                IntPtr hWnd = WindowFromPoint(p.X, p.Y);
                
                // Преобразуем координаты из экранных в координаты приложения
                PresentationSource source = PresentationSource.FromVisual(Application.Current.MainWindow);
                if (source != null)
                {
                    Matrix transformMatrix = source.CompositionTarget.TransformFromDevice;
                    cursorPoint = transformMatrix.Transform(cursorPoint);
                }
                
                // Устанавливаем позицию окна с учетом размеров экрана
                double screenWidth = SystemParameters.PrimaryScreenWidth;
                double screenHeight = SystemParameters.PrimaryScreenHeight;
                
                double offsetX = 20;
                double offsetY = 20;
                
                // Проверяем, чтобы всплывающее окно не выходило за пределы экрана
                if (cursorPoint.X + _colorInfoPopup.Width + offsetX > screenWidth)
                    offsetX = -(_colorInfoPopup.Width + 10);
                    
                if (cursorPoint.Y + _colorInfoPopup.Height + offsetY > screenHeight)
                    offsetY = -(_colorInfoPopup.Height + 10);
                
                _colorInfoPopup.Left = cursorPoint.X + offsetX;
                _colorInfoPopup.Top = cursorPoint.Y + offsetY;
            }
        }
        
        private Color GetColorAtCursorPosition()
        {
            if (GetCursorPos(out POINT p))
            {
                IntPtr hdc = GetDC(IntPtr.Zero);
                try
                {
                    int colorRef = GetPixel(hdc, p.X, p.Y);
                    if (colorRef != -1) // -1 означает ошибку
                    {
                        byte r = (byte)(colorRef & 0xFF);
                        byte g = (byte)((colorRef >> 8) & 0xFF);
                        byte b = (byte)((colorRef >> 16) & 0xFF);
                        return Color.FromRgb(r, g, b);
                    }
                }
                finally
                {
                    ReleaseDC(IntPtr.Zero, hdc);
                }
            }
            
            return Colors.Black; // По умолчанию
        }
        
        private void UpdatePopupInfo(Color color)
        {
            _popupColorSample.Background = new SolidColorBrush(color);
            _popupHexText.Text = $"HEX: {color.R:X2}{color.G:X2}{color.B:X2}";
            _popupRgbText.Text = $"RGB: {color.R}, {color.G}, {color.B}";
        }
        
        private void UpdateColorDisplay(Color color)
        {
            textBoxHex.Text = $"#{color.R:X2}{color.G:X2}{color.B:X2}";
            textBoxRgb.Text = $"{color.R}, {color.G}, {color.B}";
            colorSample.Background = new SolidColorBrush(color);
        }
        
        private void ButtonCopyHex_Click(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrEmpty(textBoxHex.Text))
            {
                Clipboard.SetText(textBoxHex.Text);
                MessageBox.Show("HEX-код скопирован в буфер обмена!", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }
        
        private void ButtonCopyRgb_Click(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrEmpty(textBoxRgb.Text))
            {
                Clipboard.SetText(textBoxRgb.Text);
                MessageBox.Show("RGB-значение скопировано в буфер обмена!", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }
    }
}
