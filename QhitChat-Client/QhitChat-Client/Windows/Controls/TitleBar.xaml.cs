using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace QhitChat_Client.Windows.Controls
{
    public enum ControlMode
    {
        MinMax,
        Minimize
    }

    /// <summary>
    /// TitleBar.xaml 的交互逻辑
    /// </summary>
    public partial class TitleBar : UserControl
    {
        private const ControlMode defaultControlMode = ControlMode.MinMax;
        private const string defaultTitle = "Title";

        #region DependencyProperty : ControlTypeProperty
        public ControlMode ControlMode
        {
            get => (ControlMode)GetValue(ControlModeProperty);
            set => SetValue(ControlModeProperty, value);
        }

        public static readonly DependencyProperty ControlModeProperty =
            DependencyProperty.Register(
                nameof(ControlMode), typeof(ControlMode), typeof(TitleBar), new FrameworkPropertyMetadata(defaultControlMode));
        #endregion

        #region DependencyProperty : TitleProperty
        public string Title
        {
            get => (string)GetValue(TitleProperty);
            set => SetValue(TitleProperty, value);
        }

        public static readonly DependencyProperty TitleProperty =
            DependencyProperty.Register(
                nameof(Title), typeof(string), typeof(TitleBar), new FrameworkPropertyMetadata(defaultTitle));
        #endregion

        public TitleBar()
        {
            InitializeComponent();
        }

        private void minimizeButton_Click(object sender, RoutedEventArgs e)
        {
            Window window = Window.GetWindow(this);

            if (window.WindowState != WindowState.Minimized)
                window.WindowState = WindowState.Minimized;
        }

        private void maximizeButton_Click(object sender, RoutedEventArgs e)
        {
            Window window = Window.GetWindow(this);

            if (window.WindowState != WindowState.Maximized)
            {
                window.WindowState = WindowState.Maximized;
            }
            else
            {
                window.WindowState = WindowState.Normal;
            }
        }

        private void closeButton_Click(object sender, RoutedEventArgs e)
        {
            Window.GetWindow(this).Close();
        }

        protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e)
        {
            base.OnMouseLeftButtonDown(e);  // Default handling.
            var window = Window.GetWindow(this);

            if (window.ResizeMode != ResizeMode.NoResize && window.ResizeMode != ResizeMode.CanMinimize)    // No resize functionality if window is not resizable.
            {
                if (e.ClickCount == 2)  // Switch state if double click.
                {
                    if (window.WindowState == WindowState.Maximized)
                    {
                        window.WindowState = WindowState.Normal;
                    }
                    else
                    {
                        window.WindowState = WindowState.Maximized;
                    }

                    return;
                }

                if (window.WindowState == WindowState.Maximized)    // Switch window state to normal if maximized.
                {
                    // Snap window back to mouse position first.
                    var point = PointToScreen(e.MouseDevice.GetPosition(this));

                    if (point.X <= window.RestoreBounds.Width / 2)
                        window.Left = 0;
                    else if (point.X >= window.RestoreBounds.Width)
                        window.Left = point.X - (window.RestoreBounds.Width - (this.ActualWidth - point.X));
                    else
                        window.Left = point.X - (window.RestoreBounds.Width / 2);

                    window.Top = point.Y - (((FrameworkElement)this).ActualHeight / 2);

                    // Then we set window state to normal.
                    window.WindowState = WindowState.Normal;
                }
            }

            window.DragMove();  // Move window by dragging.
        }
    }
}
