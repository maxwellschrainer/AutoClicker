using System;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace AutoClicker
{
    // Sends real mouse clicks through Windows (user32.dll), at wherever the cursor is right now
    public static class Mouse
    {
        [DllImport("user32.dll")]
        private static extern void mouse_event(uint dwFlags, uint dx, uint dy, uint dwData, UIntPtr dwExtraInfo);

        private const uint LEFTDOWN = 0x0002;
        private const uint LEFTUP = 0x0004;
        private const uint RIGHTDOWN = 0x0008;
        private const uint RIGHTUP = 0x0010;

        // One click (press + release) at the current cursor position.
        // Only Left and Right are supported; any other button clicks Left
        public static void Click(MouseButtons button)
        {
            uint down = button == MouseButtons.Right ? RIGHTDOWN : LEFTDOWN;
            uint up = button == MouseButtons.Right ? RIGHTUP : LEFTUP;

            mouse_event(down, 0, 0, 0, UIntPtr.Zero);
            mouse_event(up, 0, 0, 0, UIntPtr.Zero);
        }

        // Two clicks right after each other: Windows sees a double click when both
        // happen in the same spot within the double-click time (500 ms by default)
        public static void DoubleClick(MouseButtons button)
        {
            Click(button);
            Click(button);
        }
    }
}