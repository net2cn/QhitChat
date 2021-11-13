using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace QhitChat_Client.Windows
{
    public partial class TitleGrid : Grid
    {
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
