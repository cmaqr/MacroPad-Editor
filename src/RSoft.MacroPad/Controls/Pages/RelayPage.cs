using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using RSoft.MacroPad.BLL.Infrasturture.Model;
using RSoft.MacroPad.BLL.Macros;

namespace RSoft.MacroPad.Controls.Pages
{
    /// <summary>
    /// Liga cada tecla F13 a F24 a uma ação do PC (digitar texto ou abrir algo).
    /// </summary>
    internal class RelayPage : Panel
    {
        private static readonly KeyCode[] RelayKeys =
        {
            KeyCode.F13, KeyCode.F14, KeyCode.F15, KeyCode.F16, KeyCode.F17, KeyCode.F18,
            KeyCode.F19, KeyCode.F20, KeyCode.F21, KeyCode.F22, KeyCode.F23, KeyCode.F24,
        };

        private readonly RelayStore _store;
        private readonly Label _title;
        private readonly Label _description;
        private readonly Label _status;
        private readonly FlowLayoutPanel _rows;

        /// <summary>Pedido de gravar a tecla F correspondente no macropad.</summary>
        public event EventHandler<Macro> MacroChosen;

        public event EventHandler ActionsChanged;

        public RelayPage(RelayStore store)
        {
            _store = store;
            Theme.Register(this, TextRole.Primary, SurfaceRole.Card);

            _title = NewLabel("Turbo: teclas F13 a F24", Theme.Title, TextRole.Primary);
            _description = NewLabel(
                "Teclado comum não tem F13 a F24, então elas não brigam com atalho nenhum. Grave uma delas numa tecla do macropad "
                + "e diga aqui o que o PC faz quando ela chegar: digitar um texto longo (com acento) ou abrir um programa, pasta ou site. "
                + "Funciona enquanto este app estiver aberto.", Theme.Callout, TextRole.Secondary);
            _status = NewLabel("", Theme.CalloutSemibold, TextRole.Secondary);

            _rows = Theme.Register(new FlowLayoutPanel { AutoScroll = true, WrapContents = false, FlowDirection = FlowDirection.TopDown }, TextRole.Primary, SurfaceRole.Card);
            foreach (var key in RelayKeys)
                _rows.Controls.Add(new RelayRow(key, _store, this));
            _rows.Resize += (s, e) => ResizeRows();

            Controls.AddRange(new Control[] { _title, _description, _status, _rows });
            UpdateStatus();
        }

        protected override void OnLayout(LayoutEventArgs levent)
        {
            base.OnLayout(levent);
            if (_rows == null)
                return;

            var width = ClientSize.Width;
            _title.SetBounds(0, 0, width, Theme.Scale(this, 30));
            _description.SetBounds(0, _title.Bottom, width, Theme.Scale(this, 58));
            _status.SetBounds(0, _description.Bottom, width, Theme.Scale(this, 24));
            var rowsTop = _status.Bottom + Theme.Scale(this, 8);
            _rows.SetBounds(0, rowsTop, width, Math.Max(0, ClientSize.Height - rowsTop));
        }

        private void UpdateStatus()
        {
            var configured = RelayKeys.Count(key => _store.Get(key.ToString()).ParsedKind != RelayKind.None && _store.Get(key.ToString()).Value.Length > 0);
            _status.Text = configured == 0
                ? "Nenhuma tecla configurada ainda."
                : $"● Ouvindo {configured} tecla(s). O app precisa ficar aberto para elas funcionarem.";
            Theme.SetTextRole(_status, configured == 0 ? TextRole.Secondary : TextRole.Success);
        }

        private void ResizeRows()
        {
            var width = _rows.ClientSize.Width - SystemInformation.VerticalScrollBarWidth - Theme.Scale(this, 2);
            foreach (RelayRow row in _rows.Controls)
                row.Size = new Size(Math.Max(Theme.Scale(this, 320), width), Theme.Scale(this, 54));
        }

        private void RowChanged()
        {
            UpdateStatus();
            ActionsChanged?.Invoke(this, EventArgs.Empty);
        }

        private void StageKeyMacro(KeyCode key)
        {
            MacroChosen?.Invoke(this, new Macro
            {
                Id = "relay-" + key,
                Title = $"Turbo {key}",
                Category = "Streaming",
                Icon = key.ToString(),
                Hint = "Faz o PC executar a ação configurada na aba Turbo",
                Kind = MacroKind.Keys,
                Keys = new[] { (key, Modifier.None) },
            });
        }

        private static Label NewLabel(string text, Font font, TextRole role)
        {
            return Theme.Register(new Label { Text = text, Font = font, UseMnemonic = false }, role, SurfaceRole.Card);
        }

        /// <summary>Uma linha da lista: a tecla, o tipo de ação, o valor e o botão de gravar no macropad.</summary>
        private sealed class RelayRow : RoundedPanel
        {
            private readonly KeyCode _key;
            private readonly RelayStore _store;
            private readonly RelayPage _page;
            private readonly Label _keyLabel;
            private readonly SegmentedControl _kind;
            private readonly RoundedPanel _valueBox;
            private readonly TextBox _value;
            private readonly PillButton _useButton;

            public RelayRow(KeyCode key, RelayStore store, RelayPage page)
            {
                _key = key;
                _store = store;
                _page = page;
                Surface = SurfaceRole.Background;
                Radius = 12;
                Margin = new Padding(0, 0, 0, Theme.Scale(this, 8));

                var action = store.Get(key.ToString());

                _keyLabel = Theme.Register(new Label { Text = key.ToString(), Font = Theme.Headline, TextAlign = ContentAlignment.MiddleLeft }, TextRole.Primary, SurfaceRole.Background);
                _kind = new SegmentedControl { Items = new[] { "Nada", "Digitar", "Abrir" }, Height = Theme.Scale(this, 28) };
                _kind.SelectedIndex = (int)action.ParsedKind;
                _kind.SelectedIndexChanged += (s, e) => Save();

                _valueBox = new RoundedPanel { Surface = SurfaceRole.Fill, Radius = 9 };
                _value = Theme.Register(new TextBox { BorderStyle = BorderStyle.None, Font = Theme.Callout, Text = action.Value }, TextRole.Primary, SurfaceRole.Fill);
                _value.Leave += (s, e) => Save();
                _valueBox.Controls.Add(_value);
                _valueBox.Resize += (s, e) => PlaceValue();

                _useButton = new PillButton { Text = "Gravar no macropad", Style = PillStyle.Secondary, Font = Theme.CaptionSemibold, Height = Theme.Scale(this, 28) };
                _useButton.FitWidthToText();
                _useButton.Click += (s, e) => _page.StageKeyMacro(_key);

                Controls.AddRange(new Control[] { _keyLabel, _kind, _valueBox, _useButton });
                UpdatePlaceholder();
            }

            protected override void OnLayout(LayoutEventArgs levent)
            {
                base.OnLayout(levent);
                if (_useButton == null)
                    return;

                var padding = Theme.Scale(this, 12);
                var middle = (Height - _kind.Height) / 2;
                _keyLabel.SetBounds(padding, 0, Theme.Scale(this, 44), Height);
                _kind.SetBounds(_keyLabel.Right, middle, Theme.Scale(this, 190), _kind.Height);
                _useButton.Location = new Point(Width - _useButton.Width - padding, middle);
                var valueLeft = _kind.Right + Theme.Scale(this, 10);
                _valueBox.SetBounds(valueLeft, middle, Math.Max(Theme.Scale(this, 60), _useButton.Left - valueLeft - Theme.Scale(this, 10)), _kind.Height);
            }

            private void PlaceValue()
            {
                var inner = Theme.Scale(this, 9);
                _value.SetBounds(inner, (_valueBox.Height - _value.PreferredHeight) / 2, _valueBox.Width - inner * 2, _value.PreferredHeight);
            }

            private void Save()
            {
                UpdatePlaceholder();
                _store.Set(_key.ToString(), (RelayKind)_kind.SelectedIndex, _value.Text);
                _page.RowChanged();
            }

            private void UpdatePlaceholder()
            {
                var kind = (RelayKind)_kind.SelectedIndex;
                _value.Enabled = kind != RelayKind.None;
                _value.PlaceholderText = kind switch
                {
                    RelayKind.Text => "Texto que o PC vai digitar",
                    RelayKind.Open => "Programa, pasta ou site. Ex.: notepad.exe ou https://...",
                    _ => "Escolha Digitar ou Abrir",
                };
            }
        }
    }
}
