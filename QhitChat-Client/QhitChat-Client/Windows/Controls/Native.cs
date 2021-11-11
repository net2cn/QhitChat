using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

namespace QhitChat_Client.Windows
{
    public static class Native
    {

        [DllImport("user32")]
        internal static extern bool GetMonitorInfo(IntPtr hMonitor, MONITORINFO lpmi);

        [DllImport("user32")]
        internal static extern IntPtr MonitorFromWindow(IntPtr handle, int flags);

        public static void WmGetMinMaxInfo(IntPtr hwnd, IntPtr lParam, int minWidth, int minHeight)
        {
            MINMAXINFO mmi = (MINMAXINFO)Marshal.PtrToStructure(lParam, typeof(MINMAXINFO));

            // Adjust the maximized size and position to fit the work area of the correct monitor
            int MONITOR_DEFAULTTONEAREST = 0x00000002;
            IntPtr monitor = Native.MonitorFromWindow(hwnd, MONITOR_DEFAULTTONEAREST);

            if (monitor != IntPtr.Zero)
            {

                Native.MONITORINFO monitorInfo = new Native.MONITORINFO();
                Native.GetMonitorInfo(monitor, monitorInfo);
                Native.RECT rcWorkArea = monitorInfo.rcWork;
                Native.RECT rcMonitorArea = monitorInfo.rcMonitor;
                mmi.ptMaxPosition.x = Math.Abs(rcWorkArea.left - rcMonitorArea.left)-10;    // Add 20 offset to fill the whole area. Otherwise there will be some extra space. (Maybe use multiply?)
                mmi.ptMaxPosition.y = Math.Abs(rcWorkArea.top - rcMonitorArea.top)-10;
                mmi.ptMaxSize.x = Math.Abs(rcWorkArea.right - rcWorkArea.left)+20;
                mmi.ptMaxSize.y = Math.Abs(rcWorkArea.bottom - rcWorkArea.top)+20;
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