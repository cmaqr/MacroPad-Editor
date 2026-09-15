using System.Drawing;
using System.Windows.Forms;

namespace RSoft.MacroPad.Controls
{
    /// <summary>
    /// Painel com cantos arredondados, usado como "cartão" em volta de cada bloco da tela.
    /// </summary>
    internal class RoundedPanel : Panel
    {
        private SurfaceRole _surface = SurfaceRole.Card;

        public int Radius { get; set; } = 16;

        /// <summary>Qual fundo do tema o cartão usa; muda junto quando o tema troca.</summary>
        public SurfaceRole Surface
        {
            get => _surface;
            set { _surface = value; Theme.Register(this, TextRole.Primary, value); }
        }

        public RoundedPanel()
        {
            DoubleBuffered = true;
            ResizeRedraw = true;
            Theme.Register(this, TextRole.Primary, _surface);
        }

        protected override void OnPaintBackground(PaintEventArgs e)
        {
            // Os cantos de fora do arredondado mostram a cor de quem está atrás do cartão
            e.Graphics.Clear(Parent?.BackColor ?? Theme.Background);
            Theme.PrepareGraphics(e.Graphics);
            Theme.FillRounded(e.Graphics, BackColor, new RectangleF(0, 0, Width, Height), Theme.Scale(this, (float)Radius));
        }
    }
}
