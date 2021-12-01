using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
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
using Ookii.Dialogs.Wpf;
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
                    searchContacts();
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
            Core.Configuration.Network.RaiseNetworkEvent += onJsonRpcDisconnected;
            Core.Configuration.Notification.Contacts.CollectionChanged += OnNewContactsAdded;

            TitleBar.Title = Core.Configuration.TITLE;
            updateUserProfileAsync();
            await updateContactsAsync();
            await fetchNewMessagesAsync();
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

        private async Task updateUserProfileAsync()
        {
            // Update username
            Core.Configuration.Username = await Core.API.Authentication.GetUsernameAsync(Core.Configuration.Account);
            updateUsername(Core.Configuration.Account, Core.Configuration.Username);

            // Update avatar
            var avatarFilepath = await updateAvatarAsync(Core.Configuration.Account);

            usernameTextBox.Text = Core.Configuration.Username;
            userAvatarImageBrush.ImageSource = new BitmapImage(new Uri(Path.GetFullPath(avatarFilepath)));
        }

        private async Task updateContactsAsync()
        {
            relationship = await Core.API.Relationship.GetRelationshipAsync(Core.Configuration.Account, Core.Configuration.Token);
            Users.Clear();
            foreach(var i in relationship)
            {
                Users.Add(new User(i.Key, i.Value));
                await Users.Last().UpdateUserAvatarAsync();
                Core.Configuration.Notification.AddQuene(i.Key);
            }
            _contacts = new ObservableCollection<User>(Users);  // Make a copy of contacts.
        }

        private async Task fetchNewMessagesAsync()
        {
            var messages = await Core.API.Chat.FetchAsync(Core.Configuration.Account, Core.Configuration.Token);
            foreach (var message in messages)
            {
                Core.Configuration.Notification.NewMessage(message);
            }
        }

        private void updateUsername(string account, string username)
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

        private async Task<string> updateAvatarAsync(string account)
        {
            var avatarFilepath = (from r in Presistent.Presistent.DatabaseContext.Avatar
                                  where r.Account == Core.Configuration.Account
                                  select r.Path).SingleOrDefault();

            if (avatarFilepath == null)
            {
                avatarFilepath = await getUserProfileImageAsync(account);
            }
            else
            {
                if (!await Core.API.File.IsAvatarMatchedAsync(account, Path.GetFileName(avatarFilepath)))
                {
                    avatarFilepath = await getUserProfileImageAsync(account);
                }
            }

            return avatarFilepath;
        }

        private async Task<string> getUserProfileImageAsync(string account)
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

        private async void searchContacts()
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
                        _ = updateUserProfileAsync();
                        DisplayMessage("已更改用户名！");
                        return;
                    }
                }

                // Unchanged
                _ = updateUserProfileAsync();
                DisplayMessage("用户名未更改，请检查后重试。用户名长度应大于1并小于33。");
            }
        }

        private async void onJsonRpcDisconnected(object sender, Core.NetworkEventArgs e)
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

        private void closeChatBoxButton_Click(object sender, RoutedEventArgs e)
        {
            ContactsListBox.SelectedItem = null;
        }

        private async void SendMessageButton_Click(object sender, RoutedEventArgs e)
        {
            if (!String.IsNullOrWhiteSpace(MessageTextBox.Text))
            {
                var message = new Presistent.Database.Models.Messages { From = Core.Configuration.Account, To = SelectedUser.Account, Content = MessageTextBox.Text, CreatedOn = DateTime.Now };
                MessageTextBox.Text = "";

                CurrentMessageQuene.Add(message);
                ChatBoxListBox.SelectedIndex = ChatBoxListBox.Items.Count - 1;
                ChatBoxListBox.ScrollIntoView(ChatBoxListBox.SelectedItem);
                await Core.API.Chat.SendAsync(Core.Configuration.Account, Core.Configuration.Token, SelectedUser.Account, message.Content); // Send message to server
                Presistent.Presistent.DatabaseContext.Messages.Add(message);    // Save message to local database.
                Presistent.Presistent.DatabaseContext.SaveChanges();
            }
        }

        private void RefreshContactsButton_Click(object sender, RoutedEventArgs e)
        {
            _ = updateContactsAsync();
        }

        private void MessageTextBox_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == Key.Enter && Keyboard.IsKeyDown(Key.LeftCtrl))
            {
                SendMessageButton_Click(sender, e);
            }
        }

        private async void Ellipse_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            VistaOpenFileDialog dlg = new VistaOpenFileDialog();
            dlg.Title = "上传新头像";

            var filter = "";
            foreach (var ext in "png,jpg,bmp".Split(","))
            {
                filter += $"{ext} (*.{ext})|*.{ext}|";
            }
            dlg.Filter = filter + "All files (*.*)|*.*";

            if ((bool)dlg.ShowDialog(this))
            {
                var uploadAvatarPath = dlg.FileName;
                
                var uploadBytes = Filesystem.GetFileChunckByChunckNumber(uploadAvatarPath, 0);
                using (System.Drawing.Image image = System.Drawing.Image.FromStream(new MemoryStream(uploadBytes)))
                {
                    if (image.Width / image.Height != 1)
                    {
                        DisplayMessage("上传的新头像应为正方形！");
                        return;
                    }
                    using (var ms = new MemoryStream())
                    {
                        image.Save(ms, ImageFormat.Png);  // Or Png
                        uploadBytes = ms.ToArray();
                    }
                }
                if (uploadBytes.Length >= Filesystem.ChunckSize)
                {
                    DisplayMessage("新头像不能超过4MB大小！");
                }
                else
                {
                    if (await Core.API.File.UploadNewAvatarAsync(Core.Configuration.Account, Core.Configuration.Token, uploadBytes))
                    {
                        DisplayMessage("已成功更改头像！");
                        await updateUserProfileAsync();
                        return;
                    }
                }
                DisplayMessage("头像未更改！");
            }
        }

        private async void SendFileButton_Click(object sender, RoutedEventArgs e)
        {
            VistaOpenFileDialog dlg = new VistaOpenFileDialog();
            dlg.Title = "选择文件";
            dlg.Filter = "All files (*.*)|*.*";
            if ((bool)dlg.ShowDialog(this))
            {
                var uploadFilePath = dlg.FileName;
                var originalFilename = Filesystem.GetFilenameFromPath(uploadFilePath);
                var uuid = await Core.API.File.CreateEmptyFileAsync(Core.Configuration.Account, Core.Configuration.Token, originalFilename, Filesystem.GetFileSize(uploadFilePath));
                var chunckCount = Filesystem.GetChunkCount(uploadFilePath);

                if (uuid != null)
                {
                    var content = $"&![File]{uuid}\n&![Path]{uploadFilePath}\n&![ChunckCount]{chunckCount}\n&![IsDone]{chunckCount}";
                    var message = new Presistent.Database.Models.Messages { From = Core.Configuration.Account, To = SelectedUser.Account, Content = content, CreatedOn=DateTime.Now };
                    CurrentMessageQuene.Add(message);
                    ChatBoxListBox.SelectedIndex = ChatBoxListBox.Items.Count - 1;
                    ChatBoxListBox.ScrollIntoView(ChatBoxListBox.SelectedItem);
                    Presistent.Presistent.DatabaseContext.Messages.Add(message);    // Save message to local database.
                    for (int chunck = 0; chunck < chunckCount; chunck++)
                    {
                        await Core.API.File.UploadFileByChunckAsync(Core.Configuration.Account, Core.Configuration.Token, uuid, chunck, Filesystem.GetFileChunckByChunckNumber(uploadFilePath, chunck));
                        message.Content = $"&![File]{uuid}\n&![Path]{uploadFilePath}\n&![ChunckCount]{chunckCount}\n&![IsDone]{chunckCount-chunck-1}";
                        Presistent.Presistent.DatabaseContext.SaveChanges();
                    }
                    await Core.API.Chat.SendAsync(Core.Configuration.Account, Core.Configuration.Token, SelectedUser.Account, $"&![File]{uuid}\n&![Path]{originalFilename}"); // Send message to server


                    foreach (var ext in Core.Configuration.ImageExtensions.Split(","))
                    {
                        if (originalFilename.Substring(originalFilename.Length - 5).Contains(ext))
                        {
                            message.Content = $"&![Image]{uploadFilePath}";
                            Presistent.Presistent.DatabaseContext.SaveChanges();
                            break;
                        }
                    }
                }
            }
        }

        private async void openFileButton_Click(object sender, RoutedEventArgs e)
        {
            // Disable button to prevent double click.
            ((Button)sender).IsEnabled = false;

            var message = (Presistent.Database.Models.Messages)((Button)sender).DataContext;
            var content = message.Content.Split('\n');
            if (content.Length == 4)
            {
                // If file exists opens it anyway.
                var filePath = content[1].Replace("&![Path]", "");
                var isDone = Convert.ToInt32(content[3].Replace("&![IsDone]", ""));
                if (Filesystem.Exists(filePath) && isDone == 0)
                {
                    new Process
                    {
                        StartInfo = new ProcessStartInfo(@Path.GetFullPath(filePath))
                        {
                            UseShellExecute = true
                        }
                    }.Start();
                    ((Button)sender).IsEnabled = true;
                    return;
                }
            }

            // Otherwise download file from server.
            var uuid = content[0].Replace("&![File]", "");
            var originalFilename = content[1].Replace("&![Path]", "");
            var fileSize = await Core.API.File.GetFileSizeAsync(Core.Configuration.Account, Core.Configuration.Token, uuid);

            var savePath = Path.Combine(Core.Configuration.FileDirectory, originalFilename);
            // Check if file exists.
            if (Filesystem.Exists(savePath))
            {
                int count = 1;
                string fileExtension = Path.GetExtension(originalFilename);
                string filename = Path.GetFileNameWithoutExtension(originalFilename);
                do
                {
                    originalFilename = $"{filename}_{count}{fileExtension}";
                    savePath = Path.Combine(Core.Configuration.FileDirectory, originalFilename);
                    count++;
                } while (Filesystem.Exists(savePath));
            }

            Filesystem.CreateEmptyFile(savePath, fileSize);
            var chunckCount = Filesystem.GetChunkCount(savePath);

            for (int chunck = 0; chunck < chunckCount; chunck++)
            {
                var data = await Core.API.File.GetFileByChunckAsync(Core.Configuration.Account, Core.Configuration.Token, uuid, chunck);
                Filesystem.SaveFileByChunckNumber(savePath, data, chunck);
                message.Content = $"&![File]{uuid}\n&![Path]{savePath}\n&![ChunckCount]{chunckCount}\n&![IsDone]{chunckCount - chunck - 1}";
                Presistent.Presistent.DatabaseContext.SaveChanges();
            }

            foreach (var ext in Core.Configuration.ImageExtensions.Split(","))
            {
                if (originalFilename.Substring(originalFilename.Length - 5).Contains(ext))
                {
                    message.Content = $"&![Image]{savePath}";
                    Presistent.Presistent.DatabaseContext.SaveChanges();
                    break;
                }
            }

            ((Button)sender).IsEnabled = true;
        }

        private void chatBoxImage_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ClickCount == 2)
            {
                var filePath = ((BitmapFrame)((System.Windows.Controls.Image)sender).Source).Decoder.ToString();

                var uri = new Uri(Uri.EscapeUriString(filePath));
                filePath = uri.LocalPath;

                if (Filesystem.Exists(filePath))
                {
                    new Process
                    {
                        StartInfo = new ProcessStartInfo(filePath)
                        {
                            UseShellExecute = true
                        }
                    }.Start();
                }
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
                var avatarFilepath = Path.Combine(Core.Configuration.AvatarDirectory, avatarFilename);
                var avatarData = avatar.First().Value;

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