using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using System.Windows.Forms;

namespace AutoClicker
{
    public partial class MaxClicks : Form
    {
        int contagem = 0;

        public MaxClicks()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            label1.Text = "Contagem: " + contagem.ToString();
        }

        private void timer1_Tick(object sender, EventArgs e)
        {

            contagem++;
        }
    }
}
