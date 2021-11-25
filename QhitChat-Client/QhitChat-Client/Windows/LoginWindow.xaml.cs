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
        public LoginWindow()
        {
            // Subscribe to network connection events.
            Core.Configuration.Network.RaiseNetworkEvent += OnJsonRpcDisconnected;
            Core.Configuration.Network.RaiseNetworkEvent += OnJsonRpcConnected;

            InitializeComponent();
        }

        private async void LoginButton_Click(object sender, RoutedEventArgs e)
        {
            Core.Configuration.Account = UsernameTextBox.Text;
            Core.Configuration.Password = PasswordTextBox.Password;

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
                LoginButton.IsEnabled = false;
                var salt = await Core.API.Authentication.GetSaltAsync(Core.Configuration.Account);   // Get salt to calculate salted password.
                var token = await Core.API.Authentication.LoginAsync(Core.Configuration.Account, Core.Utils.SHA512Hash(Core.Configuration.Password + salt));

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
                DelayButton(LoginButton, 2000);
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

        private void PasswordTextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Return)
                LoginButton_Click(sender, e);
        }

        private void DisplayMessage(string message)
        {
            MainSnackbar.MessageQueue?.Enqueue(message);
        }

        private void OnJsonRpcDisconnected(object sender, Core.NetworkEventArgs e)
        {
            NotificationLabel.Content = "No Connection.";
            NotificationLabel.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FFCA5100"));
        }

        private async void OnJsonRpcConnected(object sender, Core.NetworkEventArgs e)
        {
            if (await TestConnectionAsync())
            {
                NotificationLabel.Content = "Connected.";
                NotificationLabel.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FF007ACC"));
            }
        }

        private void Window_Unloaded(object sender, RoutedEventArgs e)
        {
            // Subscribe to network connection events to prevent leaking.
            Core.Configuration.Network.RaiseNetworkEvent -= OnJsonRpcDisconnected;
            Core.Configuration.Network.RaiseNetworkEvent -= OnJsonRpcConnected;
        }
    }
}