using System.Drawing;
using System.Windows.Forms;

namespace AutoClicker
{
    // What a macro step does
    public enum StepKind
    {
        KeyDown,
        KeyUp,
        MouseDown,
        MouseUp,
        Wheel
    }

    // One recorded action of a macro: "wait Delay ms, then press / release this key or mouse button
    // (or turn the wheel)". A click is two steps (down + up), so holding and dragging replay exactly
    public class MacroStep
    {
        #region Data

        // Milliseconds to wait before this step (after the previous one)
        public int Delay { get; set; }

        public StepKind Kind { get; set; }

        // --- Keyboard steps ---
        public Keys Key { get; set; }
        public int ScanCode { get; set; }
        public bool Extended { get; set; }

        // --- Mouse steps ---
        public MouseButtons Button { get; set; }
        public Point Position { get; set; }   // In real pixels (see PhysicalScreen)
        public int WheelDelta { get; set; }   // 120 per notch, + up / - down

        #endregion

        #region Pairs (press / release)

        public bool IsKeyStep => Kind == StepKind.KeyDown || Kind == StepKind.KeyUp;

        public bool IsPress => Kind == StepKind.KeyDown || Kind == StepKind.MouseDown;

        public bool IsRelease => Kind == StepKind.KeyUp || Kind == StepKind.MouseUp;

        // True when this step presses what "release" lets go of (same key, or same mouse button)
        public bool IsReleasedBy(MacroStep release)
        {
            if (Kind == StepKind.KeyDown && release.Kind == StepKind.KeyUp)
                return Key == release.Key;

            if (Kind == StepKind.MouseDown && release.Kind == StepKind.MouseUp)
                return Button == release.Button;

            return false;
        }

        // A new step that releases what this one pressed (used to let go of everything when the macro stops)
        public MacroStep CreateRelease()
        {
            return new MacroStep
            {
                Kind = Kind == StepKind.KeyDown ? StepKind.KeyUp : StepKind.MouseUp,
                Key = Key,
                ScanCode = ScanCode,
                Extended = Extended,
                Button = Button,
                Position = Position
            };
        }

        #endregion

        #region Playing

        // Sends this step to Windows. Mouse steps first move the cursor to the recorded point.
        // Returns false if Windows refused the input (e.g. the PC is locked)
        public bool Play()
        {
            switch (Kind)
            {
                case StepKind.KeyDown:
                case StepKind.KeyUp:
                    return Keyboard.Send(Key, ScanCode, Extended, Kind == StepKind.KeyDown);

                case StepKind.MouseDown:
                    PhysicalScreen.MoveCursor(Position);
                    return Mouse.Press(Button);

                case StepKind.MouseUp:
                    PhysicalScreen.MoveCursor(Position);
                    return Mouse.Release(Button);

                default: // Wheel
                    PhysicalScreen.MoveCursor(Position);
                    return Mouse.Wheel(WheelDelta);
            }
        }

        #endregion

        #region Text

        // What the step list shows, e.g. "Key down: A", "Left up (512, 300)", "Wheel down (512, 300)"
        public string Description
        {
            get
            {
                string at = " (" + Position.X + ", " + Position.Y + ")";

                switch (Kind)
                {
                    case StepKind.KeyDown: return "Key down: " + KeyName;
                    case StepKind.KeyUp: return "Key up: " + KeyName;
                    case StepKind.MouseDown: return ButtonName + " down" + at;
                    case StepKind.MouseUp: return ButtonName + " up" + at;
                    default: return (WheelDelta > 0 ? "Wheel up" : "Wheel down") + at;
                }
            }
        }

        // "A", "Space", "LShiftKey"... (the names of the Keys enum)
        private string KeyName => new KeysConverter().ConvertToString(Key);

        private string ButtonName
        {
            get
            {
                switch (Button)
                {
                    case MouseButtons.Right: return "Right";
                    case MouseButtons.Middle: return "Middle";
                    case MouseButtons.XButton1: return "Back";
                    case MouseButtons.XButton2: return "Forward";
                    default: return "Left";
                }
            }
        }

        // The step as one line of numbers, to save it in the settings:
        // Delay,Kind,Key,ScanCode,Extended,Button,X,Y,WheelDelta
        public string ToSaveText()
        {
            return string.Join(",", Delay, (int)Kind, (int)Key, ScanCode, Extended ? 1 : 0,
                (int)Button, Position.X, Position.Y, WheelDelta);
        }

        // The opposite of ToSaveText. Returns null if the text is not a valid step
        // (the settings file is plain text and can be edited by hand)
        public static MacroStep FromSaveText(string text)
        {
            string[] parts = text.Split(',');
            if (parts.Length != 9)
                return null;

            int[] n = new int[9];
            for (int i = 0; i < 9; i++)
            {
                if (!int.TryParse(parts[i], out n[i]))
                    return null;
            }

            if (n[0] < 0 || n[1] < (int)StepKind.KeyDown || n[1] > (int)StepKind.Wheel)
                return null;

            return new MacroStep
            {
                Delay = n[0],
                Kind = (StepKind)n[1],
                Key = (Keys)n[2],
                ScanCode = n[3],
                Extended = n[4] == 1,
                Button = (MouseButtons)n[5],
                Position = new Point(n[6], n[7]),
                WheelDelta = n[8]
            };
        }

        #endregion
    }
}
