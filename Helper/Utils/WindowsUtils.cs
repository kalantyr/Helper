using System;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;

namespace Helper.Utils
{
    internal static class WindowsUtils
    {
        [DllImport("user32.dll")]
        static extern int FlashWindowEx(ref FLASHWINFO pwfi);

        // stop flashing
        private const int FLASHW_STOP = 0;

        // flash the window title 
        private const int FLASHW_CAPTION = 1;

        // flash the taskbar button
        private const int FLASHW_TRAY = 2;

        // 1 | 2
        private const int FLASHW_ALL = 3;

        // flash continuously 
        private const int FLASHW_TIMER = 4;

        // flash until the window comes to the foreground 
        private const int FLASHW_TIMERNOFG = 12; 

        public static void Flash(Window window)
        {
            var fw = new FLASHWINFO
            {
                cbSize = Convert.ToUInt32(Marshal.SizeOf(typeof(FLASHWINFO))),
                hwnd = new WindowInteropHelper(window).Handle,
                dwFlags = FLASHW_TRAY,
                uCount = uint.MaxValue
            };

            FlashWindowEx(ref fw);
        }

        public static void StopFlash(Window window)
        {
            var fw = new FLASHWINFO
            {
                cbSize = Convert.ToUInt32(Marshal.SizeOf(typeof(FLASHWINFO))),
                hwnd = new WindowInteropHelper(window).Handle,
                dwFlags = FLASHW_STOP,
                uCount = uint.MaxValue
            };

            FlashWindowEx(ref fw);
        }
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct FLASHWINFO
    {
        public uint cbSize;
        public IntPtr hwnd;
        public int dwFlags;
        public uint uCount;
        public int dwTimeout;
    }
}
