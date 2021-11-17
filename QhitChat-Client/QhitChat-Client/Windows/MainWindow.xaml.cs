using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media.Imaging;
using QhitChat_Client.Presistent.Filesystem;

namespace QhitChat_Client.Windows
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        private string avatarPath = "./cache/avatar.txt";
        private Dictionary<string, string> relationship;

        public MainWindow()
        {
            SourceInitialized += Window_SourceInitialized;
            InitializeComponent();
        }

        private async void Window_Loaded(object sender, RoutedEventArgs e)
        {
            TitleBar.Title = Core.Configuration.TITLE;
            relationship = await Core.API.Relationship.GetRelationshipAsync(Core.Configuration.Account, Core.Configuration.Token);
            UpdateUserProfileAsync();
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

        private async Task UpdateUserProfileAsync()
        {
            string avatarFilepath = "";

            if (!Filesystem.Exists(avatarPath))
            {
                avatarFilepath = await GetUserProfileImageAsync(Core.Configuration.Account);
                Filesystem.WriteLine(avatarPath, avatarFilepath);
            }
            else
            {
                avatarFilepath = await Filesystem.ReadLineAsync(avatarPath, 1);
                if (!await Core.API.File.IsAvatarMatchedAsync(Core.Configuration.Account, Path.GetFileName(avatarFilepath)))
                {
                    avatarFilepath = await GetUserProfileImageAsync(Core.Configuration.Account);
                }
            }

            UsernameTextBox.Text = Core.Configuration.Username;
            UserProfileImageBrush.ImageSource = new BitmapImage(new Uri(Path.GetFullPath(avatarFilepath)));
        }

        private async Task<string> GetUserProfileImageAsync(string account)
        {
            var avatar = await Core.API.File.GetAvatarAsync(account);
            if (avatar != null)
            {
                var avatarFilename = avatar.First().Key;
                var avatarFilepath = Path.Combine("./cache/avatars/", avatarFilename);
                var avatarData = avatar.First().Value;
                BitmapImage img = new BitmapImage();
                using (MemoryStream memStream = new MemoryStream(avatarData))
                {
                    img.BeginInit();
                    img.CacheOption = BitmapCacheOption.OnLoad;
                    img.StreamSource = memStream;
                    img.EndInit();
                    img.Freeze();
                }
                Filesystem.CreateEmptyFile(avatarFilepath, avatarData.Length);
                Filesystem.SaveFileByChunckNumber(avatarFilepath, avatarData, 0);
                return avatarFilepath;
            }
            return null;
        }

        private async void ContactsSearchBox_LostFocus(object sender, RoutedEventArgs e)
        {
            if (ContactsSearchBox.Text.Length > 0)
            {
                var userMatched = await Core.API.Authentication.FindUserAsync(ContactsSearchBox.Text);
            }
        }

        private async void UsernameTextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            if (UsernameTextBox.Text != Core.Configuration.Username)
            {
                if (UsernameTextBox.Text.Length > 1 && UsernameTextBox.Text.Length < 33)
                {
                    Core.Configuration.Username = UsernameTextBox.Text;
                    if(await Core.API.Authentication.ChangeUsernameAsync(Core.Configuration.Account, Core.Configuration.Token, Core.Configuration.Username))
                    {
                        // Successfully changed username
                        DisplayMessage("已更改用户名！");
                        return;
                    }
                }

                // Unchanged
                DisplayMessage("用户名未更改，请检查后重试。");
            }
        }

        private void DisplayMessage(string message)
        {
            MainSnackbar.MessageQueue?.Enqueue(message);
        }
    }
}