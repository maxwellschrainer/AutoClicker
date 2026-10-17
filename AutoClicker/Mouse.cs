using System.Windows.Forms;
using static AutoClicker.NativeInput;

namespace AutoClicker
{
    // Sends real mouse input through Windows (SendInput, see NativeInput), at wherever the cursor is right now
    public static class Mouse
    {
        private const uint LEFTDOWN = 0x0002;
        private const uint LEFTUP = 0x0004;
        private const uint RIGHTDOWN = 0x0008;
        private const uint RIGHTUP = 0x0010;
        private const uint MIDDLEDOWN = 0x0020;
        private const uint MIDDLEUP = 0x0040;
        private const uint XDOWN = 0x0080;     // The side buttons (back / forward):
        private const uint XUP = 0x0100;       // mouseData says which one, 1 or 2
        private const uint WHEEL = 0x0800;     // mouseData = how far: 120 per notch, + up / - down

        // One click (press + release) at the current cursor position.
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

        // Only press, or only release, a button. The Macro uses these to replay exactly
        // what was recorded (e.g. hold the button, wait, release it somewhere else = a drag)
        public static bool Press(MouseButtons button)
        {
            return Send(new[] { ButtonInput(button, true) });
        }

        public static bool Release(MouseButtons button)
        {
            return Send(new[] { ButtonInput(button, false) });
        }

        // Turns the wheel. delta: 120 = one notch up, -120 = one notch down
        public static bool Wheel(int delta)
        {
            INPUT input = new INPUT { type = INPUT_MOUSE };
            input.u.mi.dwFlags = WHEEL;
            input.u.mi.mouseData = (uint)delta; // Negative numbers keep their bits: Windows reads them back as negative
            return Send(new[] { input });
        }

        // Builds "press, release" once per click and sends them all in one SendInput call
        private static bool SendClicks(MouseButtons button, int clicks)
        {
            INPUT[] inputs = new INPUT[clicks * 2];
            for (int i = 0; i < inputs.Length; i++)
                inputs[i] = ButtonInput(button, i % 2 == 0);   // even = press, odd = release

            return Send(inputs);
        }

        // One press or release of a button. Left, Right, Middle, XButton1 and XButton2 are supported;
        // anything else is treated as Left
        private static INPUT ButtonInput(MouseButtons button, bool press)
        {
            INPUT input = new INPUT { type = INPUT_MOUSE };

            switch (button)
            {
                case MouseButtons.Right:
                    input.u.mi.dwFlags = press ? RIGHTDOWN : RIGHTUP;
                    break;
                case MouseButtons.Middle:
                    input.u.mi.dwFlags = press ? MIDDLEDOWN : MIDDLEUP;
                    break;
                case MouseButtons.XButton1:
                case MouseButtons.XButton2:
                    input.u.mi.dwFlags = press ? XDOWN : XUP;
                    input.u.mi.mouseData = button == MouseButtons.XButton1 ? 1u : 2u;
                    break;
                default:
                    input.u.mi.dwFlags = press ? LEFTDOWN : LEFTUP;
                    break;
            }
            return input;
        }
    }
}
