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
            this.pnlHeader = new System.Windows.Forms.Panel();
            this.lblTitle = new System.Windows.Forms.Label();
            this.btnSwitchView = new System.Windows.Forms.Button();
            this.pnlViews = new System.Windows.Forms.Panel();
            this.clickerView = new AutoClicker.ClickerView();
            this.macroView = new AutoClicker.MacroView();
            this.slideTimer = new System.Windows.Forms.Timer(this.components);
            this.pnlHeader.SuspendLayout();
            this.pnlViews.SuspendLayout();
            this.SuspendLayout();
            //
            // pnlHeader
            //
            this.pnlHeader.BackColor = System.Drawing.SystemColors.ControlLight;
            this.pnlHeader.Controls.Add(this.lblTitle);
            this.pnlHeader.Controls.Add(this.btnSwitchView);
            this.pnlHeader.Location = new System.Drawing.Point(0, 0);
            this.pnlHeader.Name = "pnlHeader";
            this.pnlHeader.Size = new System.Drawing.Size(284, 40);
            this.pnlHeader.TabIndex = 0;
            //
            // lblTitle
            //
            this.lblTitle.AutoSize = true;
            this.lblTitle.Font = new System.Drawing.Font("Segoe UI", 11F, System.Drawing.FontStyle.Bold);
            this.lblTitle.Location = new System.Drawing.Point(10, 10);
            this.lblTitle.Name = "lblTitle";
            this.lblTitle.Size = new System.Drawing.Size(92, 20);
            this.lblTitle.TabIndex = 0;
            this.lblTitle.Text = "Auto Clicker";
            //
            // btnSwitchView
            //
            this.btnSwitchView.Location = new System.Drawing.Point(172, 7);
            this.btnSwitchView.Name = "btnSwitchView";
            this.btnSwitchView.Size = new System.Drawing.Size(100, 27);
            this.btnSwitchView.TabIndex = 1;
            this.btnSwitchView.Text = "Macro";
            this.btnSwitchView.UseVisualStyleBackColor = true;
            this.btnSwitchView.Click += new System.EventHandler(this.btnSwitchView_Click);
            //
            // pnlViews
            //
            this.pnlViews.Controls.Add(this.clickerView);
            this.pnlViews.Controls.Add(this.macroView);
            this.pnlViews.Location = new System.Drawing.Point(0, 40);
            this.pnlViews.Name = "pnlViews";
            this.pnlViews.Size = new System.Drawing.Size(284, 334);
            this.pnlViews.TabIndex = 1;
            //
            // clickerView
            //
            this.clickerView.Location = new System.Drawing.Point(0, 0);
            this.clickerView.Name = "clickerView";
            this.clickerView.Size = new System.Drawing.Size(284, 334);
            this.clickerView.TabIndex = 0;
            //
            // macroView
            //
            this.macroView.Location = new System.Drawing.Point(284, 0);
            this.macroView.Name = "macroView";
            this.macroView.Size = new System.Drawing.Size(284, 334);
            this.macroView.TabIndex = 1;
            this.macroView.Visible = false;
            //
            // slideTimer
            //
            this.slideTimer.Interval = 15;
            this.slideTimer.Tick += new System.EventHandler(this.slideTimer_Tick);
            //
            // MaxClicks
            //
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(284, 374);
            this.Controls.Add(this.pnlViews);
            this.Controls.Add(this.pnlHeader);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.Name = "MaxClicks";
            this.Text = "MaxClicks";
            this.TopMost = true;
            this.pnlHeader.ResumeLayout(false);
            this.pnlHeader.PerformLayout();
            this.pnlViews.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion
        private System.Windows.Forms.Panel pnlHeader;
        private System.Windows.Forms.Label lblTitle;
        private System.Windows.Forms.Button btnSwitchView;
        private System.Windows.Forms.Panel pnlViews;
        private ClickerView clickerView;
        private MacroView macroView;
        private System.Windows.Forms.Timer slideTimer;
    }
}
