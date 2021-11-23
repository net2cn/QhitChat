using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
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
        private ObservableCollection<User> _contacts { get; set; }

        public ObservableCollection<User> Users { get; set; }

        public User? SelectedUser { get; set; }

        private string? _searchKeyword;
        public string? SearchKeyword
        {
            get => _searchKeyword;
            set
            {
                _searchKeyword = value;
                if (!String.IsNullOrEmpty(value))
                {
                    SearchContacts();
                }
                else
                {
                    Users.Clear();
                    foreach ( var i in _contacts)
                    {
                        Users.Add(i);
                    }
                }
            }
        }

        private string avatarPath = "./cache/avatar.txt";
        private Dictionary<string, string> relationship;

        public MainWindow()
        {
            SourceInitialized += Window_SourceInitialized;
            DataContext = this;
            Users = new ObservableCollection<User>();

            InitializeComponent();
        }

        private async void Window_Loaded(object sender, RoutedEventArgs e)
        {
            // Subscribe to network connection events.
            Core.Configuration.Network.RaiseNetworkEvent += OnJsonRpcDisconnected;

            TitleBar.Title = Core.Configuration.TITLE;
            UpdateUserProfileAsync();
            UpdateContactsAsync();
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
            UserAvatarImageBrush.ImageSource = new BitmapImage(new Uri(Path.GetFullPath(avatarFilepath)));
        }

        private async Task UpdateContactsAsync()
        {
            relationship = await Core.API.Relationship.GetRelationshipAsync(Core.Configuration.Account, Core.Configuration.Token);

            foreach(var i in relationship)
            {
                Users.Add(new User(i.Key, i.Value));
                await Users.Last().GetUserProfileImageAsync();
            }
            _contacts = new ObservableCollection<User>(Users);
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

        private async void SearchContacts()
        {
            var userMatched = await Core.API.Authentication.FindUserAsync(SearchKeyword, 0);
            Users.Clear();
            foreach (var i in userMatched)
            {
                Users.Add(new User(i.Key, i.Value));
                await Users.Last().GetUserProfileImageAsync();
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

        private async void OnJsonRpcDisconnected(object sender, Core.NetworkEventArgs e)
        {
            DisplayMessage("网络连接已断开！应用程序将于5s后自动退出。");
            await Task.Delay(5000);
            Close();
        }

        private void DisplayMessage(string message)
        {
            MainSnackbar.MessageQueue?.Enqueue(message);
        }

        private void ContactsListBox_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            if (SelectedUser != null)
            {
                Trace.WriteLine(SelectedUser.Account);
            }
        }
    }

    public class User : INotifyPropertyChanged
    {
        public string Account { get; set; }
        public string Username { get; set; }
        public BitmapImage Avatar { get; set; }

        public event PropertyChangedEventHandler PropertyChanged;

        public User(string account, string username)
        {
            Account = account;
            Username = username;
        }

        public async Task GetUserProfileImageAsync()
        {
            // TODO: Add user avatar cache.
            var avatar = await Core.API.File.GetAvatarAsync(Account);
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
                Avatar = new BitmapImage(new Uri(Path.GetFullPath(avatarFilepath)));
            }
        }

        private void NotifyPropertyChanged([CallerMemberName] String propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    public class NullVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value == null ? Visibility.Hidden : Visibility.Visible;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}