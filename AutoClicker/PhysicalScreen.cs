using System;
using System.Drawing;
using System.Runtime.InteropServices;

namespace AutoClicker
{
    // Screen positions in real pixels.
    // With Windows display scaling above 100% (e.g. 125%), Windows "shrinks" the coordinates it shows to apps
    // that don't declare DPI support, like this one. The recording hook always reports real pixels, so the
    // methods here switch this thread to real pixels for one call, then switch back
    internal static class PhysicalScreen
    {
        [DllImport("user32.dll")]
        private static extern IntPtr SetThreadDpiAwarenessContext(IntPtr dpiContext);

        [DllImport("user32.dll")]
        private static extern bool SetCursorPos(int x, int y);

        [DllImport("user32.dll")]
        private static extern IntPtr WindowFromPoint(Point point);

        [DllImport("user32.dll")]
        private static extern uint GetWindowThreadProcessId(IntPtr hWnd, out uint processId);

        // "Per monitor aware v2": the thread sees real pixels on every monitor
        private static readonly IntPtr PerMonitorAwareV2 = new IntPtr(-4);

        // Moves the mouse cursor to a point recorded by the hook
        public static void MoveCursor(Point point)
        {
            IntPtr old = EnterRealPixels();
            SetCursorPos(point.X, point.Y);
            LeaveRealPixels(old);
        }

        // True if the window under this point belongs to MaxClicks (the recorder ignores clicks on its own buttons)
        public static bool IsThisAppAt(Point point)
        {
            IntPtr old = EnterRealPixels();
            IntPtr window = WindowFromPoint(point);
            LeaveRealPixels(old);

            return IsThisApp(window);
        }

        private static bool IsThisApp(IntPtr window)
        {
            if (window == IntPtr.Zero)
                return false;

            GetWindowThreadProcessId(window, out uint processId);
            return processId == (uint)System.Diagnostics.Process.GetCurrentProcess().Id;
        }

        // Returns the old setting, so it can be put back. Windows older than 10 (1607) doesn't have this
        // function: then nothing is changed, which is right as long as the scaling is 100%
        private static IntPtr EnterRealPixels()
        {
            try
            {
                return SetThreadDpiAwarenessContext(PerMonitorAwareV2);
            }
            catch (EntryPointNotFoundException)
            {
                return IntPtr.Zero;
            }
        }

        private static void LeaveRealPixels(IntPtr old)
        {
            if (old != IntPtr.Zero)
                SetThreadDpiAwarenessContext(old);
        }
    }
}
