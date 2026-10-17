using System;
using System.Collections.Generic;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace AutoClicker
{
    // Watches every key and mouse button in Windows (not only in this app) while it is running.
    // It uses "low-level hooks": Windows calls our method for each input event, before any program sees it.
    // Each event becomes a MacroStep and is handed out through the Captured event; Delay is left to whoever listens
    public class InputRecorder
    {
        #region Windows (user32.dll)

        // The method Windows calls. nCode < 0 means "not for you, just pass it on"
        private delegate IntPtr HookProc(int nCode, IntPtr wParam, IntPtr lParam);

        [DllImport("user32.dll")]
        private static extern IntPtr SetWindowsHookEx(int idHook, HookProc lpfn, IntPtr hMod, uint dwThreadId);

        [DllImport("user32.dll")]
        private static extern bool UnhookWindowsHookEx(IntPtr hhk);

        [DllImport("user32.dll")]
        private static extern IntPtr CallNextHookEx(IntPtr hhk, int nCode, IntPtr wParam, IntPtr lParam);

        [DllImport("kernel32.dll")]
        private static extern IntPtr GetModuleHandle(string lpModuleName);

        private const int WH_KEYBOARD_LL = 13;
        private const int WH_MOUSE_LL = 14;

        // wParam: what happened
        private const int WM_KEYDOWN = 0x0100;
        private const int WM_KEYUP = 0x0101;
        private const int WM_SYSKEYDOWN = 0x0104;   // "SYS" = with Alt held (or F10)
        private const int WM_SYSKEYUP = 0x0105;
        private const int WM_LBUTTONDOWN = 0x0201;
        private const int WM_LBUTTONUP = 0x0202;
        private const int WM_RBUTTONDOWN = 0x0204;
        private const int WM_RBUTTONUP = 0x0205;
        private const int WM_MBUTTONDOWN = 0x0207;
        private const int WM_MBUTTONUP = 0x0208;
        private const int WM_MOUSEWHEEL = 0x020A;
        private const int WM_XBUTTONDOWN = 0x020B;
        private const int WM_XBUTTONUP = 0x020C;

        // lParam points to one of these, with the details of the event
        [StructLayout(LayoutKind.Sequential)]
        private struct KBDLLHOOKSTRUCT
        {
            public uint vkCode;
            public uint scanCode;
            public uint flags;
            public uint time;
            public IntPtr dwExtraInfo;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct MSLLHOOKSTRUCT
        {
            public Point pt;
            public uint mouseData;
            public uint flags;
            public uint time;
            public IntPtr dwExtraInfo;
        }

        private const uint LLKHF_EXTENDED = 0x01;
        private const uint LLKHF_INJECTED = 0x10;   // Sent by a program (SendInput), not by a real keyboard
        private const uint LLMHF_INJECTED = 0x01;   // Same for the mouse

        #endregion

        #region Fields

        // Raised for every recorded event: the step and the moment it happened
        // (Windows' clock, in milliseconds, so the listener can work out the waits)
        public event Action<MacroStep, uint> Captured;

        // The hooks Windows gave us; IntPtr.Zero = not installed
        private IntPtr keyboardHook = IntPtr.Zero;
        private IntPtr mouseHook = IntPtr.Zero;

        // The delegates must be kept in fields: Windows holds only a raw pointer to them, and if the
        // garbage collector threw them away, the next key press would crash the app
        private readonly HookProc keyboardProc;
        private readonly HookProc mouseProc;

        // Keys held down right now. Holding a key makes Windows repeat "key down" many times a second;
        // only the first one is recorded
        private readonly HashSet<Keys> keysDown = new HashSet<Keys>();

        #endregion

        public InputRecorder()
        {
            keyboardProc = KeyboardHookProc;
            mouseProc = MouseHookProc;
        }

        public bool Running => keyboardHook != IntPtr.Zero;

        // Installs both hooks. Returns false if Windows refused
        public bool Start()
        {
            if (Running)
                return true;

            IntPtr module = GetModuleHandle(null);
            keyboardHook = SetWindowsHookEx(WH_KEYBOARD_LL, keyboardProc, module, 0);
            mouseHook = SetWindowsHookEx(WH_MOUSE_LL, mouseProc, module, 0);

            if (keyboardHook == IntPtr.Zero || mouseHook == IntPtr.Zero)
            {
                Stop();
                return false;
            }
            return true;
        }

        public void Stop()
        {
            if (keyboardHook != IntPtr.Zero)
                UnhookWindowsHookEx(keyboardHook);
            if (mouseHook != IntPtr.Zero)
                UnhookWindowsHookEx(mouseHook);

            keyboardHook = IntPtr.Zero;
            mouseHook = IntPtr.Zero;
            keysDown.Clear();
        }

        #region Hook Methods

        // Windows waits for these methods before the input goes on, so they must be quick.
        // They always call CallNextHookEx, so the input still reaches every program normally
        private IntPtr KeyboardHookProc(int nCode, IntPtr wParam, IntPtr lParam)
        {
            if (nCode >= 0)
            {
                KBDLLHOOKSTRUCT info = Marshal.PtrToStructure<KBDLLHOOKSTRUCT>(lParam);
                int message = (int)wParam;
                bool down = message == WM_KEYDOWN || message == WM_SYSKEYDOWN;
                bool up = message == WM_KEYUP || message == WM_SYSKEYUP;
                Keys key = (Keys)info.vkCode;

                if ((info.flags & LLKHF_INJECTED) == 0 && (down || up))
                {
                    // HashSet.Add returns false if the key was already held: that is a repeat
                    bool firstPress = down && keysDown.Add(key);
                    bool release = up && keysDown.Remove(key);

                    if (firstPress || release)
                    {
                        Captured?.Invoke(new MacroStep
                        {
                            Kind = down ? StepKind.KeyDown : StepKind.KeyUp,
                            Key = key,
                            ScanCode = (int)info.scanCode,
                            Extended = (info.flags & LLKHF_EXTENDED) != 0
                        }, info.time);
                    }
                }
            }
            return CallNextHookEx(IntPtr.Zero, nCode, wParam, lParam);
        }

        private IntPtr MouseHookProc(int nCode, IntPtr wParam, IntPtr lParam)
        {
            // Mouse moves are not recorded: each mouse step saves where it happened instead
            if (nCode >= 0)
            {
                MSLLHOOKSTRUCT info = Marshal.PtrToStructure<MSLLHOOKSTRUCT>(lParam);

                if ((info.flags & LLMHF_INJECTED) == 0)
                {
                    MacroStep step = MouseStep((int)wParam, info.mouseData);
                    if (step != null)
                    {
                        step.Position = info.pt;
                        Captured?.Invoke(step, info.time);
                    }
                }
            }
            return CallNextHookEx(IntPtr.Zero, nCode, wParam, lParam);
        }

        // Turns a mouse message into a step, or null for messages we don't record (moves)
        private static MacroStep MouseStep(int message, uint mouseData)
        {
            // For the side buttons and the wheel, the useful number is in the high 16 bits of mouseData
            int high = (short)(mouseData >> 16);

            switch (message)
            {
                case WM_LBUTTONDOWN: return new MacroStep { Kind = StepKind.MouseDown, Button = MouseButtons.Left };
                case WM_LBUTTONUP: return new MacroStep { Kind = StepKind.MouseUp, Button = MouseButtons.Left };
                case WM_RBUTTONDOWN: return new MacroStep { Kind = StepKind.MouseDown, Button = MouseButtons.Right };
                case WM_RBUTTONUP: return new MacroStep { Kind = StepKind.MouseUp, Button = MouseButtons.Right };
                case WM_MBUTTONDOWN: return new MacroStep { Kind = StepKind.MouseDown, Button = MouseButtons.Middle };
                case WM_MBUTTONUP: return new MacroStep { Kind = StepKind.MouseUp, Button = MouseButtons.Middle };
                case WM_XBUTTONDOWN: return new MacroStep { Kind = StepKind.MouseDown, Button = XButton(high) };
                case WM_XBUTTONUP: return new MacroStep { Kind = StepKind.MouseUp, Button = XButton(high) };
                case WM_MOUSEWHEEL: return new MacroStep { Kind = StepKind.Wheel, WheelDelta = high };
                default: return null;
            }
        }

        private static MouseButtons XButton(int which)
        {
            return which == 1 ? MouseButtons.XButton1 : MouseButtons.XButton2;
        }

        #endregion
    }
}
