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
        private const uint MOD_ALT = 0x0001; 
        private const uint VK_K = 0x4B;      
        private const int WM_HOTKEY = 0x0312;

        private OverlayWindow _overlay;
        private bool _isBlocked = false;

        private uint currentModifier = 0x0001;
        private uint currentKey = 0x4B;      

        public MainWindow()
        {
            InitializeComponent();
        }

        protected override void OnSourceInitialized(EventArgs e)
        {
            base.OnSourceInitialized(e);

            IntPtr handle = new WindowInteropHelper(this).Handle;
            HwndSource source = HwndSource.FromHwnd(handle);
            source.AddHook(HwndHook);

            RegisterHotKey(handle, HOTKEY_ID, MOD_ALT, VK_K);
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
                _overlay = new OverlayWindow();
                _overlay.Show();
                this.Title = "Ninput: INPUT DISABLED";
            }
            else
            {
                _overlay?.Close();
                this.Title = "Ninput";
            }
        }

        protected override void OnClosed(EventArgs e)
        {
            IntPtr handle = new WindowInteropHelper(this).Handle;
            UnregisterHotKey(handle, HOTKEY_ID);
            base.OnClosed(e);
        }

        private void BtnSettings_Click(object sender, RoutedEventArgs e)
        {
            var settings = new SettingsWindow();
            settings.Owner = this;

            if (settings.ShowDialog() == true) 
            {
                IntPtr handle = new WindowInteropHelper(this).Handle;
                UnregisterHotKey(handle, HOTKEY_ID);

                currentModifier = settings.SelectedModifier;
                currentKey = settings.SelectedKey;

                RegisterHotKey(handle, HOTKEY_ID, currentModifier, currentKey);

                InputName.Text = $"Keys: {settings.KeyName}";
            }
        }
    }
}