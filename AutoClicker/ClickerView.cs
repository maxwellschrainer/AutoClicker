using System;
using System.ComponentModel;
using System.Windows.Forms;
using AutoClicker.Properties;

namespace AutoClicker
{
    // The Auto Clicker screen. It is a UserControl: a piece of screen with its own Designer and code,
    // placed inside the main window (MaxClicks) like any other control
    public partial class ClickerView : UserControl
    {
        #region Fields

        // --- Clicking ---

        // Number of clicks since the last Start
        private int count = 0;

        // --- Hotkey ---

        // Id we give Windows for our hotkey (any number, it only has to be unique in this app)
        private const int HotkeyId = 1;

        // Current hotkey: key + modifiers (e.g. Keys.Control | Keys.F6).
        // Keys.None means no hotkey is registered. The real value comes from the settings in OnLoad
        private Keys hotkey = Keys.None;

        // True while we wait for the user to press the new hotkey
        private bool waitingForHotkey = false;

        // --- Designer ---

        // True when this screen is being drawn inside the Visual Studio Designer (not the real app)
        private readonly bool inDesigner;

        #endregion

        #region Constructor
        public ClickerView()
        {
            InitializeComponent();

            // The Designer creates a real ClickerView to draw it. This is only reliable inside the constructor
            inDesigner = LicenseManager.UsageMode == LicenseUsageMode.Designtime;
        }
        #endregion

        #region Control Lifecycle

        // Runs once when the screen is first shown: load the saved settings and register the hotkey
        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            // Inside the Visual Studio Designer, don't register anything: otherwise Visual Studio itself
            // would hold the hotkey (and start clicking when it is pressed) while the Designer tab is open
            if (inDesigner || DesignMode)
                return;

            LoadSettings();

            if (!TryRegisterHotkey(hotkey))
            {
                ShowHotkeyTakenWarning(hotkey);
                hotkey = Keys.None; // So the screen doesn't show a hotkey that doesn't work
            }
            UpdateControls();
        }

        // Runs when this screen is hidden (e.g. switching to Macro) or shown again.
        // If the user was choosing a new hotkey, cancel it: the keys can't reach a hidden screen
        protected override void OnVisibleChanged(EventArgs e)
        {
            base.OnVisibleChanged(e);

            if (!Visible && waitingForHotkey)
                FinishHotkeyCapture(hotkey);
        }

        // Runs when the window closes and this control is destroyed:
        // release the hotkey, so Windows frees it for other programs, and remember the click settings
        protected override void OnHandleDestroyed(EventArgs e)
        {
            GlobalHotkey.Unregister(Handle, HotkeyId);

            if (!inDesigner && !DesignMode)
                SaveClickSettings();

            base.OnHandleDestroyed(e);
        }

        #endregion

        #region Clicking

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

        private void StartClicking()
        {
            count = 0; // Reset the counter on every Start
            UpdateCount();

            // Set the timer interval to the value typed in the milliseconds field
            clickTimer.Interval = (int)numInterval.Value;
            clickTimer.Start();

            // Also saved here, not only on close: if the app is killed or crashes, the choices are not lost
            SaveClickSettings();

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

        // Each tick from the timer is one click (or one double click, counted as 1)
        private void clickTimer_Tick(object sender, EventArgs e)
        {
            if (SelectedDoubleClick)
                Mouse.DoubleClick(SelectedButton);
            else
                Mouse.Click(SelectedButton);

            count++;
            UpdateCount();

            // Click limit reached: stop by itself, just like pressing "Stop"
            if (LimitReached)
                StopClicking();
        }

        // True when "Stop after" is checked and the counter got to the chosen number
        private bool LimitReached => chkLimit.Checked && count >= numLimit.Value;

        // The options chosen in the "Click" group, translated from the combo boxes' positions.
        // Item 0 is "Left button" / "Single click", item 1 is "Right button" / "Double click"
        private MouseButtons SelectedButton => cmbButton.SelectedIndex == 1 ? MouseButtons.Right : MouseButtons.Left;
        private bool SelectedDoubleClick => cmbClickType.SelectedIndex == 1;

        // Runs when "Stop after" is checked or unchecked: the number field only makes sense while it is checked
        private void chkLimit_CheckedChanged(object sender, EventArgs e)
        {
            UpdateControls();
            UpdateCount();
        }

        #endregion

        #region Hotkey: Choosing a New One

        // Step 1 - First click: start waiting for the new hotkey. Clicking again (now "Cancel") gives up
        private void btnSetHotkey_Click(object sender, EventArgs e)
        {
            if (waitingForHotkey)
                FinishHotkeyCapture(hotkey);
            else
                StartHotkeyCapture();
        }

        // Step 2 - Get ready to receive the key
        private void StartHotkeyCapture()
        {
            // Unregister the current one, otherwise Windows would catch it before our window sees it
            GlobalHotkey.Unregister(Handle, HotkeyId);
            waitingForHotkey = true;
            UpdateControls();
        }

        // Step 3 - Receives every key pressed in this screen before its controls do.
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

        // Step 4 - Tries to register the new hotkey; if another program already uses it, keeps the old one
        private void FinishHotkeyCapture(Keys newHotkey)
        {
            waitingForHotkey = false;

            if (TryRegisterHotkey(newHotkey))
            {
                hotkey = newHotkey;
                SaveHotkey();
            }
            else
            {
                ShowHotkeyTakenWarning(newHotkey);
                if (!TryRegisterHotkey(hotkey))
                    hotkey = Keys.None;
            }

            UpdateControls();
        }

        #endregion

        #region Hotkey: Registering and Receiving

        // Shortcut for GlobalHotkey.Register that always uses this control and our HotkeyId.
        // Returns true if Windows accepted the hotkey, false if it was refused (taken by another program).
        // Keys.None means "no hotkey": there is nothing to register, so it just returns false
        private bool TryRegisterHotkey(Keys keys)
        {
            return keys != Keys.None && GlobalHotkey.Register(Handle, HotkeyId, keys);
        }

        // Every message Windows sends to this control goes through here.
        // We only care about WM_HOTKEY: our hotkey was pressed (even with another app in focus,
        // and even while the Macro screen is the one on display)
        protected override void WndProc(ref Message m)
        {
            if (m.Msg == GlobalHotkey.WM_HOTKEY && (int)m.WParam == HotkeyId)
            {
                ToggleClicking();
            }
            base.WndProc(ref m);
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

        #region Saved Settings

        // Reads what the last run saved (or the defaults from Settings.settings on the first run)
        private void LoadSettings()
        {
            hotkey = Settings.Default.Hotkey;

            // Assigning a value outside Minimum..Maximum would throw, and the saved file can be edited by hand
            numInterval.Value = Math.Max(numInterval.Minimum, Math.Min(numInterval.Maximum, Settings.Default.Interval));

            cmbButton.SelectedIndex = Settings.Default.ClickButton == MouseButtons.Right ? 1 : 0;
            cmbClickType.SelectedIndex = Settings.Default.DoubleClick ? 1 : 0;

            chkLimit.Checked = Settings.Default.LimitEnabled;
            numLimit.Value = Math.Max(numLimit.Minimum, Math.Min(numLimit.Maximum, Settings.Default.ClickLimit));
        }

        // Saved only when the user picks a new hotkey that works. If the saved one is taken at startup,
        // the screen shows "(none)" but the file keeps the old choice, so the next run tries it again
        private void SaveHotkey()
        {
            Settings.Default.Hotkey = hotkey;
            Settings.Default.Save();
        }

        // Interval, button, single/double click and click limit. Called on Start and when the app closes
        // (the fields can change many times, only the values in use matter)
        private void SaveClickSettings()
        {
            Settings.Default.Interval = (int)numInterval.Value;
            Settings.Default.ClickButton = SelectedButton;
            Settings.Default.DoubleClick = SelectedDoubleClick;
            Settings.Default.LimitEnabled = chkLimit.Checked;
            Settings.Default.ClickLimit = (int)numLimit.Value;
            Settings.Default.Save();
        }

        #endregion

        #region UI Updates

        private void UpdateCount()
        {
            // With a limit, also show the target: "Clicks: 40 / 100"
            lblCount.Text = "Clicks: " + count + (chkLimit.Checked ? " / " + numLimit.Value : "");
        }

        // The ONLY place that enables/disables controls and changes the hotkey texts.
        // It reads the current state (timer running? waiting for a hotkey?) and adjusts everything to match
        private void UpdateControls()
        {
            bool running = clickTimer.Enabled;

            // While running: only "Stop" is enabled, the interval, click options, limit and hotkey are locked
            btnStart.Enabled = !running && !waitingForHotkey;
            btnStop.Enabled = running;
            numInterval.Enabled = !running;
            cmbButton.Enabled = !running;
            cmbClickType.Enabled = !running;
            chkLimit.Enabled = !running;
            numLimit.Enabled = !running && chkLimit.Checked;
            btnSetHotkey.Enabled = !running;

            btnSetHotkey.Text = waitingForHotkey ? "Cancel" : "Change";
            lblHotkey.Text = waitingForHotkey ? "Press a key..." : HotkeyToText(hotkey);
        }

        #endregion
    }
}
