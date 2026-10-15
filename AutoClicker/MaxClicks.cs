using System;
using System.Windows.Forms;

namespace AutoClicker
{
    public partial class MaxClicks : Form
    {
        // Quantos cliques foram feitos desde o último "Iniciar"
        int contagem = 0;

        public MaxClicks()
        {
            InitializeComponent();
        }

        private void btnIniciar_Click(object sender, EventArgs e)
        {
            contagem = 0;
            AtualizarContagem();

            // O Timer dispara o evento Tick a cada "Interval" milissegundos
            timerCliques.Interval = (int)numIntervalo.Value;
            timerCliques.Start();

            AtualizarBotoes(rodando: true);
        }

        private void btnParar_Click(object sender, EventArgs e)
        {
            timerCliques.Stop();
            AtualizarBotoes(rodando: false);
        }

        // Chamado automaticamente pelo Timer: cada Tick é um clique
        private void timerCliques_Tick(object sender, EventArgs e)
        {
            Mouse.LeftClick();
            contagem++;
            AtualizarContagem();
        }

        private void AtualizarContagem()
        {
            lblContagem.Text = "Cliques: " + contagem;
        }

        // Enquanto está clicando, só o "Parar" fica habilitado (e não dá pra mudar o intervalo)
        private void AtualizarBotoes(bool rodando)
        {
            btnIniciar.Enabled = !rodando;
            btnParar.Enabled = rodando;
            numIntervalo.Enabled = !rodando;
        }
    }
}
