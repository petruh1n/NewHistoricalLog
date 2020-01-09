using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace NewHistoricalLog
{
    class WINAPI
    {
        const int SC_CLOSE = 0xF010;
        const int MF_BYCOMMAND = 0;
        const int WM_NCLBUTTONDOWN = 0x00A1;
        const int WM_NCHITTEST = 0x0084;
        const int HTCAPTION = 2;
        public const int GWL_STYLE = -16; //WPF's Message code for Title Bar's Style 
        public const int WS_SYSMENU = 0x80000; //WPF's Message code for System Menu
        [DllImport("user32.dll", SetLastError = true)]
        public static extern int GetWindowLong(IntPtr hWnd, int nIndex);
        [DllImport("user32.dll")]
        public static extern int SetWindowLong(IntPtr hWnd, int nIndex, int dwNewLong);
        [DllImport("User32.dll")]
        static extern int SendMessage(IntPtr hWnd,
        int Msg, IntPtr wParam, IntPtr lParam);

        [DllImport("User32.dll")]
        static extern IntPtr GetSystemMenu(IntPtr hWnd, bool bRevert);

        [DllImport("User32.dll")]
        static extern bool RemoveMenu(IntPtr hMenu, int uPosition, int uFlags);

        internal static IntPtr WndProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            if (msg == WM_NCLBUTTONDOWN)
            {
                int result = SendMessage(hwnd, WM_NCHITTEST,
                IntPtr.Zero, lParam);
                if (result == HTCAPTION)
                    handled = true;
            }
            return IntPtr.Zero;
        }
        [DllImport("user32.dll")]
        public static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int X, int Y, int cx, int cy, uint uFlags);

        public static readonly IntPtr HWND_BOTTOM = new IntPtr(1);
        public const UInt32 SWP_NOSIZE = 0x0001;
        public const UInt32 SWP_NOMOVE = 0x0002;
        public const UInt32 SWP_NOACTIVATE = 0x0010;
    }
}
