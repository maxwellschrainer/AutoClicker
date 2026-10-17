using System;
using System.Runtime.InteropServices;

namespace AutoClicker
{
    // The Windows side of sending input (user32.dll), shared by Mouse and Keyboard.
    // Both build INPUT structs and hand them to Send, which plays them in order
    internal static class NativeInput
    {
        // SendInput takes an array of input events and plays them in order, with nothing from
        // other programs in between. Returns how many events it managed to send
        [DllImport("user32.dll")]
        private static extern uint SendInput(uint nInputs, INPUT[] pInputs, int cbSize);

        // The structs below must match the memory layout Windows expects, field by field.
        // In Windows, INPUT holds a union: one event is EITHER mouse OR keyboard input,
        // and both share the same bytes. InputUnion does that with [FieldOffset(0)] on each member
        [StructLayout(LayoutKind.Sequential)]
        public struct INPUT
        {
            public uint type;
            public InputUnion u;
        }

        [StructLayout(LayoutKind.Explicit)]
        public struct InputUnion
        {
            [FieldOffset(0)] public MOUSEINPUT mi;
            [FieldOffset(0)] public KEYBDINPUT ki;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct MOUSEINPUT
        {
            public int dx;
            public int dy;
            public uint mouseData;
            public uint dwFlags;
            public uint time;
            public IntPtr dwExtraInfo;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct KEYBDINPUT
        {
            public ushort wVk;
            public ushort wScan;
            public uint dwFlags;
            public uint time;
            public IntPtr dwExtraInfo;
        }

        public const uint INPUT_MOUSE = 0;
        public const uint INPUT_KEYBOARD = 1;

        // Sends all the events in one go. Returns false if Windows did not take all of them
        // (e.g. while the PC is locked or a UAC prompt is on screen)
        public static bool Send(INPUT[] inputs)
        {
            uint sent = SendInput((uint)inputs.Length, inputs, Marshal.SizeOf(typeof(INPUT)));
            return sent == inputs.Length;
        }
    }
}
