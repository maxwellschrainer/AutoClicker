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
            this.grpIntervalo = new System.Windows.Forms.GroupBox();
            this.lblMs = new System.Windows.Forms.Label();
            this.numIntervalo = new System.Windows.Forms.NumericUpDown();
            this.btnIniciar = new System.Windows.Forms.Button();
            this.btnParar = new System.Windows.Forms.Button();
            this.lblContagem = new System.Windows.Forms.Label();
            this.timerCliques = new System.Windows.Forms.Timer(this.components);
            this.grpIntervalo.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numIntervalo)).BeginInit();
            this.SuspendLayout();
            //
            // grpIntervalo
            //
            this.grpIntervalo.Controls.Add(this.lblMs);
            this.grpIntervalo.Controls.Add(this.numIntervalo);
            this.grpIntervalo.Location = new System.Drawing.Point(12, 12);
            this.grpIntervalo.Name = "grpIntervalo";
            this.grpIntervalo.Size = new System.Drawing.Size(260, 55);
            this.grpIntervalo.TabIndex = 0;
            this.grpIntervalo.TabStop = false;
            this.grpIntervalo.Text = "Intervalo entre cliques";
            //
            // lblMs
            //
            this.lblMs.AutoSize = true;
            this.lblMs.Location = new System.Drawing.Point(142, 24);
            this.lblMs.Name = "lblMs";
            this.lblMs.Size = new System.Drawing.Size(84, 13);
            this.lblMs.TabIndex = 1;
            this.lblMs.Text = "milissegundos";
            //
            // numIntervalo
            //
            this.numIntervalo.Location = new System.Drawing.Point(15, 22);
            this.numIntervalo.Maximum = new decimal(new int[] {
            60000,
            0,
            0,
            0});
            this.numIntervalo.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.numIntervalo.Name = "numIntervalo";
            this.numIntervalo.Size = new System.Drawing.Size(120, 20);
            this.numIntervalo.TabIndex = 0;
            this.numIntervalo.Value = new decimal(new int[] {
            100,
            0,
            0,
            0});
            //
            // btnIniciar
            //
            this.btnIniciar.Location = new System.Drawing.Point(12, 78);
            this.btnIniciar.Name = "btnIniciar";
            this.btnIniciar.Size = new System.Drawing.Size(125, 35);
            this.btnIniciar.TabIndex = 1;
            this.btnIniciar.Text = "Iniciar";
            this.btnIniciar.UseVisualStyleBackColor = true;
            this.btnIniciar.Click += new System.EventHandler(this.btnIniciar_Click);
            //
            // btnParar
            //
            this.btnParar.Enabled = false;
            this.btnParar.Location = new System.Drawing.Point(147, 78);
            this.btnParar.Name = "btnParar";
            this.btnParar.Size = new System.Drawing.Size(125, 35);
            this.btnParar.TabIndex = 2;
            this.btnParar.Text = "Parar";
            this.btnParar.UseVisualStyleBackColor = true;
            this.btnParar.Click += new System.EventHandler(this.btnParar_Click);
            //
            // lblContagem
            //
            this.lblContagem.AutoSize = true;
            this.lblContagem.Location = new System.Drawing.Point(12, 125);
            this.lblContagem.Name = "lblContagem";
            this.lblContagem.Size = new System.Drawing.Size(54, 13);
            this.lblContagem.TabIndex = 3;
            this.lblContagem.Text = "Cliques: 0";
            //
            // timerCliques
            //
            this.timerCliques.Tick += new System.EventHandler(this.timerCliques_Tick);
            //
            // MaxClicks
            //
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(284, 151);
            this.Controls.Add(this.lblContagem);
            this.Controls.Add(this.btnParar);
            this.Controls.Add(this.btnIniciar);
            this.Controls.Add(this.grpIntervalo);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.Name = "MaxClicks";
            this.Text = "MaxClicks";
            this.TopMost = true;
            this.grpIntervalo.ResumeLayout(false);
            this.grpIntervalo.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numIntervalo)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private System.Windows.Forms.GroupBox grpIntervalo;
        private System.Windows.Forms.Label lblMs;
        private System.Windows.Forms.NumericUpDown numIntervalo;
        private System.Windows.Forms.Button btnIniciar;
        private System.Windows.Forms.Button btnParar;
        private System.Windows.Forms.Label lblContagem;
        private System.Windows.Forms.Timer timerCliques;
    }
}