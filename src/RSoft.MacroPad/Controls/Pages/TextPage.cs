using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using RSoft.MacroPad.BLL.Macros;

namespace RSoft.MacroPad.Controls.Pages
{
    /// <summary>
    /// Transforma um texto digitado numa macro que escreve esse texto.
    /// </summary>
    internal class TextPage : Panel
    {
        private readonly Label _title;
        private readonly Label _description;
        private readonly RoundedPanel _inputBox;
        private readonly TextBox _input;
        private readonly CheckBox _pressEnter;
        private readonly Label _counter;
        private readonly Label _warning;
        private readonly Label _suggestionsTitle;
        private readonly FlowLayoutPanel _suggestions;
        private readonly PillButton _useButton;
        private int _maxKeys = 18;

        public event EventHandler<Macro> MacroChosen;

        public TextPage()
        {
            Theme.Register(this, TextRole.Primary, SurfaceRole.Card);

            _title = NewLabel("Digitar um texto", Theme.Title, TextRole.Primary);
            _description = NewLabel("A tecla escreve o texto sozinha, como se você digitasse. Bom para saudações, e-mail e respostas rápidas.", Theme.Callout, TextRole.Secondary);

            _inputBox = new RoundedPanel { Surface = SurfaceRole.Background, Radius = 12 };
            _input = Theme.Register(new TextBox
            {
                BorderStyle = BorderStyle.None,
                Font = Theme.Title,
                Multiline = true,
                PlaceholderText = "Escreva aqui…",
            }, TextRole.Primary, SurfaceRole.Background);
            _input.TextChanged += (s, e) => UpdateState();
            _inputBox.Controls.Add(_input);

            _pressEnter = Theme.Register(new CheckBox
            {
                Text = "Apertar Enter no final (bom para chats)",
                Font = Theme.Callout,
                AutoSize = true,
                Cursor = Cursors.Hand,
                FlatStyle = FlatStyle.Flat,
            }, TextRole.Primary, SurfaceRole.Card);
            _pressEnter.CheckedChanged += (s, e) => UpdateState();

            _counter = NewLabel("", Theme.CalloutSemibold, TextRole.Secondary);
            _counter.TextAlign = ContentAlignment.MiddleRight;
            _warning = NewLabel("", Theme.Callout, TextRole.Warning);

            _suggestionsTitle = NewLabel("Textos prontos", Theme.Headline, TextRole.Primary);
            _suggestions = Theme.Register(new FlowLayoutPanel { WrapContents = true }, TextRole.Primary, SurfaceRole.Card);
            foreach (var macro in MacroCatalog.Macros.Where(m => m.Category == "Texto"))
            {
                var chip = new PillButton
                {
                    Text = macro.Title,
                    Style = PillStyle.Secondary,
                    Font = Theme.Callout,
                    Height = Theme.Scale(this, 30),
                    Margin = new Padding(0, 0, Theme.Scale(this, 6), Theme.Scale(this, 6)),
                };
                chip.FitWidthToText();
                chip.Click += (s, e) => _input.Text = macro.Title;
                _suggestions.Controls.Add(chip);
            }

            _useButton = new PillButton { Text = "Usar este texto", Style = PillStyle.Primary, Height = Theme.Scale(this, 38) };
            _useButton.FitWidthToText();
            _useButton.Click += (s, e) => ChooseText();

            Controls.AddRange(new Control[] { _title, _description, _inputBox, _pressEnter, _counter, _warning, _suggestionsTitle, _suggestions, _useButton });
            UpdateState();
        }

        public void SetMaxKeys(int maxKeys)
        {
            _maxKeys = maxKeys;
            UpdateState();
        }

        protected override void OnLayout(LayoutEventArgs levent)
        {
            base.OnLayout(levent);
            if (_useButton == null)
                return;

            var width = ClientSize.Width;
            var gap = Theme.Scale(this, 8);
            _title.SetBounds(0, 0, width, Theme.Scale(this, 30));
            _description.SetBounds(0, _title.Bottom, width, Theme.Scale(this, 40));

            _inputBox.SetBounds(0, _description.Bottom + gap, width, Theme.Scale(this, 110));
            var inner = Theme.Scale(this, 14);
            _input.SetBounds(inner, inner, _inputBox.Width - inner * 2, _inputBox.Height - inner * 2);

            _pressEnter.Location = new Point(0, _inputBox.Bottom + gap + Theme.Scale(this, 2));
            _counter.SetBounds(width / 2, _inputBox.Bottom + gap, width / 2, Theme.Scale(this, 26));
            _warning.SetBounds(0, _pressEnter.Bottom + gap, width, Theme.Scale(this, 40));

            _useButton.Location = new Point(0, _warning.Bottom + gap);

            _suggestionsTitle.SetBounds(0, _useButton.Bottom + Theme.Scale(this, 28), width, Theme.Scale(this, 26));
            var suggestionsHeight = _suggestions.GetPreferredSize(new Size(width, 0)).Height;
            _suggestions.SetBounds(0, _suggestionsTitle.Bottom + gap, width, suggestionsHeight);
        }

        private void UpdateState()
        {
            var typed = TextTyper.ToKeys(_input.Text, _pressEnter.Checked);
            var count = typed.Keys.Count;
            var tooLong = count > _maxKeys;

            _counter.Text = $"{count} de {_maxKeys} teclas";
            Theme.SetTextRole(_counter, tooLong ? TextRole.Danger : TextRole.Secondary);

            if (typed.UnsupportedCharacters.Length > 0)
                _warning.Text = $"Estes caracteres vão ficar de fora: {typed.UnsupportedCharacters}  (acentos, ç e símbolos como ? ; : / mudam conforme o layout do PC)";
            else if (tooLong)
                _warning.Text = $"Este modelo guarda no máximo {_maxKeys} teclas por macro. Encurte o texto.";
            else
                _warning.Text = "";

            _useButton.Enabled = count > 0 && !tooLong;
        }

        private void ChooseText()
        {
            var typed = TextTyper.ToKeys(_input.Text, _pressEnter.Checked);
            var title = _input.Text.Trim().Replace("\r", "").Replace('\n', ' ');
            MacroChosen?.Invoke(this, new Macro
            {
                Id = "custom-text",
                Title = $"“{title}”",
                Category = "Texto",
                Icon = "\uE8D2",
                Kind = MacroKind.Keys,
                Keys = typed.Keys,
            });
        }

        private static Label NewLabel(string text, Font font, TextRole role)
        {
            return Theme.Register(new Label { Text = text, Font = font, UseMnemonic = false }, role, SurfaceRole.Card);
        }
    }
}
