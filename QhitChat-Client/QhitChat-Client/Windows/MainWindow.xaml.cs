using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
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

        private ObservableCollection<Presistent.Database.Models.Messages> _currentMessageQuene { get; set; }
        public ObservableCollection<Presistent.Database.Models.Messages> CurrentMessageQuene
        {
            get => _currentMessageQuene;
            set
            {
                _currentMessageQuene = value;
                ChatBoxListBox.ItemsSource = _currentMessageQuene;
            }
        }

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
            Core.Configuration.Notification.Contacts.CollectionChanged += OnNewContactsAdded;

            TitleBar.Title = Core.Configuration.TITLE;
            UpdateUserProfileAsync();
            await UpdateContactsAsync();
            await FetchNewMessagesAsync();
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
            // Update username
            Core.Configuration.Username = await Core.API.Authentication.GetUsernameAsync(Core.Configuration.Account);
            UpdateUsername(Core.Configuration.Account, Core.Configuration.Username);

            // Update avatar
            var avatarFilepath = await UpdateAvatarAsync(Core.Configuration.Account);

            usernameTextBox.Text = Core.Configuration.Username;
            UserAvatarImageBrush.ImageSource = new BitmapImage(new Uri(Path.GetFullPath(avatarFilepath)));
        }

        private async Task UpdateContactsAsync()
        {
            relationship = await Core.API.Relationship.GetRelationshipAsync(Core.Configuration.Account, Core.Configuration.Token);

            foreach(var i in relationship)
            {
                Users.Add(new User(i.Key, i.Value));
                await Users.Last().UpdateUserAvatarAsync();
                Core.Configuration.Notification.AddQuene(i.Key);
            }
            _contacts = new ObservableCollection<User>(Users);  // Make a copy of contacts.
        }

        private async Task FetchNewMessagesAsync()
        {
            var messages = await Core.API.Chat.FetchAsync(Core.Configuration.Account, Core.Configuration.Token);
            foreach (var message in messages)
            {
                Core.Configuration.Notification.NewMessage(message);
            }
        }

        private void UpdateUsername(string account, string username)
        {
            var user = (from r in Presistent.Presistent.DatabaseContext.User
                        where r.Account == account
                        select r).SingleOrDefault();

            if (user == null)
            {
                user = new Presistent.Database.Models.User { Account = account, Username = username };
                Presistent.Presistent.DatabaseContext.User.Add(user);
                Presistent.Presistent.DatabaseContext.SaveChanges();
            }
            else
            {
                if (user.Username != username)
                {
                    user.Username = username;
                    Presistent.Presistent.DatabaseContext.SaveChanges();
                }
            }
        }

        private async Task<string> UpdateAvatarAsync(string account)
        {
            var avatarFilepath = (from r in Presistent.Presistent.DatabaseContext.Avatar
                                  where r.Account == Core.Configuration.Account
                                  select r.Path).SingleOrDefault();

            if (avatarFilepath == null)
            {
                avatarFilepath = await GetUserProfileImageAsync(account);
            }
            else
            {
                if (!await Core.API.File.IsAvatarMatchedAsync(account, Path.GetFileName(avatarFilepath)))
                {
                    avatarFilepath = await GetUserProfileImageAsync(account);
                }
            }

            return avatarFilepath;
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

                var dbAvatar = new Presistent.Database.Models.Avatar();
                dbAvatar.Account = account;
                dbAvatar.Path = avatarFilepath;
                Presistent.Presistent.DatabaseContext.Avatar.Add(dbAvatar);
                Presistent.Presistent.DatabaseContext.SaveChanges();
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
                await Users.Last().UpdateUserAvatarAsync();
            }
        }

        private async void usernameTextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            if (usernameTextBox.Text != Core.Configuration.Username)
            {
                if (usernameTextBox.Text.Length > 1 && usernameTextBox.Text.Length < 33)
                {
                    Core.Configuration.Username = usernameTextBox.Text;
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

        private async void OnNewContactsAdded(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.Action == NotifyCollectionChangedAction.Add)
            {
                ObservableCollection<string> addedAccounts = (ObservableCollection<string>)sender;
                var newUser = new User(addedAccounts.Last(), await Core.API.Authentication.GetUsernameAsync(addedAccounts.Last()));
                await newUser.UpdateUserAvatarAsync();
                Application.Current.Dispatcher.BeginInvoke(new Action(() => Users.Add(newUser)));
            }
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
                if (!Core.Configuration.Notification.HasQuene(SelectedUser.Account) && !_contacts.Contains(SelectedUser))
                {
                    _contacts.Add(SelectedUser);    // Add temp user.
                }
                CurrentMessageQuene = Core.Configuration.Notification.GetQuene(SelectedUser.Account);
            }
        }

        private void CloseChatBoxButton_Click(object sender, RoutedEventArgs e)
        {
            ContactsListBox.SelectedItem = null;
        }

        private async void SendMessageButton_Click(object sender, RoutedEventArgs e)
        {
            var message = new Presistent.Database.Models.Messages { From = Core.Configuration.Account, To = SelectedUser.Account, Content = MessageTextBox.Text };
            CurrentMessageQuene.Add(message);
            await Core.API.Chat.SendAsync(Core.Configuration.Account, Core.Configuration.Token, SelectedUser.Account, MessageTextBox.Text); // Send message to server
            Presistent.Presistent.DatabaseContext.Messages.Add(message);    // Save message to local database.
            Presistent.Presistent.DatabaseContext.SaveChanges();
            MessageTextBox.Text = "";
            ChatBoxListBox.SelectedIndex = ChatBoxListBox.Items.Count - 1;
            ChatBoxListBox.ScrollIntoView(ChatBoxListBox.SelectedItem);
        }

        private void RefreshContactsButton_Click(object sender, RoutedEventArgs e)
        {
            _ = UpdateContactsAsync();
        }

        private void MessageTextBox_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == Key.Enter && Keyboard.IsKeyDown(Key.LeftCtrl))
            {
                SendMessageButton_Click(sender, e);
            }
        }
    }

    public class User : INotifyPropertyChanged
    {
        private string _avatar;
        public string Account { get; set; }
        public string Username { get; set; }
        public string Avatar
        {
            get => _avatar;
            set => SetField(ref _avatar, value);
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public User(string account, string username)
        {
            Account = account;
            Username = username;

            var user = (from r in Presistent.Presistent.DatabaseContext.User
                        where r.Account == account
                        select r).SingleOrDefault();

            if (user == null)
            {
                user = new Presistent.Database.Models.User { Account = account, Username = username };
                Presistent.Presistent.DatabaseContext.User.Add(user);
                Presistent.Presistent.DatabaseContext.SaveChanges();
            }
            else
            {
                if (user.Username != username)
                {
                    user.Username = username;
                    Presistent.Presistent.DatabaseContext.SaveChanges();
                }
            }
        }

        public async Task UpdateUserAvatarAsync()
        {
            var avatarFilepath = (from r in Presistent.Presistent.DatabaseContext.Avatar
                                  where r.Account == Account
                                  select r.Path).SingleOrDefault();

            if (avatarFilepath == null)
            {
                avatarFilepath = await getUserProfileImageAsync();
            }
            else
            {
                if (!await Core.API.File.IsAvatarMatchedAsync(Account, Path.GetFileName(avatarFilepath)))
                {
                    avatarFilepath = await getUserProfileImageAsync();
                }
            }

            if (!String.IsNullOrEmpty(avatarFilepath))
            {
                Avatar = avatarFilepath;
            }
        }

        private async Task<string> getUserProfileImageAsync()
        {
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

                var dbAvatar = new Presistent.Database.Models.Avatar();
                dbAvatar.Account = Account;
                dbAvatar.Path = avatarFilepath;
                Presistent.Presistent.DatabaseContext.Avatar.Add(dbAvatar);
                Presistent.Presistent.DatabaseContext.SaveChanges();
                return avatarFilepath;
            }
            return null;
        }

        protected bool SetField<T>(ref T field, T value, [CallerMemberName] string propertyName = null)
        {
            if (EqualityComparer<T>.Default.Equals(field, value)) return false;
            field = value;
            OnPropertyChanged(propertyName);
            return true;
        }

        protected void OnPropertyChanged(string propertyName)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}