using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace QhitChat_Client.Windows
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class LoginWindow : Window
    {
        private static string authenticationPath = "./cache/authenticaton";
        private static bool savedAuthentication = false;

        public LoginWindow()
        {
            // Subscribe to network connection events.
            Core.Configuration.Network.RaiseNetworkEvent += onJsonRpcDisconnected;
            Core.Configuration.Network.RaiseNetworkEvent += onJsonRpcConnected;

            InitializeComponent();
        }

        private async void Window_Loaded(object sender, RoutedEventArgs e)
        {
            if (Presistent.Filesystem.Filesystem.Exists(authenticationPath))
            {
                usernameTextBox.Text = await Presistent.Filesystem.Filesystem.ReadLineAsync(authenticationPath, 0);
                passwordTextBox.Password = await Presistent.Filesystem.Filesystem.ReadLineAsync(authenticationPath, 1);
                savedAuthentication = true;
                isSaveCheckBox.IsChecked = true;
            }
        }

        private void Window_Unloaded(object sender, RoutedEventArgs e)
        {
            // Subscribe to network connection events to prevent leaking.
            Core.Configuration.Network.RaiseNetworkEvent -= onJsonRpcDisconnected;
            Core.Configuration.Network.RaiseNetworkEvent -= onJsonRpcConnected;

            Presistent.Filesystem.Filesystem.DeleteFile(authenticationPath);
            if (isSaveCheckBox.IsChecked.GetValueOrDefault() == true)
            {
                // Write password to config to save password.
                Presistent.Filesystem.Filesystem.WriteLine(authenticationPath, Core.Configuration.Account);
                Presistent.Filesystem.Filesystem.WriteLine(authenticationPath, Core.Configuration.Password);
            }
        }

        private async void loginButton_Click(object sender, RoutedEventArgs e)
        {
            Core.Configuration.Account = usernameTextBox.Text;
            Core.Configuration.Password = passwordTextBox.Password;

            if (Core.Configuration.Account == "" || Core.Configuration.Password == "")
            {
                DisplayMessage("用户名或密码不得为空！");
                return;
            }

            if (!Core.Configuration.Network.Connected)
            {
                DisplayMessage("无网络连接！");
                return;
            }

            try
            {
                loginButton.IsEnabled = false;
                if (!savedAuthentication || !isSaveCheckBox.IsChecked.GetValueOrDefault())
                {
                    var salt = await Core.API.Authentication.GetSaltAsync(Core.Configuration.Account);   // Get salt to calculate salted password.
                    Core.Configuration.Password = Core.Utils.SHA512Hash(Core.Configuration.Password + salt);
                }
                
                var token = await Core.API.Authentication.LoginAsync(Core.Configuration.Account, Core.Configuration.Password);

                if (!token.StartsWith(Core.CodeDefinition.ErrorPrefix))
                {
                    // Login success.
                    Core.Configuration.Token = token;
                    new MainWindow().Show();
                    Close();
                    return;
                }
                else if (token == Core.CodeDefinition.Authentication.NoUser)
                {
                    // Login failed.
                    DisplayMessage("登陆失败！用户不存在！");
                }
                else if (token == Core.CodeDefinition.Authentication.WrongPassword)
                {
                    // Login failed.
                    DisplayMessage("登陆失败！密码错误！");
                }
            }
            finally
            {
                DelayButton(loginButton, 2000);
            }
        }

        private async void DelayButton(Control control, int ms)
        {
            control.IsEnabled = false;
            await Task.Delay(ms);
            control.IsEnabled = true;
        } // 防止用户点击太快导致出不可描述的Bug

        private async Task<bool> TestConnectionAsync()
        {
            if (await Core.API.Utils.PingAsync() == "Pong")
            {
                return true;
            }
            await Task.Delay(1000);
            return await TestConnectionAsync();
        }

        private void passwordTextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Return)
                loginButton_Click(sender, e);
        }

        private void DisplayMessage(string message)
        {
            MainSnackbar.MessageQueue?.Enqueue(message);
        }

        private void onJsonRpcDisconnected(object sender, Core.NetworkEventArgs e)
        {
            NotificationLabel.Content = "No Connection.";
            NotificationLabel.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FFCA5100"));
        }

        private async void onJsonRpcConnected(object sender, Core.NetworkEventArgs e)
        {
            if (await TestConnectionAsync())
            {
                NotificationLabel.Content = "Connected.";
                NotificationLabel.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FF007ACC"));
            }
        }

        private void usernameTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            savedAuthentication = false;
            isSaveCheckBox.IsChecked = false;
            passwordTextBox.Password = "";
        }

        private void passwordTextBox_TargetUpdated(object sender, System.Windows.Data.DataTransferEventArgs e)
        {
            savedAuthentication = false;
            isSaveCheckBox.IsChecked = false;
        }
    }
}