using System;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace AutoClicker
{
    // Sends real mouse clicks through Windows (user32.dll), at wherever the cursor is right now
    public static class Mouse
    {
        // SendInput takes an array of input events and plays them in order, with nothing from
        // other programs in between. Returns how many events it managed to send
        [DllImport("user32.dll")]
        private static extern uint SendInput(uint nInputs, INPUT[] pInputs, int cbSize);

        // The structs below must match the memory layout Windows expects, field by field.
        // In Windows, INPUT holds a union (mouse, keyboard or hardware input): MOUSEINPUT is the
        // biggest of the three, so declaring only it still gives INPUT the right size.
        // Keyboard input (for the Macro) will need the full union with [FieldOffset]
        [StructLayout(LayoutKind.Sequential)]
        private struct INPUT
        {
            public uint type;
            public MOUSEINPUT mi;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct MOUSEINPUT
        {
            public int dx;
            public int dy;
            public uint mouseData;
            public uint dwFlags;
            public uint time;
            public IntPtr dwExtraInfo;
        }

        private const uint INPUT_MOUSE = 0;

        private const uint LEFTDOWN = 0x0002;
        private const uint LEFTUP = 0x0004;
        private const uint RIGHTDOWN = 0x0008;
        private const uint RIGHTUP = 0x0010;

        // One click (press + release) at the current cursor position.
        // Only Left and Right are supported; any other button clicks Left.
        // Returns false if Windows did not take all the events
        public static bool Click(MouseButtons button)
        {
            return SendClicks(button, 1);
        }

        // Two clicks right after each other: Windows sees a double click when both
        // happen in the same spot within the double-click time (500 ms by default)
        public static bool DoubleClick(MouseButtons button)
        {
            return SendClicks(button, 2);
        }

        // Builds "press, release" once per click and sends them all in one SendInput call
        private static bool SendClicks(MouseButtons button, int clicks)
        {
            uint down = button == MouseButtons.Right ? RIGHTDOWN : LEFTDOWN;
            uint up = button == MouseButtons.Right ? RIGHTUP : LEFTUP;

            INPUT[] inputs = new INPUT[clicks * 2];
            for (int i = 0; i < inputs.Length; i++)
            {
                inputs[i].type = INPUT_MOUSE;
                inputs[i].mi.dwFlags = i % 2 == 0 ? down : up;   // even = press, odd = release
            }

            uint sent = SendInput((uint)inputs.Length, inputs, Marshal.SizeOf(typeof(INPUT)));
            return sent == inputs.Length;
        }
    }
}
