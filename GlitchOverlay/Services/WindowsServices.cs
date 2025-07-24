using System;
using System.Runtime.InteropServices;

namespace GlitchOverlay.Services
{
    /// <summary>
    /// Windows API services for low-level window manipulation
    /// </summary>
    public static class WindowsServices
    {
        #region Windows API Constants

        private const int GWL_EXSTYLE = -20;
        private const int WS_EX_LAYERED = 0x80000;
        private const int WS_EX_TRANSPARENT = 0x20;
        private const int WS_EX_TOOLWINDOW = 0x80;
        private const int WS_EX_NOACTIVATE = 0x08000000;

        private static readonly IntPtr HWND_TOPMOST = new IntPtr(-1);
        private const uint SWP_NOSIZE = 0x0001;
        private const uint SWP_NOMOVE = 0x0002;
        private const uint SWP_NOACTIVATE = 0x0010;
        private const uint SWP_SHOWWINDOW = 0x0040;

        #endregion

        #region Windows API Imports
        
        [DllImport("user32.dll")]
        private static extern int GetWindowLong(IntPtr hwnd, int index);

        [DllImport("user32.dll")]
        private static extern int SetWindowLong(IntPtr hwnd, int index, int newStyle);

        [DllImport("user32.dll")]
        private static extern bool SetLayeredWindowAttributes(IntPtr hwnd, uint crKey, byte bAlpha, uint dwFlags);

        [DllImport("user32.dll")]
        private static extern IntPtr GetDesktopWindow();

        [DllImport("user32.dll")]
        private static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int X, int Y, int cx, int cy, uint uFlags);

        [DllImport("kernel32.dll", EntryPoint = "RtlZeroMemory")]
        private static extern void RtlZeroMemory(IntPtr dest, uint size);

        #endregion

        /// <summary>
        /// Makes a window click-through (transparent to mouse input)
        /// </summary>
        public static void SetWindowExTransparent(IntPtr hwnd)
        {
            try
            {
                var extendedStyle = GetWindowLong(hwnd, GWL_EXSTYLE);
                SetWindowLong(hwnd, GWL_EXSTYLE, extendedStyle | WS_EX_TRANSPARENT | WS_EX_LAYERED | WS_EX_TOOLWINDOW | WS_EX_NOACTIVATE);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error setting window transparent: {ex.Message}");
            }
        }

        /// <summary>
        /// Sets a window to always stay on top
        /// </summary>
        public static void SetWindowTopmost(IntPtr hwnd)
        {
            try
            {
                SetWindowPos(hwnd, HWND_TOPMOST, 0, 0, 0, 0, SWP_NOMOVE | SWP_NOSIZE | SWP_NOACTIVATE);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error setting window topmost: {ex.Message}");
            }
        }

        /// <summary>
        /// Sets window opacity
        /// </summary>
        public static void SetWindowOpacity(IntPtr hwnd, byte opacity)
        {
            try
            {
                SetLayeredWindowAttributes(hwnd, 0, opacity, 0x2);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error setting window opacity: {ex.Message}");
            }
        }

        /// <summary>
        /// Gets the desktop window handle
        /// </summary>
        public static IntPtr GetDesktopWindowHandle()
        {
            return GetDesktopWindow();
        }

        /// <summary>
        /// Zeros out a memory region (used for clearing bitmap buffers)
        /// </summary>
        public static void ZeroMemory(IntPtr dest, int size)
        {
            try
            {
                RtlZeroMemory(dest, (uint)size);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error zeroing memory: {ex.Message}");
            }
        }
    }
}
