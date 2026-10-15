using System;
using System.Windows.Forms;

namespace AutoClicker
{
    public partial class MaxClicks : Form
    {
        #region Fields

        // Number of clicks since the last Start
        int count = 0;

        #endregion

        #region Constructor
        public MaxClicks()
        {
            InitializeComponent();
        }
        #endregion

        #region Event Handlers
        private void btnStart_Click(object sender, EventArgs e)
        {
            count = 0;
            UpdateCount();

            // The Timer raises the Tick event every "Interval" milliseconds
            clickTimer.Interval = (int)numInterval.Value;
            clickTimer.Start();

            UpdateButtons(running: true);
        }

        private void btnStop_Click(object sender, EventArgs e)
        {
            clickTimer.Stop();
            UpdateButtons(running: false);
        }

        // Called automatically by the Timer: each Tick is one click
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

        // While clicking, only Stop is enabled (and the interval can't be changed)
        private void UpdateButtons(bool running)
        {
            btnStart.Enabled = !running;
            btnStop.Enabled = running;
            numInterval.Enabled = !running;
        }
        #endregion
    }
}
