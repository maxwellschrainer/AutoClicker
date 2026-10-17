using System;
using System.ComponentModel;
using System.Windows.Forms;

namespace AutoClicker
{
    // A button that shows a global hotkey and lets the user change it: click it, then press the new key
    // or combination (Esc cancels). When the hotkey is pressed anywhere in Windows, it raises HotkeyPressed.
    // Used by both screens: the Auto Clicker's start/stop hotkey and the Macro's Record and Play hotkeys
    public class HotkeyButton : Button
    {
        #region Fields

        // Current hotkey (key + modifiers). Keys.None = no hotkey registered
        private Keys hotkey = Keys.None;

        // True while we wait for the user to press the new hotkey
        private bool waitingForKey = false;

        #endregion

        #region Properties and Events

        // Id we give Windows for this hotkey. Each hotkey of the app needs its own (the Auto Clicker uses 1)
        [DefaultValue(0)]
        public int HotkeyId { get; set; }

        // Hidden from the Designer: the value comes from the saved settings at run time
        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public Keys Hotkey => hotkey;

        // The hotkey was pressed (from any program)
        public event EventHandler HotkeyPressed;

        // The user picked a new hotkey and Windows accepted it (time to save it)
        public event EventHandler HotkeyChanged;

        #endregion

        #region Setting the Hotkey

        // Called once at startup with the saved hotkey. If it is taken, warns and shows "(none)"
        public void SetHotkey(Keys keys)
        {
            if (TryRegister(keys))
            {
                hotkey = keys;
            }
            else
            {
                if (keys != Keys.None)
                    ShowTakenWarning(keys);
                hotkey = Keys.None;
            }
            UpdateText();
        }

        // First click: start waiting for the new key. A second click cancels
        protected override void OnClick(EventArgs e)
        {
            base.OnClick(e);

            if (waitingForKey)
                FinishCapture(hotkey);
            else
                StartCapture();
        }

        private void StartCapture()
        {
            // Unregister the current one, otherwise Windows would catch it before this button sees it
            GlobalHotkey.Unregister(Handle, HotkeyId);
            waitingForKey = true;
            UpdateText();
        }

        // Receives the keys while this button has the focus, before the normal handling (Tab, Enter, arrows...)
        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            if (!waitingForKey)
                return base.ProcessCmdKey(ref msg, keyData);

            Keys key = keyData & Keys.KeyCode; // The key without Ctrl/Alt/Shift

            if (key == Keys.Escape)
            {
                FinishCapture(hotkey); // Cancel: keep the old hotkey
            }
            // Only a modifier so far (e.g. just Ctrl): keep waiting for the real key
            else if (key != Keys.ControlKey && key != Keys.ShiftKey && key != Keys.Menu
                     && key != Keys.LWin && key != Keys.RWin)
            {
                FinishCapture(keyData);
            }
            return true; // "Handled": the key goes nowhere else
        }

        // Clicking somewhere else, hiding the screen or disabling the button cancels the capture
        protected override void OnLostFocus(EventArgs e)
        {
            base.OnLostFocus(e);
            CancelCapture();
        }

        protected override void OnVisibleChanged(EventArgs e)
        {
            base.OnVisibleChanged(e);
            if (!Visible)
                CancelCapture();
        }

        protected override void OnEnabledChanged(EventArgs e)
        {
            base.OnEnabledChanged(e);
            if (!Enabled)
                CancelCapture();
        }

        private void CancelCapture()
        {
            if (waitingForKey)
                FinishCapture(hotkey);
        }

        // Tries the new hotkey; if it is taken, keeps the old one
        private void FinishCapture(Keys newHotkey)
        {
            waitingForKey = false;

            if (TryRegister(newHotkey))
            {
                bool changed = newHotkey != hotkey;
                hotkey = newHotkey;
                if (changed)
                    HotkeyChanged?.Invoke(this, EventArgs.Empty);
            }
            else
            {
                ShowTakenWarning(newHotkey);
                if (!TryRegister(hotkey))
                    hotkey = Keys.None;
            }
            UpdateText();
        }

        #endregion

        #region Registering and Receiving

        private bool TryRegister(Keys keys)
        {
            return keys != Keys.None && GlobalHotkey.Register(Handle, HotkeyId, keys);
        }

        // WM_HOTKEY arrives here even while the button is disabled or its screen is hidden
        protected override void WndProc(ref Message m)
        {
            if (m.Msg == GlobalHotkey.WM_HOTKEY && (int)m.WParam == HotkeyId)
                HotkeyPressed?.Invoke(this, EventArgs.Empty);

            base.WndProc(ref m);
        }

        // The window is closing: free the key for other programs
        protected override void OnHandleDestroyed(EventArgs e)
        {
            GlobalHotkey.Unregister(Handle, HotkeyId);
            base.OnHandleDestroyed(e);
        }

        private static void ShowTakenWarning(Keys keys)
        {
            MessageBox.Show("The hotkey " + HotkeyToText(keys) + " is already in use (by another program or another MaxClicks hotkey).\nChoose another one.",
                "MaxClicks", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }

        #endregion

        #region UI Updates

        // Turns Keys.Control | Keys.F6 into "Ctrl+F6"
        public static string HotkeyToText(Keys keys)
        {
            if (keys == Keys.None)
                return "(none)";

            return new KeysConverter().ConvertToString(keys);
        }

        // "Start" + F6 = "Start (F6)": the screens' big buttons show their hotkey
        public static string AddHotkey(string text, Keys keys)
        {
            return keys == Keys.None ? text : text + " (" + HotkeyToText(keys) + ")";
        }

        private void UpdateText()
        {
            Text = waitingForKey ? "Press key" : HotkeyToText(hotkey);  // Short: must fit the button
        }

        #endregion
    }
}
