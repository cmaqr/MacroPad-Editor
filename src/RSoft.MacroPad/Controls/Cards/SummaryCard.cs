using System;
using System.Drawing;
using System.Windows.Forms;
using RSoft.MacroPad.BLL.Macros;

namespace RSoft.MacroPad.Controls.Cards
{
    /// <summary>
    /// Cartão que mostra "tecla escolhida → ação escolhida" e o botão de enviar.
    /// </summary>
    internal class SummaryCard : RoundedPanel
    {
        private readonly Label _target;
        private readonly Label _macroTitle;
        private readonly Label _shortcut;
        private readonly Label _delayLabel;
        private readonly NumericUpDown _delay;
        private readonly PillButton _sendButton;
        private readonly PillButton _testButton;
        private readonly PillButton _cheatSheetButton;
        private readonly Label _status;

        public event EventHandler SendRequested;
        /// <summary>Executar a macro no próprio PC, para ver o que ela faz antes de gravar.</summary>
        public event EventHandler TestRequested;
        public event EventHandler CheatSheetRequested;

        /// <summary>Atraso em milissegundos que o macropad espera entre as teclas da sequência.</summary>
        public ushort Delay => (ushort)_delay.Value;

        public SummaryCard()
        {
            _target = NewLabel("", Theme.CaptionSemibold, TextRole.Secondary);
            _macroTitle = NewLabel("", Theme.Title, TextRole.Primary);
            _macroTitle.AutoEllipsis = true;
            _shortcut = NewLabel("", Theme.Callout, TextRole.Secondary);
            _shortcut.AutoEllipsis = true;

            _delayLabel = NewLabel("Atraso entre teclas (ms)", Theme.Callout, TextRole.Secondary);
            _delay = Theme.Register(new NumericUpDown
            {
                Minimum = 1,
                Maximum = 6000,
                Value = 100,
                Increment = 50,
                Font = Theme.Callout,
                BorderStyle = BorderStyle.None,
                TextAlign = HorizontalAlignment.Center,
            }, TextRole.Primary, SurfaceRole.Background);

            _sendButton = new PillButton { Text = "Enviar para o teclado", Style = PillStyle.Primary, Icon = "\uE724", Height = Theme.Scale(this, 42) };
            _sendButton.Click += (s, e) => SendRequested?.Invoke(this, EventArgs.Empty);

            _testButton = new PillButton { Text = "Testar no PC", Style = PillStyle.Plain, Font = Theme.CaptionSemibold, Height = Theme.Scale(this, 24) };
            _testButton.FitWidthToText();
            _testButton.Click += (s, e) => TestRequested?.Invoke(this, EventArgs.Empty);

            _cheatSheetButton = new PillButton { Text = "Gerar colinha", Style = PillStyle.Plain, Font = Theme.CaptionSemibold, Height = Theme.Scale(this, 24) };
            _cheatSheetButton.FitWidthToText();
            _cheatSheetButton.Click += (s, e) => CheatSheetRequested?.Invoke(this, EventArgs.Empty);

            _status = NewLabel("", Theme.Callout, TextRole.Secondary);
            _status.TextAlign = ContentAlignment.MiddleCenter;

            Controls.AddRange(new Control[] { _target, _macroTitle, _shortcut, _delayLabel, _delay, _sendButton, _testButton, _cheatSheetButton, _status });
        }

        public void ShowSelection(string target, Macro macro, bool supportsDelay)
        {
            _target.Text = target.ToUpperInvariant();
            _macroTitle.Text = macro == null ? "Escolha o que a tecla faz" : macro.Title;
            Theme.SetTextRole(_macroTitle, macro == null ? TextRole.Tertiary : TextRole.Primary);
            _shortcut.Text = macro == null ? "Use as abas ao lado: atalhos prontos, texto, gravação ou kits." : ShortcutText.Describe(macro);

            var showDelay = supportsDelay && macro != null && macro.Kind == MacroKind.Keys && macro.Keys.Count > 1;
            _delayLabel.Visible = showDelay;
            _delay.Visible = showDelay;

            // Clique e rolagem do mouse não dá para simular sem atrapalhar quem está usando o PC
            _testButton.Enabled = macro != null && macro.Kind != MacroKind.Mouse;
            PerformLayout();
        }

        public void SetCanSend(bool canSend)
        {
            _sendButton.Enabled = canSend;
        }

        public void ShowStatus(string text, TextRole role)
        {
            _status.Text = text;
            Theme.SetTextRole(_status, role);
        }

        protected override void OnLayout(LayoutEventArgs levent)
        {
            base.OnLayout(levent);
            if (_status == null)
                return;

            var padding = Theme.Scale(this, 20);
            var width = ClientSize.Width - padding * 2;
            _target.SetBounds(padding, padding, width, Theme.Scale(this, 18));
            _macroTitle.SetBounds(padding, _target.Bottom + Theme.Scale(this, 2), width, Theme.Scale(this, 30));
            _shortcut.SetBounds(padding, _macroTitle.Bottom, width, Theme.Scale(this, 22));

            var delayTop = _shortcut.Bottom + Theme.Scale(this, 6);
            _delayLabel.SetBounds(padding, delayTop, width - Theme.Scale(this, 90), Theme.Scale(this, 24));
            _delay.SetBounds(ClientSize.Width - padding - Theme.Scale(this, 80), delayTop, Theme.Scale(this, 80), Theme.Scale(this, 24));

            _status.SetBounds(padding, ClientSize.Height - padding - Theme.Scale(this, 20), width, Theme.Scale(this, 20));

            var extrasTop = _status.Top - Theme.Scale(this, 4) - _testButton.Height;
            var extrasWidth = _testButton.Width + Theme.Scale(this, 16) + _cheatSheetButton.Width;
            _testButton.Location = new Point(padding + (width - extrasWidth) / 2, extrasTop);
            _cheatSheetButton.Location = new Point(_testButton.Right + Theme.Scale(this, 16), extrasTop);

            _sendButton.SetBounds(padding, extrasTop - Theme.Scale(this, 6) - _sendButton.Height, width, _sendButton.Height);
        }

        private static Label NewLabel(string text, Font font, TextRole role)
        {
            return Theme.Register(new Label { Text = text, Font = font, UseMnemonic = false }, role, SurfaceRole.Card);
        }
    }
}
