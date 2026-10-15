namespace AutoClicker
{
    partial class MaxClicks
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

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this.grpInterval = new System.Windows.Forms.GroupBox();
            this.lblMs = new System.Windows.Forms.Label();
            this.numInterval = new System.Windows.Forms.NumericUpDown();
            this.btnStart = new System.Windows.Forms.Button();
            this.btnStop = new System.Windows.Forms.Button();
            this.lblCount = new System.Windows.Forms.Label();
            this.clickTimer = new System.Windows.Forms.Timer(this.components);
            this.grpInterval.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numInterval)).BeginInit();
            this.SuspendLayout();
            // 
            // grpInterval
            // 
            this.grpInterval.Controls.Add(this.lblMs);
            this.grpInterval.Controls.Add(this.numInterval);
            this.grpInterval.Location = new System.Drawing.Point(12, 12);
            this.grpInterval.Name = "grpInterval";
            this.grpInterval.Size = new System.Drawing.Size(260, 55);
            this.grpInterval.TabIndex = 0;
            this.grpInterval.TabStop = false;
            this.grpInterval.Text = "Interval between clicks";
            // 
            // lblMs
            // 
            this.lblMs.AutoSize = true;
            this.lblMs.Location = new System.Drawing.Point(142, 24);
            this.lblMs.Name = "lblMs";
            this.lblMs.Size = new System.Drawing.Size(63, 13);
            this.lblMs.TabIndex = 1;
            this.lblMs.Text = "milliseconds";
            // 
            // numInterval
            // 
            this.numInterval.Location = new System.Drawing.Point(15, 22);
            this.numInterval.Maximum = new decimal(new int[] {
            60000,
            0,
            0,
            0});
            this.numInterval.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.numInterval.Name = "numInterval";
            this.numInterval.Size = new System.Drawing.Size(120, 20);
            this.numInterval.TabIndex = 0;
            this.numInterval.Value = new decimal(new int[] {
            100,
            0,
            0,
            0});
            // 
            // btnStart
            // 
            this.btnStart.Location = new System.Drawing.Point(12, 78);
            this.btnStart.Name = "btnStart";
            this.btnStart.Size = new System.Drawing.Size(125, 35);
            this.btnStart.TabIndex = 1;
            this.btnStart.Text = "Start";
            this.btnStart.UseVisualStyleBackColor = true;
            this.btnStart.Click += new System.EventHandler(this.btnStart_Click);
            // 
            // btnStop
            // 
            this.btnStop.Enabled = false;
            this.btnStop.Location = new System.Drawing.Point(147, 78);
            this.btnStop.Name = "btnStop";
            this.btnStop.Size = new System.Drawing.Size(125, 35);
            this.btnStop.TabIndex = 2;
            this.btnStop.Text = "Stop";
            this.btnStop.UseVisualStyleBackColor = true;
            this.btnStop.Click += new System.EventHandler(this.btnStop_Click);
            // 
            // lblCount
            // 
            this.lblCount.AutoSize = true;
            this.lblCount.Location = new System.Drawing.Point(12, 125);
            this.lblCount.Name = "lblCount";
            this.lblCount.Size = new System.Drawing.Size(47, 13);
            this.lblCount.TabIndex = 3;
            this.lblCount.Text = "Clicks: 0";
            // 
            // clickTimer
            // 
            this.clickTimer.Tick += new System.EventHandler(this.clickTimer_Tick);
            // 
            // MaxClicks
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(284, 151);
            this.Controls.Add(this.lblCount);
            this.Controls.Add(this.btnStop);
            this.Controls.Add(this.btnStart);
            this.Controls.Add(this.grpInterval);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.Name = "MaxClicks";
            this.Text = "MaxClicks";
            this.TopMost = true;
            this.grpInterval.ResumeLayout(false);
            this.grpInterval.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numInterval)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private System.Windows.Forms.GroupBox grpInterval;
        private System.Windows.Forms.Label lblMs;
        private System.Windows.Forms.NumericUpDown numInterval;
        private System.Windows.Forms.Button btnStart;
        private System.Windows.Forms.Button btnStop;
        private System.Windows.Forms.Label lblCount;
        private System.Windows.Forms.Timer clickTimer;
    }
}