using System;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace AutoClicker
{
    // System-wide hotkey: Windows sends WM_HOTKEY to our window
    // even when another program has the focus
    public static class GlobalHotkey
    {
        // Message Windows sends to the window when the hotkey is pressed
        public const int WM_HOTKEY = 0x0312;

        [DllImport("user32.dll", SetLastError = true)]
        private static extern bool RegisterHotKey(IntPtr hWnd, int id, uint fsModifiers, uint vk);

        [DllImport("user32.dll", SetLastError = true)]
        private static extern bool UnregisterHotKey(IntPtr hWnd, int id);

        private const uint MOD_ALT = 0x0001;
        private const uint MOD_CONTROL = 0x0002;
        private const uint MOD_SHIFT = 0x0004;
        private const uint MOD_NOREPEAT = 0x4000; // Holding the key down doesn't repeat the message

        // keys = key + modifiers, e.g. Keys.Control | Keys.F6
        // Returns false if the combination is already taken by another program
        public static bool Register(IntPtr windowHandle, int id, Keys keys)
        {
            uint modifiers = MOD_NOREPEAT;
            if ((keys & Keys.Alt) == Keys.Alt) modifiers |= MOD_ALT;
            if ((keys & Keys.Control) == Keys.Control) modifiers |= MOD_CONTROL;
            if ((keys & Keys.Shift) == Keys.Shift) modifiers |= MOD_SHIFT;

            uint keyCode = (uint)(keys & Keys.KeyCode);
            return RegisterHotKey(windowHandle, id, modifiers, keyCode);
        }

        public static void Unregister(IntPtr windowHandle, int id)
        {
            UnregisterHotKey(windowHandle, id);
        }
    }
}
