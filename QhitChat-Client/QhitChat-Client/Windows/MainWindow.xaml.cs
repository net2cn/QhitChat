using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Interop;
using SharpVectors.Converters;
using SharpVectors.Renderers.Wpf;

namespace QhitChat_Client.Windows
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        private List<string> relationship;

        public MainWindow()
        {
            SourceInitialized += Window_SourceInitialized;
            InitializeComponent();
        }

        private async void Window_Loaded(object sender, RoutedEventArgs e)
        {
            TitleBar.Title = Core.Configuration.TITLE;
            relationship = await Core.API.Relationship.GetRelationshipAsync(Core.Configuration.Account, Core.Configuration.Token);
        }

        private void Window_SourceInitialized(object sender, EventArgs e)
        {
            IntPtr handle = new WindowInteropHelper(this).Handle;
            HwndSource.FromHwnd(handle)?.AddHook(WindowProc);
        }

        private IntPtr WindowProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            switch (msg)
            {
                case 0x0024:
                    Native.WmGetMinMaxInfo(hwnd, lParam, (int)MinWidth, (int)MinHeight);
                    handled = true;
                    break;
            }

            return (IntPtr)0;
        }

        private void MenuToggleButton_Click(object sender, RoutedEventArgs e)
        {

        }
    }
}
