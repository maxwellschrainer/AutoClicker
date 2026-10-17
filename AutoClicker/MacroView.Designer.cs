namespace AutoClicker
{
    partial class MacroView
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Component Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this.lvSteps = new System.Windows.Forms.ListView();
            this.colNumber = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.colWait = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.colAction = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.lblWait = new System.Windows.Forms.Label();
            this.numStepDelay = new System.Windows.Forms.NumericUpDown();
            this.lblWaitMs = new System.Windows.Forms.Label();
            this.btnDeleteStep = new System.Windows.Forms.Button();
            this.btnClear = new System.Windows.Forms.Button();
            this.btnLoad = new System.Windows.Forms.Button();
            this.btnSave = new System.Windows.Forms.Button();
            this.grpPlayback = new System.Windows.Forms.GroupBox();
            this.lblSpeedX = new System.Windows.Forms.Label();
            this.numSpeed = new System.Windows.Forms.NumericUpDown();
            this.lblSpeed = new System.Windows.Forms.Label();
            this.lblPauseMs = new System.Windows.Forms.Label();
            this.numPause = new System.Windows.Forms.NumericUpDown();
            this.lblPause = new System.Windows.Forms.Label();
            this.chkForever = new System.Windows.Forms.CheckBox();
            this.lblTimes = new System.Windows.Forms.Label();
            this.numRepeat = new System.Windows.Forms.NumericUpDown();
            this.lblRepeat = new System.Windows.Forms.Label();
            this.grpRecording = new System.Windows.Forms.GroupBox();
            this.chkRecordMoves = new System.Windows.Forms.CheckBox();
            this.chkAppend = new System.Windows.Forms.CheckBox();
            this.grpHotkeys = new System.Windows.Forms.GroupBox();
            this.hkPlay = new AutoClicker.HotkeyButton();
            this.lblPlayHotkey = new System.Windows.Forms.Label();
            this.hkRecord = new AutoClicker.HotkeyButton();
            this.lblRecordHotkey = new System.Windows.Forms.Label();
            this.btnRecord = new System.Windows.Forms.Button();
            this.btnPlay = new System.Windows.Forms.Button();
            this.lblStatus = new System.Windows.Forms.Label();
            this.playTimer = new System.Windows.Forms.Timer(this.components);
            this.toolTip = new System.Windows.Forms.ToolTip(this.components);
            this.openFileDialog = new System.Windows.Forms.OpenFileDialog();
            this.saveFileDialog = new System.Windows.Forms.SaveFileDialog();
            ((System.ComponentModel.ISupportInitialize)(this.numStepDelay)).BeginInit();
            this.grpPlayback.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numSpeed)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numPause)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numRepeat)).BeginInit();
            this.grpRecording.SuspendLayout();
            this.grpHotkeys.SuspendLayout();
            this.SuspendLayout();
            //
            // lvSteps
            //
            this.lvSteps.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.colNumber,
            this.colWait,
            this.colAction});
            this.lvSteps.FullRowSelect = true;
            this.lvSteps.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.Nonclickable;
            this.lvSteps.HideSelection = false;
            this.lvSteps.Location = new System.Drawing.Point(12, 12);
            this.lvSteps.MultiSelect = false;
            this.lvSteps.Name = "lvSteps";
            this.lvSteps.Size = new System.Drawing.Size(260, 120);
            this.lvSteps.TabIndex = 0;
            this.lvSteps.UseCompatibleStateImageBehavior = false;
            this.lvSteps.View = System.Windows.Forms.View.Details;
            this.lvSteps.SelectedIndexChanged += new System.EventHandler(this.lvSteps_SelectedIndexChanged);
            this.lvSteps.KeyDown += new System.Windows.Forms.KeyEventHandler(this.lvSteps_KeyDown);
            //
            // colNumber
            //
            this.colNumber.Text = "#";
            this.colNumber.Width = 32;
            //
            // colWait
            //
            this.colWait.Text = "Wait";
            this.colWait.Width = 62;
            //
            // colAction
            //
            this.colAction.Text = "Action";
            this.colAction.Width = 145;
            //
            // lblWait
            //
            this.lblWait.AutoSize = true;
            this.lblWait.Location = new System.Drawing.Point(12, 141);
            this.lblWait.Name = "lblWait";
            this.lblWait.Size = new System.Drawing.Size(29, 13);
            this.lblWait.TabIndex = 1;
            this.lblWait.Text = "Wait";
            //
            // numStepDelay
            //
            this.numStepDelay.Enabled = false;
            this.numStepDelay.Increment = new decimal(new int[] {
            10,
            0,
            0,
            0});
            this.numStepDelay.Location = new System.Drawing.Point(46, 139);
            this.numStepDelay.Maximum = new decimal(new int[] {
            3600000,
            0,
            0,
            0});
            this.numStepDelay.Name = "numStepDelay";
            this.numStepDelay.Size = new System.Drawing.Size(72, 20);
            this.numStepDelay.TabIndex = 2;
            this.numStepDelay.ThousandsSeparator = true;
            this.toolTip.SetToolTip(this.numStepDelay, "Wait before the selected step");
            this.numStepDelay.ValueChanged += new System.EventHandler(this.numStepDelay_ValueChanged);
            //
            // lblWaitMs
            //
            this.lblWaitMs.AutoSize = true;
            this.lblWaitMs.Location = new System.Drawing.Point(121, 141);
            this.lblWaitMs.Name = "lblWaitMs";
            this.lblWaitMs.Size = new System.Drawing.Size(20, 13);
            this.lblWaitMs.TabIndex = 3;
            this.lblWaitMs.Text = "ms";
            //
            // btnDeleteStep
            //
            this.btnDeleteStep.Enabled = false;
            this.btnDeleteStep.Location = new System.Drawing.Point(152, 137);
            this.btnDeleteStep.Name = "btnDeleteStep";
            this.btnDeleteStep.Size = new System.Drawing.Size(58, 24);
            this.btnDeleteStep.TabIndex = 4;
            this.btnDeleteStep.Text = "Delete";
            this.toolTip.SetToolTip(this.btnDeleteStep, "Delete the selected step (Del)");
            this.btnDeleteStep.UseVisualStyleBackColor = true;
            this.btnDeleteStep.Click += new System.EventHandler(this.btnDeleteStep_Click);
            //
            // btnClear
            //
            this.btnClear.Enabled = false;
            this.btnClear.Location = new System.Drawing.Point(214, 137);
            this.btnClear.Name = "btnClear";
            this.btnClear.Size = new System.Drawing.Size(58, 24);
            this.btnClear.TabIndex = 5;
            this.btnClear.Text = "Clear";
            this.toolTip.SetToolTip(this.btnClear, "Delete all steps");
            this.btnClear.UseVisualStyleBackColor = true;
            this.btnClear.Click += new System.EventHandler(this.btnClear_Click);
            //
            // btnLoad
            //
            this.btnLoad.Location = new System.Drawing.Point(12, 167);
            this.btnLoad.Name = "btnLoad";
            this.btnLoad.Size = new System.Drawing.Size(125, 24);
            this.btnLoad.TabIndex = 6;
            this.btnLoad.Text = "Load...";
            this.toolTip.SetToolTip(this.btnLoad, "Open a macro saved in a file (replaces the current one)");
            this.btnLoad.UseVisualStyleBackColor = true;
            this.btnLoad.Click += new System.EventHandler(this.btnLoad_Click);
            //
            // btnSave
            //
            this.btnSave.Enabled = false;
            this.btnSave.Location = new System.Drawing.Point(147, 167);
            this.btnSave.Name = "btnSave";
            this.btnSave.Size = new System.Drawing.Size(125, 24);
            this.btnSave.TabIndex = 7;
            this.btnSave.Text = "Save...";
            this.toolTip.SetToolTip(this.btnSave, "Save this macro to a file");
            this.btnSave.UseVisualStyleBackColor = true;
            this.btnSave.Click += new System.EventHandler(this.btnSave_Click);
            //
            // grpPlayback
            //
            this.grpPlayback.Controls.Add(this.lblSpeedX);
            this.grpPlayback.Controls.Add(this.numSpeed);
            this.grpPlayback.Controls.Add(this.lblSpeed);
            this.grpPlayback.Controls.Add(this.lblPauseMs);
            this.grpPlayback.Controls.Add(this.numPause);
            this.grpPlayback.Controls.Add(this.lblPause);
            this.grpPlayback.Controls.Add(this.chkForever);
            this.grpPlayback.Controls.Add(this.lblTimes);
            this.grpPlayback.Controls.Add(this.numRepeat);
            this.grpPlayback.Controls.Add(this.lblRepeat);
            this.grpPlayback.Location = new System.Drawing.Point(12, 197);
            this.grpPlayback.Name = "grpPlayback";
            this.grpPlayback.Size = new System.Drawing.Size(260, 96);
            this.grpPlayback.TabIndex = 8;
            this.grpPlayback.TabStop = false;
            this.grpPlayback.Text = "Playback";
            //
            // lblSpeedX
            //
            this.lblSpeedX.AutoSize = true;
            this.lblSpeedX.Location = new System.Drawing.Point(197, 71);
            this.lblSpeedX.Name = "lblSpeedX";
            this.lblSpeedX.Size = new System.Drawing.Size(12, 13);
            this.lblSpeedX.TabIndex = 9;
            this.lblSpeedX.Text = "x";
            //
            // numSpeed
            //
            this.numSpeed.DecimalPlaces = 1;
            this.numSpeed.Increment = new decimal(new int[] {
            1,
            0,
            0,
            65536});
            this.numSpeed.Location = new System.Drawing.Point(118, 69);
            this.numSpeed.Maximum = new decimal(new int[] {
            10,
            0,
            0,
            0});
            this.numSpeed.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            65536});
            this.numSpeed.Name = "numSpeed";
            this.numSpeed.Size = new System.Drawing.Size(75, 20);
            this.numSpeed.TabIndex = 8;
            this.toolTip.SetToolTip(this.numSpeed, "2 = twice as fast, 0.5 = half speed (changes the waits between steps)");
            this.numSpeed.Value = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.numSpeed.ValueChanged += new System.EventHandler(this.numSpeed_ValueChanged);
            //
            // lblSpeed
            //
            this.lblSpeed.AutoSize = true;
            this.lblSpeed.Location = new System.Drawing.Point(12, 71);
            this.lblSpeed.Name = "lblSpeed";
            this.lblSpeed.Size = new System.Drawing.Size(38, 13);
            this.lblSpeed.TabIndex = 7;
            this.lblSpeed.Text = "Speed";
            //
            // lblPauseMs
            //
            this.lblPauseMs.AutoSize = true;
            this.lblPauseMs.Location = new System.Drawing.Point(197, 46);
            this.lblPauseMs.Name = "lblPauseMs";
            this.lblPauseMs.Size = new System.Drawing.Size(20, 13);
            this.lblPauseMs.TabIndex = 6;
            this.lblPauseMs.Text = "ms";
            //
            // numPause
            //
            this.numPause.Increment = new decimal(new int[] {
            100,
            0,
            0,
            0});
            this.numPause.Location = new System.Drawing.Point(118, 44);
            this.numPause.Maximum = new decimal(new int[] {
            3600000,
            0,
            0,
            0});
            this.numPause.Name = "numPause";
            this.numPause.Size = new System.Drawing.Size(75, 20);
            this.numPause.TabIndex = 5;
            this.numPause.ThousandsSeparator = true;
            this.toolTip.SetToolTip(this.numPause, "Wait after each run, before the next one starts");
            this.numPause.ValueChanged += new System.EventHandler(this.numPause_ValueChanged);
            //
            // lblPause
            //
            this.lblPause.AutoSize = true;
            this.lblPause.Location = new System.Drawing.Point(12, 46);
            this.lblPause.Name = "lblPause";
            this.lblPause.Size = new System.Drawing.Size(101, 13);
            this.lblPause.TabIndex = 4;
            this.lblPause.Text = "Pause between runs";
            //
            // chkForever
            //
            this.chkForever.AutoSize = true;
            this.chkForever.Location = new System.Drawing.Point(185, 20);
            this.chkForever.Name = "chkForever";
            this.chkForever.Size = new System.Drawing.Size(62, 17);
            this.chkForever.TabIndex = 3;
            this.chkForever.Text = "Forever";
            this.toolTip.SetToolTip(this.chkForever, "Repeat until stopped");
            this.chkForever.UseVisualStyleBackColor = true;
            this.chkForever.CheckedChanged += new System.EventHandler(this.chkForever_CheckedChanged);
            //
            // lblTimes
            //
            this.lblTimes.AutoSize = true;
            this.lblTimes.Location = new System.Drawing.Point(139, 21);
            this.lblTimes.Name = "lblTimes";
            this.lblTimes.Size = new System.Drawing.Size(31, 13);
            this.lblTimes.TabIndex = 2;
            this.lblTimes.Text = "times";
            //
            // numRepeat
            //
            this.numRepeat.Location = new System.Drawing.Point(60, 19);
            this.numRepeat.Maximum = new decimal(new int[] {
            1000000,
            0,
            0,
            0});
            this.numRepeat.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.numRepeat.Name = "numRepeat";
            this.numRepeat.Size = new System.Drawing.Size(75, 20);
            this.numRepeat.TabIndex = 1;
            this.numRepeat.ThousandsSeparator = true;
            this.numRepeat.Value = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.numRepeat.ValueChanged += new System.EventHandler(this.numRepeat_ValueChanged);
            //
            // lblRepeat
            //
            this.lblRepeat.AutoSize = true;
            this.lblRepeat.Location = new System.Drawing.Point(12, 21);
            this.lblRepeat.Name = "lblRepeat";
            this.lblRepeat.Size = new System.Drawing.Size(42, 13);
            this.lblRepeat.TabIndex = 0;
            this.lblRepeat.Text = "Repeat";
            //
            // grpRecording
            //
            this.grpRecording.Controls.Add(this.chkRecordMoves);
            this.grpRecording.Controls.Add(this.chkAppend);
            this.grpRecording.Location = new System.Drawing.Point(12, 299);
            this.grpRecording.Name = "grpRecording";
            this.grpRecording.Size = new System.Drawing.Size(260, 66);
            this.grpRecording.TabIndex = 9;
            this.grpRecording.TabStop = false;
            this.grpRecording.Text = "Recording";
            //
            // chkRecordMoves
            //
            this.chkRecordMoves.AutoSize = true;
            this.chkRecordMoves.Location = new System.Drawing.Point(12, 42);
            this.chkRecordMoves.Name = "chkRecordMoves";
            this.chkRecordMoves.Size = new System.Drawing.Size(178, 17);
            this.chkRecordMoves.TabIndex = 1;
            this.chkRecordMoves.Text = "Record mouse moves too";
            this.toolTip.SetToolTip(this.chkRecordMoves, "Replays the path of the mouse, not only where it clicked (makes many more steps)");
            this.chkRecordMoves.UseVisualStyleBackColor = true;
            //
            // chkAppend
            //
            this.chkAppend.AutoSize = true;
            this.chkAppend.Location = new System.Drawing.Point(12, 20);
            this.chkAppend.Name = "chkAppend";
            this.chkAppend.Size = new System.Drawing.Size(196, 17);
            this.chkAppend.TabIndex = 0;
            this.chkAppend.Text = "Add to the end of the current macro";
            this.toolTip.SetToolTip(this.chkAppend, "Unticked: a new recording replaces the macro");
            this.chkAppend.UseVisualStyleBackColor = true;
            //
            // grpHotkeys
            //
            this.grpHotkeys.Controls.Add(this.hkPlay);
            this.grpHotkeys.Controls.Add(this.lblPlayHotkey);
            this.grpHotkeys.Controls.Add(this.hkRecord);
            this.grpHotkeys.Controls.Add(this.lblRecordHotkey);
            this.grpHotkeys.Location = new System.Drawing.Point(12, 371);
            this.grpHotkeys.Name = "grpHotkeys";
            this.grpHotkeys.Size = new System.Drawing.Size(260, 50);
            this.grpHotkeys.TabIndex = 10;
            this.grpHotkeys.TabStop = false;
            this.grpHotkeys.Text = "Change hotkeys (click a key)";
            //
            // hkPlay
            //
            this.hkPlay.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.hkPlay.HotkeyId = 3;
            this.hkPlay.Location = new System.Drawing.Point(176, 17);
            this.hkPlay.Name = "hkPlay";
            this.hkPlay.Size = new System.Drawing.Size(76, 25);
            this.hkPlay.TabIndex = 3;
            this.hkPlay.Text = "F7";
            this.toolTip.SetToolTip(this.hkPlay, "Click, then press the new hotkey (Esc cancels)");
            this.hkPlay.UseVisualStyleBackColor = true;
            this.hkPlay.HotkeyPressed += new System.EventHandler(this.hkPlay_HotkeyPressed);
            this.hkPlay.HotkeyChanged += new System.EventHandler(this.hotkey_HotkeyChanged);
            //
            // lblPlayHotkey
            //
            this.lblPlayHotkey.AutoSize = true;
            this.lblPlayHotkey.Location = new System.Drawing.Point(146, 23);
            this.lblPlayHotkey.Name = "lblPlayHotkey";
            this.lblPlayHotkey.Size = new System.Drawing.Size(27, 13);
            this.lblPlayHotkey.TabIndex = 2;
            this.lblPlayHotkey.Text = "Play";
            //
            // hkRecord
            //
            this.hkRecord.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.hkRecord.HotkeyId = 2;
            this.hkRecord.Location = new System.Drawing.Point(58, 17);
            this.hkRecord.Name = "hkRecord";
            this.hkRecord.Size = new System.Drawing.Size(80, 25);
            this.hkRecord.TabIndex = 1;
            this.hkRecord.Text = "F8";
            this.toolTip.SetToolTip(this.hkRecord, "Click, then press the new hotkey (Esc cancels)");
            this.hkRecord.UseVisualStyleBackColor = true;
            this.hkRecord.HotkeyPressed += new System.EventHandler(this.hkRecord_HotkeyPressed);
            this.hkRecord.HotkeyChanged += new System.EventHandler(this.hotkey_HotkeyChanged);
            //
            // lblRecordHotkey
            //
            this.lblRecordHotkey.AutoSize = true;
            this.lblRecordHotkey.Location = new System.Drawing.Point(12, 23);
            this.lblRecordHotkey.Name = "lblRecordHotkey";
            this.lblRecordHotkey.Size = new System.Drawing.Size(42, 13);
            this.lblRecordHotkey.TabIndex = 0;
            this.lblRecordHotkey.Text = "Record";
            //
            // btnRecord
            //
            this.btnRecord.Location = new System.Drawing.Point(12, 427);
            this.btnRecord.Name = "btnRecord";
            this.btnRecord.Size = new System.Drawing.Size(125, 30);
            this.btnRecord.TabIndex = 11;
            this.btnRecord.Text = "Record (F8)";
            this.btnRecord.UseVisualStyleBackColor = true;
            this.btnRecord.Click += new System.EventHandler(this.btnRecord_Click);
            //
            // btnPlay
            //
            this.btnPlay.Enabled = false;
            this.btnPlay.Location = new System.Drawing.Point(147, 427);
            this.btnPlay.Name = "btnPlay";
            this.btnPlay.Size = new System.Drawing.Size(125, 30);
            this.btnPlay.TabIndex = 12;
            this.btnPlay.Text = "Play (F7)";
            this.btnPlay.UseVisualStyleBackColor = true;
            this.btnPlay.Click += new System.EventHandler(this.btnPlay_Click);
            //
            // lblStatus
            //
            this.lblStatus.AutoSize = true;
            this.lblStatus.Location = new System.Drawing.Point(12, 461);
            this.lblStatus.Name = "lblStatus";
            this.lblStatus.Size = new System.Drawing.Size(67, 13);
            this.lblStatus.TabIndex = 13;
            this.lblStatus.Text = "No steps yet";
            //
            // playTimer
            //
            this.playTimer.Interval = 1;
            this.playTimer.Tick += new System.EventHandler(this.playTimer_Tick);
            //
            // openFileDialog
            //
            this.openFileDialog.DefaultExt = "macro";
            this.openFileDialog.Filter = "MaxClicks macro (*.macro)|*.macro|All files (*.*)|*.*";
            this.openFileDialog.Title = "Load macro";
            //
            // saveFileDialog
            //
            this.saveFileDialog.DefaultExt = "macro";
            this.saveFileDialog.Filter = "MaxClicks macro (*.macro)|*.macro|All files (*.*)|*.*";
            this.saveFileDialog.Title = "Save macro";
            //
            // MacroView
            //
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.lblStatus);
            this.Controls.Add(this.btnPlay);
            this.Controls.Add(this.btnRecord);
            this.Controls.Add(this.grpHotkeys);
            this.Controls.Add(this.grpRecording);
            this.Controls.Add(this.grpPlayback);
            this.Controls.Add(this.btnSave);
            this.Controls.Add(this.btnLoad);
            this.Controls.Add(this.btnClear);
            this.Controls.Add(this.btnDeleteStep);
            this.Controls.Add(this.lblWaitMs);
            this.Controls.Add(this.numStepDelay);
            this.Controls.Add(this.lblWait);
            this.Controls.Add(this.lvSteps);
            this.Name = "MacroView";
            this.Size = new System.Drawing.Size(284, 480);
            ((System.ComponentModel.ISupportInitialize)(this.numStepDelay)).EndInit();
            this.grpPlayback.ResumeLayout(false);
            this.grpPlayback.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numSpeed)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numPause)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numRepeat)).EndInit();
            this.grpRecording.ResumeLayout(false);
            this.grpRecording.PerformLayout();
            this.grpHotkeys.ResumeLayout(false);
            this.grpHotkeys.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.ListView lvSteps;
        private System.Windows.Forms.ColumnHeader colNumber;
        private System.Windows.Forms.ColumnHeader colWait;
        private System.Windows.Forms.ColumnHeader colAction;
        private System.Windows.Forms.Label lblWait;
        private System.Windows.Forms.NumericUpDown numStepDelay;
        private System.Windows.Forms.Label lblWaitMs;
        private System.Windows.Forms.Button btnDeleteStep;
        private System.Windows.Forms.Button btnClear;
        private System.Windows.Forms.Button btnLoad;
        private System.Windows.Forms.Button btnSave;
        private System.Windows.Forms.GroupBox grpPlayback;
        private System.Windows.Forms.Label lblRepeat;
        private System.Windows.Forms.NumericUpDown numRepeat;
        private System.Windows.Forms.Label lblTimes;
        private System.Windows.Forms.CheckBox chkForever;
        private System.Windows.Forms.Label lblPause;
        private System.Windows.Forms.NumericUpDown numPause;
        private System.Windows.Forms.Label lblPauseMs;
        private System.Windows.Forms.Label lblSpeed;
        private System.Windows.Forms.NumericUpDown numSpeed;
        private System.Windows.Forms.Label lblSpeedX;
        private System.Windows.Forms.GroupBox grpRecording;
        private System.Windows.Forms.CheckBox chkAppend;
        private System.Windows.Forms.CheckBox chkRecordMoves;
        private System.Windows.Forms.GroupBox grpHotkeys;
        private System.Windows.Forms.Label lblRecordHotkey;
        private HotkeyButton hkRecord;
        private System.Windows.Forms.Label lblPlayHotkey;
        private HotkeyButton hkPlay;
        private System.Windows.Forms.Button btnRecord;
        private System.Windows.Forms.Button btnPlay;
        private System.Windows.Forms.Label lblStatus;
        private System.Windows.Forms.Timer playTimer;
        private System.Windows.Forms.ToolTip toolTip;
        private System.Windows.Forms.OpenFileDialog openFileDialog;
        private System.Windows.Forms.SaveFileDialog saveFileDialog;
    }
}
