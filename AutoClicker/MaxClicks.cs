using System;
using System.Windows.Forms;

namespace AutoClicker
{
    public partial class MaxClicks : Form
    {
        #region Fields

        // Number of clicks since the last Start
        private int count = 0;

        // Id we give Windows for our hotkey (any number, it only has to be unique in this app)
        private const int HotkeyId = 1;

        // Current hotkey: key + modifiers (e.g. Keys.Control | Keys.F6).
        // Keys.None means no hotkey is registered
        private Keys hotkey = Keys.F6;

        // True while we wait for the user to press the new hotkey
        private bool waitingForHotkey = false;

        #endregion

        #region Constructor
        public MaxClicks()
        {
            InitializeComponent();
        }
        #endregion

        #region Form Overrides

        // Runs once when the window is shown: register the default hotkey
        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            if (!TryRegisterHotkey(hotkey))
            {
                ShowHotkeyTakenWarning(hotkey);
                hotkey = Keys.None; // So the screen doesn't show a hotkey that doesn't work
            }
            UpdateControls();
        }

        // Release the hotkey when the window closes, so Windows frees it for other programs
        protected override void OnFormClosed(FormClosedEventArgs e)
        {
            GlobalHotkey.Unregister(Handle, HotkeyId);
            base.OnFormClosed(e);
        }

        // Every message Windows sends to the window goes through here.
        // We only care about WM_HOTKEY: our hotkey was pressed (even with another app in focus)
        protected override void WndProc(ref Message m)
        {
            if (m.Msg == GlobalHotkey.WM_HOTKEY && (int)m.WParam == HotkeyId)
            {
                ToggleClicking();
            }
            base.WndProc(ref m);
        }

        // Receives every key pressed in the window before the controls do.
        // While waiting for a new hotkey, we take the key here instead of letting it reach the buttons
        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            if (!waitingForHotkey)
                return base.ProcessCmdKey(ref msg, keyData);

            Keys key = keyData & Keys.KeyCode; // The key without Ctrl/Alt/Shift

            if (key == Keys.Escape)
            {
                FinishHotkeyCapture(hotkey); // Cancel: keep the old hotkey
            }
            // If only a modifier was pressed so far (e.g. just Ctrl), keep waiting for the real key
            else if (key != Keys.ControlKey && key != Keys.ShiftKey && key != Keys.Menu
                     && key != Keys.LWin && key != Keys.RWin)
            {
                FinishHotkeyCapture(keyData);
            }
            return true; // true = "already handled", the key goes nowhere else
        }

        #endregion

        #region Event Handlers

        // Runs when the "Start" button is clicked
        private void btnStart_Click(object sender, EventArgs e)
        {
            StartClicking();
        }

        // Runs when the "Stop" button is clicked
        private void btnStop_Click(object sender, EventArgs e)
        {
            StopClicking();
        }

        // Each tick from the timer is one click
        private void clickTimer_Tick(object sender, EventArgs e)
        {
            Mouse.LeftClick();
            count++;
            UpdateCount();
        }

        // First click: start waiting for the new hotkey. Clicking again (now "Cancel") gives up
        private void btnSetHotkey_Click(object sender, EventArgs e)
        {
            if (waitingForHotkey)
                FinishHotkeyCapture(hotkey);
            else
                StartHotkeyCapture();
        }

        #endregion

        #region Clicking

        private void StartClicking()
        {
            count = 0; // Reset the counter on every Start
            UpdateCount();

            // Set the timer interval to the value typed in the milliseconds field
            clickTimer.Interval = (int)numInterval.Value;
            clickTimer.Start();

            UpdateControls();
        }

        private void StopClicking()
        {
            clickTimer.Stop();
            UpdateControls();
        }

        // Used by the hotkey: the same key starts and stops
        private void ToggleClicking()
        {
            if (clickTimer.Enabled)
                StopClicking();
            else
                StartClicking();
        }

        #endregion

        #region Hotkey

        private void StartHotkeyCapture()
        {
            // Unregister the current one, otherwise Windows would catch it before our window sees it
            GlobalHotkey.Unregister(Handle, HotkeyId);
            waitingForHotkey = true;
            UpdateControls();
        }

        // Tries to register the new hotkey; if another program already uses it, keeps the old one
        private void FinishHotkeyCapture(Keys newHotkey)
        {
            waitingForHotkey = false;

            if (TryRegisterHotkey(newHotkey))
            {
                hotkey = newHotkey;
            }
            else
            {
                ShowHotkeyTakenWarning(newHotkey);
                if (!TryRegisterHotkey(hotkey))
                    hotkey = Keys.None;
            }

            UpdateControls();
        }

        // Keys.None means "no hotkey", so there is nothing to register
        private bool TryRegisterHotkey(Keys keys)
        {
            return keys != Keys.None && GlobalHotkey.Register(Handle, HotkeyId, keys);
        }

        private void ShowHotkeyTakenWarning(Keys keys)
        {
            MessageBox.Show("The hotkey " + HotkeyToText(keys) + " is already used by another program.\nChoose another one.",
                "MaxClicks", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }

        // Turns Keys.Control | Keys.F6 into "Ctrl+F6"
        private static string HotkeyToText(Keys keys)
        {
            if (keys == Keys.None)
                return "(none)";

            return new KeysConverter().ConvertToString(keys);
        }

        #endregion

        #region UI Updates

        private void UpdateCount()
        {
            lblCount.Text = "Clicks: " + count;
        }

        // The ONLY place that enables/disables controls and changes the hotkey texts.
        // It reads the current state (timer running? waiting for a hotkey?) and adjusts everything to match
        private void UpdateControls()
        {
            bool running = clickTimer.Enabled;

            // While running: only "Stop" is enabled, the interval and the hotkey are locked
            btnStart.Enabled = !running && !waitingForHotkey;
            btnStop.Enabled = running;
            numInterval.Enabled = !running;
            btnSetHotkey.Enabled = !running;

            btnSetHotkey.Text = waitingForHotkey ? "Cancel" : "Change";
            lblHotkey.Text = waitingForHotkey ? "Press a key..." : HotkeyToText(hotkey);
        }

        #endregion
    }
}
