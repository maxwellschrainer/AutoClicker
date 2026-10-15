using System;
using System.Windows.Forms;

namespace AutoClicker
{
    public partial class MaxClicks : Form
    {
        #region Fields

        // Number of clicks since the last Start
        private int count = 0;

        #endregion

        #region Constructor
        public MaxClicks()
        {
            InitializeComponent();
        }
        #endregion

        #region Event Handlers

        // Runs when the "Start" button is clicked
        private void btnStart_Click(object sender, EventArgs e)
        {
            count = 0; // Reset the counter on every Start
            UpdateCount();

            // Set the timer interval to the value typed in the milliseconds field
            clickTimer.Interval = (int)numInterval.Value;
            clickTimer.Start();

            UpdateButtons(running: true);
        }

        private void btnStop_Click(object sender, EventArgs e)
        {
            clickTimer.Stop();
            UpdateButtons(running: false);
        }

        // Each tick from the timer is one click
        private void clickTimer_Tick(object sender, EventArgs e)
        {
            Mouse.LeftClick();
            count++;
            UpdateCount();
        }

        #endregion

        #region Helper Methods

        private void UpdateCount()
        {
            lblCount.Text = "Clicks: " + count;
        }

        // Enables/disables the controls depending on whether the clicker is running
        // (while running: only "Stop" is enabled and the interval is locked)
        private void UpdateButtons(bool running)
        {
            btnStart.Enabled = !running;
            btnStop.Enabled = running;
            numInterval.Enabled = !running;
        }

        #endregion
    }
}
