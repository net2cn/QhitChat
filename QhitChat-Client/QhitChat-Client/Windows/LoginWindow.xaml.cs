using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace QhitChat_Client.Windows
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class LoginWindow : Window
    {
        public LoginWindow()
        {
            _ = Core.Configuration.Instance;    // Initialize global Configuration instance.
            InitializeComponent();
        }

        private async void Window_Loaded(object sender, RoutedEventArgs e)
        {
            TitleBar.Title = Core.Configuration.TITLE;
            if (await TestConnectionAsync())
            {
                NotificationLabel.Content = "Connected.";
            }
        }

        private async void LoginButton_Click(object sender, RoutedEventArgs e)
        {
            Core.Configuration.Account = UsernameTextBox.Text;
            Core.Configuration.Password = PasswordTextBox.Password;

            if (Core.Configuration.Account == "" || Core.Configuration.Password == "")
            {
                MessageBox.Show("用户名或密码不得为空！", "一般错误", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                LoginButton.IsEnabled = false;
                var salt = await Core.API.Authentication.GetSaltAsync(Core.Configuration.Account);   // Get salt to calculate salted password.
                if (await Core.API.Authentication.LoginAsync(Core.Configuration.Account, Core.Utils.SHA512Hash(Core.Configuration.Password + salt)))
                {
                    // Login success.
                    new MainWindow().Show();
                    Close();
                    return;
                }
                else
                {
                    // Login failed.
                    DisplayMessage("登陆失败！");
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
    }
}