using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

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
            TitleLabel.Content = Core.Configuration.TITLE;
            if (await API.Utils.PingAsync()=="Pong")
            {
                NotificationLabel.Content = "Connected.";
            }
            else
            {
                MessageBox.Show("无法连接服务器！软件即将退出", "严重错误", MessageBoxButton.OK, MessageBoxImage.Error);
                Environment.Exit(0);
            }
        }

        private void LoginButton_Click(object sender, RoutedEventArgs e)
        {
            Core.Configuration.username = UsernameTextBox.Text;
            Core.Configuration.password = PasswordTextBox.Password;

            if (Core.Configuration.username == "" || Core.Configuration.password == "")
            {
                MessageBox.Show("用户名或密码不得为空！", "一般错误", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            DelayButton(LoginButton, 2000);
        }

        private async void DelayButton(Control control, int ms)
        {
            control.IsEnabled = false;
            await Task.Delay(ms);
            control.IsEnabled = true;
        } //防止用户点击太快导致出不可描述的Bug

        private void PasswordTextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Return)
                LoginButton_Click(sender, e);
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void MinimizeButton_Click(object sender, RoutedEventArgs e)
        {
            Window window = GetWindow(this);

            if (window.WindowState != WindowState.Minimized)
                window.WindowState = WindowState.Minimized;
        }

    }
}