using System;
using System.Windows.Forms;

namespace AutoClicker
{
    // The main window: a header with the title and the switch button, and below it
    // the area (pnlViews) where one of the two screens is shown: ClickerView or MacroView
    public partial class MaxClicks : Form
    {
        #region Fields

        // Screen on display right now
        private UserControl currentView;

        // Screen sliding in during the animation
        private UserControl nextView;

        // --- Slide animation ---

        // How many timer ticks the slide takes (12 x ~15 ms = about 0.2 seconds)
        private const int SlideSteps = 12;

        // Which tick of the slide we are on (0 to SlideSteps)
        private int slideStep;

        // 1 = the new screen comes in from the right, -1 = from the left
        private int slideDirection;

        #endregion

        #region Constructor
        public MaxClicks()
        {
            InitializeComponent();

            currentView = clickerView;
            UpdateHeader();
        }
        #endregion

        #region Startup

        // Runs once, right after the window first appears.
        // macroView starts "visible" but out of sight (Left = width, outside pnlViews), because a hidden
        // UserControl is not created (no OnLoad, no window handle) until it is shown: its hotkeys would
        // only start working after the first visit to the Macro screen. Now that it is created, hide it
        protected override void OnShown(EventArgs e)
        {
            base.OnShown(e);
            macroView.Visible = false;
        }

        #endregion

        #region Switching Screens

        // Runs when the switch button is clicked: slide to the other screen
        private void btnSwitchView_Click(object sender, EventArgs e)
        {
            if (currentView == clickerView)
                SlideTo(macroView, 1);      // Clicker -> Macro: comes in from the right
            else
                SlideTo(clickerView, -1);   // Macro -> Clicker: comes back from the left
        }

        // Puts the new screen just outside the visible area and starts the animation
        private void SlideTo(UserControl view, int direction)
        {
            nextView = view;
            slideDirection = direction;
            slideStep = 0;

            nextView.Left = direction * pnlViews.Width; // Right outside the edge
            nextView.Visible = true;

            btnSwitchView.Enabled = false; // No double click in the middle of the slide
            slideTimer.Start();
        }

        // Each tick moves both screens a little: the current one goes out, the next one comes in
        private void slideTimer_Tick(object sender, EventArgs e)
        {
            slideStep++;

            int width = pnlViews.Width;
            int offset = width * slideStep / SlideSteps; // How far we have moved so far

            currentView.Left = -slideDirection * offset;
            nextView.Left = slideDirection * (width - offset);

            if (slideStep == SlideSteps)
                FinishSlide();
        }

        // The new screen is in place: hide the old one and update the header
        private void FinishSlide()
        {
            slideTimer.Stop();

            currentView.Visible = false;
            currentView.Left = 0;

            currentView = nextView;
            nextView = null;

            btnSwitchView.Enabled = true;
            UpdateHeader();
        }

        #endregion

        #region UI Updates

        // Title shows where you are, the button shows where it takes you
        private void UpdateHeader()
        {
            bool onClicker = currentView == clickerView;

            lblTitle.Text = onClicker ? "Auto Clicker" : "Macro";
            btnSwitchView.Text = onClicker ? "Macro  \u25BA" : "\u25C4  Clicker"; // \u25BA = right arrow, \u25C4 = left arrow
        }

        #endregion
    }
}