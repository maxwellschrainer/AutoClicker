using System.Windows.Forms;
using static AutoClicker.NativeInput;

namespace AutoClicker
{
    // Sends real key presses through Windows (SendInput, see NativeInput), to whatever window has the focus
    public static class Keyboard
    {
        private const uint KEYEVENTF_EXTENDEDKEY = 0x0001;
        private const uint KEYEVENTF_KEYUP = 0x0002;

        // Presses or releases one key.
        // scanCode: the number of the physical key on the keyboard (0 if unknown). Sent together with the
        //           key code because some programs, mostly games, read only this one.
        // extended: true for keys that exist twice and need it to be told apart (right Ctrl / Alt,
        //           arrows, Insert, Delete, Home, End, Page Up / Down, the numpad's Enter and /).
        //           The recording hook tells us which ones are extended
        public static bool Send(Keys key, int scanCode, bool extended, bool press)
        {
            INPUT input = new INPUT { type = INPUT_KEYBOARD };
            input.u.ki.wVk = (ushort)key;
            input.u.ki.wScan = (ushort)scanCode;
            input.u.ki.dwFlags = (extended ? KEYEVENTF_EXTENDEDKEY : 0) | (press ? 0 : KEYEVENTF_KEYUP);

            return NativeInput.Send(new[] { input });
        }
    }
}
