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
            base.OnMouseLeftButtonDown(e);

            //让窗口动起来
            Window.GetWindow(this).DragMove();
        }
    }
}
