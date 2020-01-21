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
        const int WM_NCLBUTTONDOWN = 0x00A1;
        const int WM_NCHITTEST = 0x0084;
        const int HTCAPTION = 2;
        [DllImport("User32.dll")]
        static extern int SendMessage(IntPtr hWnd,
        int Msg, IntPtr wParam, IntPtr lParam);

        //[DllImport("User32.dll")]
        //static extern IntPtr GetSystemMenu(IntPtr hWnd, bool bRevert);

        //[DllImport("User32.dll")]
        //static extern bool RemoveMenu(IntPtr hMenu, int uPosition, int uFlags);

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
    }
}
