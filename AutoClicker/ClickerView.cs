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
            UpdateControls();
        }

        // Runs when the window closes and this control is destroyed: remember the click settings
        // (hkToggle releases the hotkey by itself)
        protected override void OnHandleDestroyed(EventArgs e)
        {
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

        // The hotkey was pressed (from any program): the same key starts and stops
        private void hkToggle_HotkeyPressed(object sender, EventArgs e)
        {
            ToggleClicking();
        }

        private void ToggleClicking()
        {
            if (clickTimer.Enabled)
                StopClicking();
            else
                StartClicking();
        }

        // Each tick from the timer is one click or one double click. The counter counts real clicks,
        // so a double click adds 2. Clicks Windows refused (e.g. while the PC is locked) are not counted,
        // so the limit is only reached by clicks that really happened
        private void clickTimer_Tick(object sender, EventArgs e)
        {
            // If the limit has room for only one more click (e.g. limit 5 with double clicks: 2 + 2 + 1),
            // the last one is a single click, so the clicker never goes past the limit
            if (SelectedDoubleClick && !OnlyOneClickLeft)
            {
                if (Mouse.DoubleClick(SelectedButton))
                    count += 2;
            }
            else
            {
                if (Mouse.Click(SelectedButton))
                    count++;
            }
            UpdateCount();

            // Click limit reached: stop by itself, just like pressing "Stop"
            if (LimitReached)
                StopClicking();
        }

        // True when "Stop after" is checked and the counter got to the chosen number
        private bool LimitReached => chkLimit.Checked && count >= numLimit.Value;

        // True when "Stop after" is checked and exactly one click is missing to reach it
        private bool OnlyOneClickLeft => chkLimit.Checked && numLimit.Value - count == 1;

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

        // Runs when the limit number changes (typed, arrows, or loaded from the settings):
        // keeps the "/ limit" part of the counter in step with the field
        private void numLimit_ValueChanged(object sender, EventArgs e)
        {
            UpdateCount();
        }

        #endregion

        #region Hotkey

        // Runs when the user picked a new hotkey and Windows accepted it. If the saved hotkey is taken at
        // startup, hkToggle shows "(none)" but this doesn't run, so the file keeps the user's choice
        private void hkToggle_HotkeyChanged(object sender, EventArgs e)
        {
            Settings.Default.Hotkey = hkToggle.Hotkey;
            Settings.Default.Save();
            UpdateControls();
        }

        #endregion

        #region Saved Settings

        // Reads what the last run saved (or the defaults from Settings.settings on the first run)
        private void LoadSettings()
        {
            hkToggle.SetHotkey(Settings.Default.Hotkey);

            // Assigning a value outside Minimum..Maximum would throw, and the saved file can be edited by hand
            numInterval.Value = Math.Max(numInterval.Minimum, Math.Min(numInterval.Maximum, Settings.Default.Interval));

            cmbButton.SelectedIndex = Settings.Default.ClickButton == MouseButtons.Right ? 1 : 0;
            cmbClickType.SelectedIndex = Settings.Default.DoubleClick ? 1 : 0;

            chkLimit.Checked = Settings.Default.LimitEnabled;
            numLimit.Value = Math.Max(numLimit.Minimum, Math.Min(numLimit.Maximum, Settings.Default.ClickLimit));
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

        // The ONLY place that enables/disables controls and changes the button texts.
        // It reads the current state (timer running?) and adjusts everything to match
        private void UpdateControls()
        {
            bool running = clickTimer.Enabled;

            // While running: only "Stop" is enabled, the interval, click options, limit and hotkey are locked
            btnStart.Enabled = !running;
            btnStop.Enabled = running;
            numInterval.Enabled = !running;
            cmbButton.Enabled = !running;
            cmbClickType.Enabled = !running;
            chkLimit.Enabled = !running;
            numLimit.Enabled = !running && chkLimit.Checked;
            hkToggle.Enabled = !running;

            btnStart.Text = HotkeyButton.AddHotkey("Start", hkToggle.Hotkey);
            btnStop.Text = HotkeyButton.AddHotkey("Stop", hkToggle.Hotkey);
        }

        #endregion
    }
}
