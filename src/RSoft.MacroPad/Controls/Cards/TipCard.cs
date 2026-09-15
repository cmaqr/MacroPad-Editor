using System;
using System.Drawing;
using System.Windows.Forms;
using RSoft.MacroPad.BLL.Macros;

namespace RSoft.MacroPad.Controls.Cards
{
    /// <summary>
    /// Cartão com uma dica por vez e o botão para ver a próxima.
    /// </summary>
    internal class TipCard : RoundedPanel
    {
        private readonly Label _icon;
        private readonly Label _heading;
        private readonly Label _tip;
        private readonly PillButton _nextButton;
        private int _tipIndex;

        public TipCard()
        {
            Surface = SurfaceRole.AccentSoft;
            // Começa numa dica diferente a cada vez que o app abre
            _tipIndex = new Random().Next(MacroCatalog.Tips.Count);

            _icon = Theme.Register(new Label { Text = "\uE946", Font = Theme.IconSmall, TextAlign = ContentAlignment.MiddleLeft }, TextRole.Accent, SurfaceRole.AccentSoft);
            _heading = Theme.Register(new Label { Text = "Dica", Font = Theme.CalloutSemibold, TextAlign = ContentAlignment.MiddleLeft }, TextRole.Accent, SurfaceRole.AccentSoft);
            _tip = Theme.Register(new Label { Font = Theme.Callout, UseMnemonic = false, AutoEllipsis = true }, TextRole.Primary, SurfaceRole.AccentSoft);
            _nextButton = new PillButton { Text = "Próxima dica  ›", Style = PillStyle.Plain, Font = Theme.CalloutSemibold, Height = Theme.Scale(this, 24) };
            _nextButton.FitWidthToText();
            _nextButton.Click += (s, e) => ShowTip(_tipIndex + 1);

            Controls.AddRange(new Control[] { _icon, _heading, _tip, _nextButton });
            ShowTip(_tipIndex);
        }

        protected override void OnLayout(LayoutEventArgs levent)
        {
            base.OnLayout(levent);
            if (_nextButton == null)
                return;

            var padding = Theme.Scale(this, 16);
            var width = ClientSize.Width - padding * 2;
            _icon.SetBounds(padding, padding - Theme.Scale(this, 2), Theme.Scale(this, 22), Theme.Scale(this, 22));
            _heading.SetBounds(_icon.Right, padding - Theme.Scale(this, 2), Theme.Scale(this, 120), Theme.Scale(this, 22));
            _nextButton.Location = new Point(ClientSize.Width - padding - _nextButton.Width + Theme.Scale(this, 4), padding - Theme.Scale(this, 3));
            _tip.SetBounds(padding, _heading.Bottom + Theme.Scale(this, 4), width, Math.Max(0, ClientSize.Height - _heading.Bottom - Theme.Scale(this, 4) - padding));
        }

        private void ShowTip(int index)
        {
            _tipIndex = index % MacroCatalog.Tips.Count;
            _tip.Text = MacroCatalog.Tips[_tipIndex];
        }
    }
}
