using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Threading;
using System.Runtime.InteropServices;
using System.Windows.Interop;
using Forms = System.Windows.Forms;
using System.Diagnostics;

namespace Ninput
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        [DllImport("user32.dll")]
        internal static extern bool RegisterHotKey(IntPtr hWnd, int id, uint fsModifiers, uint vk);

        [DllImport("user32.dll")]
        internal static extern bool UnregisterHotKey(IntPtr hWnd, int id);

        private const int HOTKEY_ID = 9000;  
        private const int WM_HOTKEY = 0x0312;

        private List<OverlayWindow> _overlays = new List<OverlayWindow>();
        private bool _isBlocked = false;

        private uint _currentModifier = 0x0001;
        private uint _currentKey = 0x4B;

        public MainWindow()
        {
            InitializeComponent();
        }

        protected override void OnSourceInitialized(EventArgs e)
        {
            base.OnSourceInitialized(e);

            _currentModifier = Properties.Settings.Default.Modifier;
            _currentKey = Properties.Settings.Default.Key;
            InputName.Text = $"Keys: {Properties.Settings.Default.KeyName}";

            IntPtr handle = new WindowInteropHelper(this).Handle;
            HwndSource source = HwndSource.FromHwnd(handle);
            source.AddHook(HwndHook);

            RegisterHotKey(handle, HOTKEY_ID, _currentModifier, _currentKey);
        }

        private IntPtr HwndHook(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled) 
        {
            if (msg == WM_HOTKEY && wParam.ToInt32() == HOTKEY_ID) 
            {
                ChangeBlocker();
                handled = true;
            }

            return IntPtr.Zero;
        }

        private void ChangeBlocker() 
        {
            _isBlocked = !_isBlocked;

            if (_isBlocked)
            {
                this.Hide();

                foreach (var screen in Forms.Screen.AllScreens) 
                {
                    var overlay = new OverlayWindow
                    {
                        WindowStartupLocation = WindowStartupLocation.Manual,
                        WindowStyle = WindowStyle.None,
                        ResizeMode = ResizeMode.NoResize,
                        Topmost = true
                    };

                    var workingArea = screen.Bounds;

                    var source = PresentationSource.FromVisual(this);
                    double dpiX = 1.0, dpiY = 1.0;
                    if (source?.CompositionTarget != null) 
                    {
                        dpiX = source.CompositionTarget.TransformToDevice.M11;
                        dpiY = source.CompositionTarget.TransformToDevice.M22;
                    }

                    overlay.Left = workingArea.X / dpiX;
                    overlay.Top = workingArea.Y / dpiY;
                    overlay.Width = workingArea.Width / dpiX;
                    overlay.Height = workingArea.Height / dpiY;

                    overlay.Show();

                    // Brute force this shit
                    overlay.Activate();
                    overlay.Topmost = true; 

                    _overlays.Add(overlay);
                }
            }
            else
            {
                this.Show();

                foreach (var overlay in _overlays) 
                {
                    overlay.CanClose = true;
                    overlay.Close();
                }
                _overlays.Clear();
            }
        }

        protected override void OnClosed(EventArgs e)
        {
            IntPtr handle = new WindowInteropHelper(this).Handle;
            UnregisterHotKey(handle, HOTKEY_ID);
            base.OnClosed(e);
        }

        protected override void OnStateChanged(EventArgs e)
        {
            if(_isBlocked && this.WindowState != WindowState.Minimized) 
            {
                this.Hide();
            }

            base.OnStateChanged(e);
        }

        private void BtnSettings_Click(object sender, RoutedEventArgs e)
        {
            var settings = new SettingsWindow();
            settings.Owner = this;

            if (settings.ShowDialog() == true) 
            {
                IntPtr handle = new WindowInteropHelper(this).Handle;
                UnregisterHotKey(handle, HOTKEY_ID);

                _currentModifier = settings.SelectedModifier;
                _currentKey = settings.SelectedKey;

                RegisterHotKey(handle, HOTKEY_ID, _currentModifier, _currentKey);

                InputName.Text = $"Keys: {settings.KeyName}";
            }
        }
    }
}