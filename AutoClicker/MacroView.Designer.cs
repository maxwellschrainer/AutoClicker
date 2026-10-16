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
            this.lblComingSoon = new System.Windows.Forms.Label();
            this.SuspendLayout();
            //
            // lblComingSoon
            //
            this.lblComingSoon.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblComingSoon.ForeColor = System.Drawing.SystemColors.GrayText;
            this.lblComingSoon.Location = new System.Drawing.Point(0, 0);
            this.lblComingSoon.Name = "lblComingSoon";
            this.lblComingSoon.Size = new System.Drawing.Size(284, 273);
            this.lblComingSoon.TabIndex = 0;
            this.lblComingSoon.Text = "Macro - coming soon";
            this.lblComingSoon.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            //
            // MacroView
            //
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.lblComingSoon);
            this.Name = "MacroView";
            this.Size = new System.Drawing.Size(284, 273);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Label lblComingSoon;
    }
}
