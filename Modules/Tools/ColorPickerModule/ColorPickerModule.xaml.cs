using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;

namespace LamerHelper.Modules.Tools
{
    public partial class ColorPickerModule : UserControl, IModule
    {
        public string ModuleName => "ColorPickerModule";
        public string DisplayName => "Цветоподборщик";
        public string Category => "Инструменты";
        public string Description => "Нажмите кнопку 'Выбрать цвет', затем перемещайте курсор для предпросмотра и нажмите левую кнопку мыши для захвата цвета. Нажмите правую – чтобы отменить.";
        public UserControl GetModuleControl() => this;

        private const int WhMouseLl = 14;
        private const int WmLbuttondown = 0x0201;
        private const int WmRbuttondown = 0x0204;

        private delegate IntPtr LowLevelMouseProc(int nCode, IntPtr wParam, IntPtr lParam);
        private readonly LowLevelMouseProc _mouseProc;
        private IntPtr _hookID = IntPtr.Zero;

        [DllImport("user32.dll", SetLastError = true)]
        private static extern IntPtr SetWindowsHookEx(int idHook, LowLevelMouseProc lpfn, IntPtr hMod, uint dwThreadId);

        [DllImport("user32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool UnhookWindowsHookEx(IntPtr hhk);

        [DllImport("user32.dll", SetLastError = true)]
        private static extern IntPtr CallNextHookEx(IntPtr hhk, int nCode, IntPtr wParam, IntPtr lParam);

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern IntPtr GetModuleHandle(string lpModuleName);

        private IntPtr SetHook(LowLevelMouseProc proc)
        {
            using Process curProcess = Process.GetCurrentProcess();
            using ProcessModule? curModule = curProcess.MainModule;
            return SetWindowsHookEx(WhMouseLl, proc, GetModuleHandle(curModule?.ModuleName ?? string.Empty), 0);
        }

        private IntPtr MouseHookCallback(int nCode, IntPtr wParam, IntPtr lParam)
        {
            if (nCode >= 0)
            {
                int msg = wParam.ToInt32();
                switch (msg)
                {
                    case WmLbuttondown:
                        Dispatcher.Invoke(() =>
                        {
                            StopColorPicking();
                            Color pickedColor = GetColorAtCursorPosition();
                            UpdateColorDisplay(pickedColor);
                            ColorInfoPanel.Visibility = Visibility.Visible;
                        });

                        return new IntPtr(1);
                    case WmRbuttondown:
                        Dispatcher.Invoke(StopColorPicking);
                        return new IntPtr(1);
                }
            }
            return CallNextHookEx(_hookID, nCode, wParam, lParam);
        }

        private DispatcherTimer? _colorUpdateTimer;

        public ColorPickerModule()
        {
            InitializeComponent();
            InitializePopup();
            InitializeColorUpdateTimer();

            _mouseProc = MouseHookCallback;
        }

        private Window? _colorInfoPopup;
        private Border? _popupColorSample;
        private TextBlock? _popupHexText;
        private TextBlock? _popupRgbText;

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
                Owner = Application.Current.MainWindow
            };

            var popupBorder = new Border
            {
                Background = new SolidColorBrush(Color.FromArgb(245, 25, 25, 25)),
                BorderBrush = Brushes.Gray,
                BorderThickness = new Thickness(1),
                CornerRadius = new CornerRadius(3),
                Padding = new Thickness(10)
            };

            var popupStackPanel = new StackPanel { Margin = new Thickness(5) };
            _popupColorSample = new Border { Height = 30, BorderBrush = Brushes.Black, BorderThickness = new Thickness(1), Margin = new Thickness(0, 0, 0, 5) };
            _popupHexText = new TextBlock { Margin = new Thickness(0, 2, 0, 2) };
            _popupRgbText = new TextBlock { Margin = new Thickness(0, 2, 0, 2) };

            popupStackPanel.Children.Add(_popupColorSample);
            popupStackPanel.Children.Add(_popupHexText);
            popupStackPanel.Children.Add(_popupRgbText);
            popupBorder.Child = popupStackPanel;

            _colorInfoPopup.Content = popupBorder;
        }

        private void InitializeColorUpdateTimer()
        {
            _colorUpdateTimer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(50) };
            _colorUpdateTimer.Tick += (sender, e) =>
            {
                Color currentColor = GetColorAtCursorPosition();
                UpdatePopupInfo(currentColor);
                UpdatePopupPosition();
            };
        }

        private void ButtonPickColor_Click(object sender, RoutedEventArgs e)
        {
            _hookID = SetHook(_mouseProc);
            _colorUpdateTimer?.Start();
            Mouse.OverrideCursor = Cursors.Cross;
            ColorInfoPanel.Visibility = Visibility.Collapsed;
            _colorInfoPopup?.Show();
        }

        private void ButtonCopyHex_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(TextBoxHex.Text)) return;
            Clipboard.SetText(TextBoxHex.Text);
            MessageBox.Show("HEX-код скопирован в буфер обмена!", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void ButtonCopyRgb_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(TextBoxRgb.Text)) return;
            Clipboard.SetText(TextBoxRgb.Text);
            MessageBox.Show("RGB-значение скопировано в буфер обмена!", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void StopColorPicking()
        {
            _colorUpdateTimer?.Stop();
            Mouse.OverrideCursor = null;
            if (_hookID != IntPtr.Zero)
            {
                UnhookWindowsHookEx(_hookID);
                _hookID = IntPtr.Zero;
            }
            _colorInfoPopup?.Hide();
        }

        private Color GetColorAtCursorPosition()
        {
            if (NativeMethods.GetCursorPos(out NativeMethods.Point p))
            {
                IntPtr hdc = NativeMethods.GetDC(IntPtr.Zero);
                try
                {
                    int colorRef = NativeMethods.GetPixel(hdc, p.X, p.Y);
                    if (colorRef != -1)
                    {
                        byte r = (byte)(colorRef & 0xFF);
                        byte g = (byte)((colorRef >> 8) & 0xFF);
                        byte b = (byte)((colorRef >> 16) & 0xFF);
                        return Color.FromRgb(r, g, b);
                    }
                }
                finally
                {
                    NativeMethods.ReleaseDC(IntPtr.Zero, hdc);
                }
            }
            return Colors.Black;
        }

        private void UpdatePopupInfo(Color color)
        {
            if (_popupColorSample != null) _popupColorSample.Background = new SolidColorBrush(color);
            if (_popupHexText != null) _popupHexText.Text = $"HEX: {color.R:X2}{color.G:X2}{color.B:X2}";
            if (_popupRgbText != null) _popupRgbText.Text = $"RGB: {color.R}, {color.G}, {color.B}";
        }

        private void UpdateColorDisplay(Color color)
        {
            TextBoxHex.Text = $"#{color.R:X2}{color.G:X2}{color.B:X2}";
            TextBoxRgb.Text = $"{color.R}, {color.G}, {color.B}";
            ColorSample.Background = new SolidColorBrush(color);
        }

        private void UpdatePopupPosition()
        {
            if (!NativeMethods.GetCursorPos(out var p)) return;
            var cursorPoint = new Point(p.X, p.Y);
            var source = PresentationSource.FromVisual(Application.Current.MainWindow);
            if (source != null)
            {
                var transform = source.CompositionTarget.TransformFromDevice;
                cursorPoint = transform.Transform(cursorPoint);
            }

            double offsetX = 20;
            double offsetY = 20;

            double screenWidth = SystemParameters.PrimaryScreenWidth;
            double screenHeight = SystemParameters.PrimaryScreenHeight;

            if (_colorInfoPopup != null && cursorPoint.X + _colorInfoPopup.Width + offsetX > screenWidth)
                offsetX = -(_colorInfoPopup.Width + 10);
            if (_colorInfoPopup != null && cursorPoint.Y + _colorInfoPopup.Height + offsetY > screenHeight)
                offsetY = -(_colorInfoPopup.Height + 10);

            if (_colorInfoPopup == null) return;
            _colorInfoPopup.Left = cursorPoint.X + offsetX;
            _colorInfoPopup.Top = cursorPoint.Y + offsetY;
        }

        internal static class NativeMethods
        {
            [StructLayout(LayoutKind.Sequential)]
            public struct Point
            {
                public int X;
                public int Y;
            }

            [DllImport("user32.dll")]
            public static extern bool GetCursorPos(out Point lpPoint);

            [DllImport("gdi32.dll", CharSet = CharSet.Auto, SetLastError = true)]
            public static extern int GetPixel(IntPtr hDc, int x, int y);

            [DllImport("user32.dll")]
            public static extern IntPtr GetDC(IntPtr hWnd);

            [DllImport("user32.dll")]
            public static extern int ReleaseDC(IntPtr hWnd, IntPtr hDc);
        }
    }
}
