using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows.Forms;
using AutoClicker.Properties;

namespace AutoClicker
{
    // The Macro screen: records keys and mouse clicks (with the waits between them) and plays them back,
    // a chosen number of times or forever, with a pause between runs
    public partial class MacroView : UserControl
    {
        #region Fields

        // The macro: every step in order
        private readonly List<MacroStep> steps = new List<MacroStep>();

        // --- Recording ---

        private readonly InputRecorder recorder = new InputRecorder();

        // Windows' clock (ms) at the last recorded step, to work out the wait before the next one
        private uint lastStepTime;

        // Longest wait a step can have: 1 hour (the same as numStepDelay.Maximum)
        private const int MaxDelay = 3600000;

        // --- Playing ---

        // Which run we are on (1, 2, 3...) and the index of the next step to play
        private int run;
        private int stepIndex;

        // Measures the time since Play. Each step has a planned moment (nextStepAt, in ms since Play)
        private readonly Stopwatch playClock = new Stopwatch();
        private long nextStepAt;

        // Keys and buttons the macro pressed and has not released yet; released on Stop,
        // so nothing stays "stuck" down
        private readonly List<MacroStep> held = new List<MacroStep>();

        // --- Designer ---

        // True when this screen is being drawn inside the Visual Studio Designer (not the real app)
        private readonly bool inDesigner;

        #endregion

        #region Constructor
        public MacroView()
        {
            InitializeComponent();

            // The Designer creates a real MacroView to draw it. This is only reliable inside the constructor
            inDesigner = LicenseManager.UsageMode == LicenseUsageMode.Designtime;

            recorder.Captured += recorder_Captured;
        }
        #endregion

        #region Control Lifecycle

        // Runs once when the screen is created: load the saved macro and options, register the hotkeys.
        // MaxClicks keeps this screen "visible" (just out of sight) until it is shown, so this runs at startup
        // and the hotkeys work before the user ever opens the Macro screen
        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            // Inside the Visual Studio Designer, don't register hotkeys or touch the settings
            if (inDesigner || DesignMode)
                return;

            LoadSettings();
            RefreshSteps();
            UpdateControls();
        }

        // Runs when the window closes: stop everything (releases held keys and the hooks) and save
        protected override void OnHandleDestroyed(EventArgs e)
        {
            if (!inDesigner && !DesignMode)
            {
                if (playTimer.Enabled)
                    StopPlaying();
                if (recorder.Running)
                    recorder.Stop();
                SaveMacroSettings();
            }
            base.OnHandleDestroyed(e);
        }

        #endregion

        #region Recording

        private void btnRecord_Click(object sender, EventArgs e)
        {
            ToggleRecording();
        }

        private void hkRecord_HotkeyPressed(object sender, EventArgs e)
        {
            ToggleRecording();
        }

        private void ToggleRecording()
        {
            if (recorder.Running)
                StopRecording();
            else if (!playTimer.Enabled)   // Can't record while playing
                StartRecording();
        }

        // A new recording replaces the current macro
        private void StartRecording()
        {
            if (!recorder.Start())
            {
                MessageBox.Show("Windows did not allow MaxClicks to record the keyboard and mouse.",
                    "MaxClicks", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            steps.Clear();
            RefreshSteps();
            UpdateControls();
        }

        private void StopRecording()
        {
            recorder.Stop();
            RemoveUnpairedSteps();
            RefreshSteps();

            // Saved right away, so a long recording is not lost if the app is killed
            SaveMacroSettings();
            UpdateControls();
        }

        // Runs for every key and mouse button event in Windows while recording
        private void recorder_Captured(MacroStep step, uint time)
        {
            if (IsOwnInput(step))
                return;

            // The first step starts right away: the time before it (going from the Record button
            // to the place to click) is not part of the macro
            if (steps.Count == 0)
                step.Delay = 0;
            else
                step.Delay = (int)Math.Min(MaxDelay, time - lastStepTime);  // uint math: also right when the clock wraps around

            lastStepTime = time;
            steps.Add(step);

            ListViewItem item = StepToItem(step, steps.Count);
            lvSteps.Items.Add(item);
            item.EnsureVisible();
            UpdateStatus();
        }

        // Input meant for MaxClicks itself is not recorded: clicks on its window (e.g. "Stop recording")
        // and the macro's own hotkeys (Windows shows the hook a hotkey before it handles it).
        // Keys are recorded even while MaxClicks is the active window: right after clicking Record it still is,
        // and the user expects the keys typed then to be recorded (ProcessCmdKey keeps them from acting on MaxClicks)
        private bool IsOwnInput(MacroStep step)
        {
            if (step.IsKeyStep)
            {
                return step.Key == (hkRecord.Hotkey & Keys.KeyCode)
                    || step.Key == (hkPlay.Hotkey & Keys.KeyCode);
            }
            return PhysicalScreen.IsThisAppAt(step.Position);
        }

        // While recording, keys typed into MaxClicks are recorded but must not act on it
        // (e.g. Space would press the focused "Stop recording" button). true = "handled", the key goes nowhere else
        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            if (recorder.Running)
                return true;

            return base.ProcessCmdKey(ref msg, keyData);
        }

        // Cleans the recording: a press with no release (e.g. Ctrl of a Ctrl+F8 hotkey) would leave the key
        // stuck down during playback, and a release with no press (a key already held when recording started)
        // is noise. Their waits are passed to the next step, so the timing of the others doesn't change
        private void RemoveUnpairedSteps()
        {
            List<int> openPresses = new List<int>();       // Indexes of presses not released yet
            List<int> toRemove = new List<int>();

            for (int i = 0; i < steps.Count; i++)
            {
                if (steps[i].IsPress)
                {
                    openPresses.Add(i);
                }
                else if (steps[i].IsRelease)
                {
                    int press = openPresses.FindIndex(p => steps[p].IsReleasedBy(steps[i]));
                    if (press >= 0)
                        openPresses.RemoveAt(press);
                    else
                        toRemove.Add(i);
                }
            }
            toRemove.AddRange(openPresses);
            toRemove.Sort();

            // From the end, so removing one doesn't shift the indexes of the others still to remove
            for (int i = toRemove.Count - 1; i >= 0; i--)
                RemoveStep(toRemove[i]);

            if (steps.Count > 0)
                steps[0].Delay = 0;
        }

        // Removes one step; its wait goes to the next one, so the steps after it keep their timing
        private void RemoveStep(int index)
        {
            if (index + 1 < steps.Count)
                steps[index + 1].Delay = Math.Min(MaxDelay, steps[index + 1].Delay + steps[index].Delay);

            steps.RemoveAt(index);
        }

        #endregion

        #region Playing

        private void btnPlay_Click(object sender, EventArgs e)
        {
            TogglePlaying();
        }

        private void hkPlay_HotkeyPressed(object sender, EventArgs e)
        {
            TogglePlaying();
        }

        private void TogglePlaying()
        {
            if (playTimer.Enabled)
                StopPlaying();
            else if (!recorder.Running && steps.Count > 0)   // Can't play while recording, or with nothing to play
                StartPlaying();
        }

        private void StartPlaying()
        {
            run = 1;
            stepIndex = 0;
            nextStepAt = steps[0].Delay;
            held.Clear();

            // Also saved here, not only on close: if the app is killed or crashes, the choices are not lost
            SaveMacroSettings();

            playClock.Restart();
            playTimer.Start();
            UpdateControls();

            PlayDueSteps(); // Steps with no wait go right now, not on the first tick
        }

        private void StopPlaying()
        {
            playTimer.Stop();
            playClock.Stop();
            ReleaseHeld();
            UpdateControls();
            UpdateStatus();
        }

        private void playTimer_Tick(object sender, EventArgs e)
        {
            PlayDueSteps();
        }

        // Plays every step whose moment has come. The moments are counted from Play (nextStepAt), not from
        // the last tick: the Timer is up to ~15 ms late each time, and this way that lateness never adds up
        private void PlayDueSteps()
        {
            while (playTimer.Enabled && playClock.ElapsedMilliseconds >= nextStepAt)
            {
                PlayStep(stepIndex);
                stepIndex++;

                if (stepIndex < steps.Count)
                {
                    nextStepAt += steps[stepIndex].Delay;
                    continue;
                }

                // End of a run
                if (!chkForever.Checked && run >= numRepeat.Value)
                {
                    StopPlaying();
                    return;
                }
                run++;
                stepIndex = 0;
                nextStepAt += (long)numPause.Value + steps[0].Delay;

                // Wait for the next tick before the next run: a macro with no waits at all would
                // otherwise keep this loop busy forever and freeze the window
                break;
            }
            UpdateStatus();
        }

        // Plays one step, shows it selected in the list, and keeps track of what is held down
        private void PlayStep(int index)
        {
            MacroStep step = steps[index];
            step.Play();

            if (step.IsPress)
            {
                held.Add(step);
            }
            else if (step.IsRelease)
            {
                int press = held.FindIndex(h => h.IsReleasedBy(step));
                if (press >= 0)
                    held.RemoveAt(press);
            }

            lvSteps.Items[index].Selected = true;
            lvSteps.Items[index].EnsureVisible();
        }

        // Lets go of every key and button the macro left pressed (stopped in the middle of a click or a key hold)
        private void ReleaseHeld()
        {
            foreach (MacroStep step in held)
                step.CreateRelease().Play();

            held.Clear();
        }

        private void chkForever_CheckedChanged(object sender, EventArgs e)
        {
            UpdateControls();
            UpdateStatus();
        }

        private void numRepeat_ValueChanged(object sender, EventArgs e)
        {
            UpdateStatus();
        }

        private void numPause_ValueChanged(object sender, EventArgs e)
        {
            UpdateStatus();
        }

        #endregion

        #region Editing Steps

        // Selecting a step shows its wait in the "Wait" field, where it can be changed
        private void lvSteps_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (SelectedIndex >= 0)
                numStepDelay.Value = Math.Min(numStepDelay.Maximum, steps[SelectedIndex].Delay);

            UpdateControls();
        }

        private void numStepDelay_ValueChanged(object sender, EventArgs e)
        {
            // Also fires when a selection fills the field; then the value is the same and nothing changes
            if (SelectedIndex < 0 || IsBusy)
                return;

            steps[SelectedIndex].Delay = (int)numStepDelay.Value;
            lvSteps.Items[SelectedIndex].SubItems[1].Text = WaitText(steps[SelectedIndex]);
            UpdateStatus();
        }

        private void btnDeleteStep_Click(object sender, EventArgs e)
        {
            DeleteSelectedStep();
        }

        // The Delete key in the list does the same as the Delete button
        private void lvSteps_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Delete && !IsBusy)
                DeleteSelectedStep();
        }

        private void DeleteSelectedStep()
        {
            int index = SelectedIndex;
            if (index < 0)
                return;

            RemoveStep(index);
            RefreshSteps();

            // Select the step that took its place, so several can be deleted in a row
            if (steps.Count > 0)
                lvSteps.Items[Math.Min(index, steps.Count - 1)].Selected = true;
        }

        private void btnClear_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show("Delete all " + steps.Count + " steps?", "MaxClicks",
                    MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes)
                return;

            steps.Clear();
            RefreshSteps();
        }

        // Index of the selected step, or -1 when none is selected
        private int SelectedIndex => lvSteps.SelectedIndices.Count == 1 ? lvSteps.SelectedIndices[0] : -1;

        #endregion

        #region Hotkeys

        // Runs when the user picks a new Record or Play hotkey and Windows accepted it
        private void hotkey_HotkeyChanged(object sender, EventArgs e)
        {
            Settings.Default.MacroRecordHotkey = hkRecord.Hotkey;
            Settings.Default.MacroPlayHotkey = hkPlay.Hotkey;
            Settings.Default.Save();
            UpdateControls();
        }

        #endregion

        #region Saved Settings

        // Reads what the last run saved (or the defaults from Settings.settings on the first run)
        private void LoadSettings()
        {
            hkRecord.SetHotkey(Settings.Default.MacroRecordHotkey);
            hkPlay.SetHotkey(Settings.Default.MacroPlayHotkey);

            // Assigning a value outside Minimum..Maximum would throw, and the saved file can be edited by hand
            numRepeat.Value = Math.Max(numRepeat.Minimum, Math.Min(numRepeat.Maximum, Settings.Default.MacroRepeat));
            numPause.Value = Math.Max(numPause.Minimum, Math.Min(numPause.Maximum, Settings.Default.MacroPause));
            chkForever.Checked = Settings.Default.MacroForever;

            // One step per line of numbers, separated by ";". If any step is broken, the macro is not loaded
            steps.Clear();
            string saved = Settings.Default.MacroSteps;
            if (string.IsNullOrEmpty(saved))
                return;

            foreach (string text in saved.Split(';'))
            {
                MacroStep step = MacroStep.FromSaveText(text);
                if (step == null)
                {
                    steps.Clear();
                    return;
                }
                step.Delay = Math.Min(MaxDelay, step.Delay);
                steps.Add(step);
            }
        }

        // The steps and the playback options. Called after recording, on Play and when the app closes
        // (edits and options can change many times, only the values in use matter)
        private void SaveMacroSettings()
        {
            List<string> texts = new List<string>();
            foreach (MacroStep step in steps)
                texts.Add(step.ToSaveText());

            Settings.Default.MacroSteps = string.Join(";", texts);
            Settings.Default.MacroRepeat = (int)numRepeat.Value;
            Settings.Default.MacroForever = chkForever.Checked;
            Settings.Default.MacroPause = (int)numPause.Value;
            Settings.Default.Save();
        }

        #endregion

        #region UI Updates

        // True while recording or playing: then the macro can't be edited
        private bool IsBusy => recorder.Running || playTimer.Enabled;

        // Rebuilds the whole list from "steps" (after loading, deleting, clearing, or a new recording)
        private void RefreshSteps()
        {
            lvSteps.BeginUpdate(); // Don't redraw after every item, only once at the end
            lvSteps.Items.Clear();
            for (int i = 0; i < steps.Count; i++)
                lvSteps.Items.Add(StepToItem(steps[i], i + 1));
            lvSteps.EndUpdate();

            UpdateControls();
            UpdateStatus();
        }

        // One line of the list: number, wait, action
        private static ListViewItem StepToItem(MacroStep step, int number)
        {
            return new ListViewItem(new[] { number.ToString(), WaitText(step), step.Description });
        }

        private static string WaitText(MacroStep step)
        {
            return step.Delay + " ms";
        }

        // The line under the buttons: what is happening right now
        private void UpdateStatus()
        {
            if (recorder.Running)
            {
                lblStatus.Text = "Recording... " + steps.Count + " steps. Stop: " + HotkeyButton.HotkeyToText(hkRecord.Hotkey);
            }
            else if (playTimer.Enabled)
            {
                string runs = chkForever.Checked ? "∞" : numRepeat.Value.ToString(); // ∞ = infinity sign
                int shownStep = Math.Max(1, stepIndex);   // stepIndex is the NEXT step; 0 between runs
                lblStatus.Text = "Run " + run + " / " + runs + "  -  step " + shownStep + " / " + steps.Count;
            }
            else if (steps.Count == 0)
            {
                lblStatus.Text = "No steps yet. Press Record to start.";
            }
            else
            {
                long runTime = 0;
                foreach (MacroStep step in steps)
                    runTime += step.Delay;

                lblStatus.Text = steps.Count + " steps, " + (runTime / 1000.0).ToString("0.0") + " s per run";
            }
        }

        // The ONLY place that enables/disables controls and changes the button texts.
        // It reads the current state (recording? playing? a step selected?) and adjusts everything to match
        private void UpdateControls()
        {
            bool recording = recorder.Running;
            bool playing = playTimer.Enabled;
            bool stepSelected = SelectedIndex >= 0;

            btnRecord.Enabled = !playing;
            btnRecord.Text = WithHotkey(recording ? "Stop recording" : "Record", hkRecord.Hotkey);
            btnPlay.Enabled = !recording && steps.Count > 0;
            btnPlay.Text = WithHotkey(playing ? "Stop" : "Play", hkPlay.Hotkey);

            numStepDelay.Enabled = !IsBusy && stepSelected;
            btnDeleteStep.Enabled = !IsBusy && stepSelected;
            btnClear.Enabled = !IsBusy && steps.Count > 0;

            numRepeat.Enabled = !playing && !chkForever.Checked;
            chkForever.Enabled = !playing;
            numPause.Enabled = !playing;

            hkRecord.Enabled = !IsBusy;
            hkPlay.Enabled = !IsBusy;
        }

        // "Record" + F8 = "Record (F8)": the big buttons show their hotkey, so it is clear that the
        // small key buttons below only change the hotkeys
        private static string WithHotkey(string text, Keys hotkey)
        {
            return hotkey == Keys.None ? text : text + " (" + HotkeyButton.HotkeyToText(hotkey) + ")";
        }

        #endregion
    }
}
