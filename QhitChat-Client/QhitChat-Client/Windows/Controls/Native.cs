using System;
using System.Runtime.InteropServices;

namespace QhitChat_Client.Windows
{
    /// <summary>
    /// This class contains Native codes to hook WinAPI in order to make borderless window behave normal as bordered window.
    /// </summary>
    public static class Native
    {

        [DllImport("user32")]
        internal static extern bool GetMonitorInfo(IntPtr hMonitor, MONITORINFO lpmi);

        [DllImport("user32")]
        internal static extern IntPtr MonitorFromWindow(IntPtr handle, int flags);

        /// <summary>
        /// Hook WinAPI WmGetMinMaxInfo to properly handle window maximize.
        /// If not hooked, during maximization window will ignore taskbar and use all avaliable space.
        /// </summary>
        /// <param name="hwnd"></param>
        /// <param name="lParam"></param>
        /// <param name="minWidth"></param>
        /// <param name="minHeight"></param>
        public static void WmGetMinMaxInfo(IntPtr hwnd, IntPtr lParam, int minWidth, int minHeight)
        {
            MINMAXINFO mmi = (MINMAXINFO)Marshal.PtrToStructure(lParam, typeof(MINMAXINFO));

            // Adjust the maximized size and position to fit the work area of the correct monitor.
            int MONITOR_DEFAULTTONEAREST = 0x00000002;
            IntPtr monitor = MonitorFromWindow(hwnd, MONITOR_DEFAULTTONEAREST);

            if (monitor != IntPtr.Zero)
            {
                MONITORINFO monitorInfo = new MONITORINFO();
                GetMonitorInfo(monitor, monitorInfo);
                RECT rcWorkArea = monitorInfo.rcWork;
                RECT rcMonitorArea = monitorInfo.rcMonitor;
                mmi.ptMaxPosition.x = Math.Abs(rcWorkArea.left - rcMonitorArea.left) - 10;    // Add 20 offset to fill the whole area. Otherwise there will be some extra space. (Maybe use multiply?)
                mmi.ptMaxPosition.y = Math.Abs(rcWorkArea.top - rcMonitorArea.top) - 10;
                mmi.ptMaxSize.x = Math.Abs(rcWorkArea.right - rcWorkArea.left) + 20;
                mmi.ptMaxSize.y = Math.Abs(rcWorkArea.bottom - rcWorkArea.top) + 20;
                mmi.ptMinTrackSize.x = minWidth;
                mmi.ptMinTrackSize.y = minHeight;
            }

            Marshal.StructureToPtr(mmi, lParam, true);
        }


        /// <summary>
        /// POINT aka POINTAPI
        /// </summary>
        [StructLayout(LayoutKind.Sequential)]
        public struct POINT
        {
            /// <summary>
            /// x coordinate of point.
            /// </summary>
            public int x;
            /// <summary>
            /// y coordinate of point.
            /// </summary>
            public int y;

            /// <summary>
            /// Construct a point of coordinates (x,y).
            /// </summary>
            public POINT(int x, int y)
            {
                this.x = x;
                this.y = y;
            }
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct MINMAXINFO
        {
            public POINT ptReserved;
            public POINT ptMaxSize;
            public POINT ptMaxPosition;
            public POINT ptMinTrackSize;
            public POINT ptMaxTrackSize;
        };

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
        public class MONITORINFO
        {
            public int cbSize = Marshal.SizeOf(typeof(MONITORINFO));
            public RECT rcMonitor = new RECT();
            public RECT rcWork = new RECT();
            public int dwFlags = 0;
        }


        [StructLayout(LayoutKind.Sequential, Pack = 0)]
        public struct RECT
        {
            public int left;
            public int top;
            public int right;
            public int bottom;

            public static readonly RECT Empty = new RECT();

            public int Width
            {
                get { return Math.Abs(right - left); }  // Abs needed for BIDI OS
            }
            public int Height
            {
                get { return bottom - top; }
            }

            public RECT(int left, int top, int right, int bottom)
            {
                this.left = left;
                this.top = top;
                this.right = right;
                this.bottom = bottom;
            }

        }
    }
}