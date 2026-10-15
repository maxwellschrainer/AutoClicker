using System;
using System.Runtime.InteropServices;

namespace AutoClicker
{
    public static class Mouse
    {
        [DllImport("user32.dll")]
        private static extern void mouse_event(uint dwFlags, uint dx, uint dy, uint dwData, UIntPtr dwExtraInfo);

        private const uint LEFTDOWN = 0x0002;
        private const uint LEFTUP = 0x0004;

        public static void LeftClick()
        {
            mouse_event(LEFTDOWN, 0, 0, 0, UIntPtr.Zero);
            mouse_event(LEFTUP, 0, 0, 0, UIntPtr.Zero);
        }
    }
}