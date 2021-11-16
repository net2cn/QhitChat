using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media.Imaging;

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
            var avatar = await Core.API.File.GetAvatarAsync(Core.Configuration.Account, Core.Configuration.Token);
            if (avatar != null)
            {
                BitmapImage img = new BitmapImage();
                using (MemoryStream memStream = new MemoryStream(avatar))
                {
                    img.BeginInit();
                    img.CacheOption = BitmapCacheOption.OnLoad;
                    img.StreamSource = memStream;
                    img.EndInit();
                    img.Freeze();
                }
                UserProfileImageBrush.ImageSource = img;
            }
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
