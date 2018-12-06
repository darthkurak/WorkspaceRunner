using WorkspaceRunner.Properties;
using System;
using System.Runtime.InteropServices;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using WinForms = System.Windows.Forms;
using Microsoft.Win32;
using System.IO;
using System.Windows.Interop;
using System.Diagnostics;
using System.Collections.ObjectModel;
using WorkspaceRunner.ViewModel;

namespace WorkspaceRunner
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

    [DllImport("user32.dll")]
    private static extern IntPtr GetSystemMenu(IntPtr hWnd, bool bRevert);
    [DllImport("user32.dll")]
    private static extern bool EnableMenuItem(IntPtr hMenu, uint uIDEnableItem, uint uEnable);
    [DllImport("user32.dll")]
    private static extern IntPtr DestroyMenu(IntPtr hWnd);

    private const uint MF_BYCOMMAND = 0x00000000;
    private const uint MF_GRAYED = 0x00000001;
    private const uint SC_CLOSE = 0xF060;

    IntPtr menuHandle;
        private IntPtr _windowHandle;

        public MainWindow()
        {
            InitializeComponent();

            WinForms.NotifyIcon ni = new WinForms.NotifyIcon();
            ni.MouseDown += new WinForms.MouseEventHandler(Notifier_MouseDown);
            ni.Icon = System.Drawing.Icon.ExtractAssociatedIcon(System.Reflection.Assembly.GetEntryAssembly().ManifestModule.Name);
            ni.Visible = true;
            ni.DoubleClick +=
                delegate (object sender, EventArgs args)
                {
                    this.Show();
                    this.WindowState = WindowState.Normal;
                };
            SourceInitialized += MainWindow_SourceInitialized;
            Loaded += (s, e) =>
            {
                if (DataContext is IHideable)
                {
                    (DataContext as IHideable).RequestHide += (_, __) => this.Dispatcher.Invoke(() => this.Hide());
                }
            };
        }

        private void MainWindow_SourceInitialized(object sender, EventArgs e)
        {
            _windowHandle = new WindowInteropHelper(this).Handle;
        }

        protected void DisableCloseButton()
        {
            if (_windowHandle == null)
                throw new InvalidOperationException("The window has not yet been completely initialized");

            menuHandle = GetSystemMenu(_windowHandle, false);
            if (menuHandle != IntPtr.Zero)
            {
                EnableMenuItem(menuHandle, SC_CLOSE, MF_BYCOMMAND | MF_GRAYED);
            }
        }

        void Notifier_MouseDown(object sender, WinForms.MouseEventArgs e)
        {
            if (e.Button == WinForms.MouseButtons.Right)
            {
                ContextMenu menu = (ContextMenu)this.FindResource("NotifierContextMenu");
                menu.IsOpen = true;
            }
        }
    }
}
