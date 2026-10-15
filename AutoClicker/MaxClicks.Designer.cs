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
            this.Header = new System.Windows.Forms.MenuStrip();
            this.programToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.exitToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.clickerToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.macroToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.helpToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.Header.SuspendLayout();
            this.SuspendLayout();
            // 
            // Header
            // 
            this.Header.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.programToolStripMenuItem,
            this.clickerToolStripMenuItem,
            this.macroToolStripMenuItem,
            this.helpToolStripMenuItem});
            this.Header.Location = new System.Drawing.Point(0, 0);
            this.Header.Name = "Header";
            this.Header.Size = new System.Drawing.Size(602, 24);
            this.Header.TabIndex = 2;
            this.Header.Text = "menuStrip1";
            // 
            // programToolStripMenuItem
            // 
            this.programToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.exitToolStripMenuItem});
            this.programToolStripMenuItem.Name = "programToolStripMenuItem";
            this.programToolStripMenuItem.Size = new System.Drawing.Size(65, 20);
            this.programToolStripMenuItem.Text = "Program";
            // 
            // exitToolStripMenuItem
            // 
            this.exitToolStripMenuItem.Name = "exitToolStripMenuItem";
            this.exitToolStripMenuItem.Size = new System.Drawing.Size(92, 22);
            this.exitToolStripMenuItem.Text = "Exit";
            this.exitToolStripMenuItem.Click += new System.EventHandler(this.exitToolStripMenuItem_Click);
            // 
            // clickerToolStripMenuItem
            // 
            this.clickerToolStripMenuItem.Name = "clickerToolStripMenuItem";
            this.clickerToolStripMenuItem.Size = new System.Drawing.Size(55, 20);
            this.clickerToolStripMenuItem.Text = "Clicker";
            // 
            // macroToolStripMenuItem
            // 
            this.macroToolStripMenuItem.Name = "macroToolStripMenuItem";
            this.macroToolStripMenuItem.Size = new System.Drawing.Size(53, 20);
            this.macroToolStripMenuItem.Text = "Macro";
            // 
            // helpToolStripMenuItem
            // 
            this.helpToolStripMenuItem.Name = "helpToolStripMenuItem";
            this.helpToolStripMenuItem.Size = new System.Drawing.Size(44, 20);
            this.helpToolStripMenuItem.Text = "Help";
            // 
            // MaxClicks
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(602, 450);
            this.Controls.Add(this.Header);
            this.MainMenuStrip = this.Header;
            this.Name = "MaxClicks";
            this.Text = "MaxClicks";
            this.Header.ResumeLayout(false);
            this.Header.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private System.Windows.Forms.MenuStrip Header;
        private System.Windows.Forms.ToolStripMenuItem programToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem exitToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem clickerToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem macroToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem helpToolStripMenuItem;
    }
}

