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

        // Current hotkey: key + modifiers (e.g. Keys.Control | Keys.F6)
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

            if (!GlobalHotkey.Register(Handle, HotkeyId, hotkey))
            {
                MessageBox.Show("The hotkey " + HotkeyToText(hotkey) + " is already used by another program.\nChoose another one.",
                    "MaxClicks", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            UpdateHotkeyLabel();
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
            {
                FinishHotkeyCapture(hotkey);
                return;
            }

            // Unregister the current one, otherwise Windows would catch it before our window sees it
            GlobalHotkey.Unregister(Handle, HotkeyId);
            waitingForHotkey = true;

            lblHotkey.Text = "Press a key...";
            btnSetHotkey.Text = "Cancel";
            btnStart.Enabled = false;
        }

        #endregion

        #region Helper Methods

        private void StartClicking()
        {
            count = 0; // Reset the counter on every Start
            UpdateCount();

            // Set the timer interval to the value typed in the milliseconds field
            clickTimer.Interval = (int)numInterval.Value;
            clickTimer.Start();

            UpdateButtons(running: true);
        }

        private void StopClicking()
        {
            clickTimer.Stop();
            UpdateButtons(running: false);
        }

        // Used by the hotkey: the same key starts and stops
        private void ToggleClicking()
        {
            if (clickTimer.Enabled)
                StopClicking();
            else
                StartClicking();
        }

        // Tries to register the new hotkey; if another program already uses it, keeps the old one
        private void FinishHotkeyCapture(Keys newHotkey)
        {
            waitingForHotkey = false;

            if (GlobalHotkey.Register(Handle, HotkeyId, newHotkey))
            {
                hotkey = newHotkey;
            }
            else
            {
                MessageBox.Show("The hotkey " + HotkeyToText(newHotkey) + " is already used by another program.\nChoose another one.",
                    "MaxClicks", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                GlobalHotkey.Register(Handle, HotkeyId, hotkey);
            }

            btnSetHotkey.Text = "Change";
            btnStart.Enabled = true;
            UpdateHotkeyLabel();
        }

        private void UpdateHotkeyLabel()
        {
            lblHotkey.Text = HotkeyToText(hotkey);
        }

        // Turns Keys.Control | Keys.F6 into "Ctrl+F6"
        private static string HotkeyToText(Keys keys)
        {
            return new KeysConverter().ConvertToString(keys);
        }

        private void UpdateCount()
        {
            lblCount.Text = "Clicks: " + count;
        }

        // Enables/disables the controls depending on whether the clicker is running
        // (while running: only "Stop" is enabled, the interval and the hotkey are locked)
        private void UpdateButtons(bool running)
        {
            btnStart.Enabled = !running;
            btnStop.Enabled = running;
            numInterval.Enabled = !running;
            btnSetHotkey.Enabled = !running;
        }

        #endregion
    }
}
