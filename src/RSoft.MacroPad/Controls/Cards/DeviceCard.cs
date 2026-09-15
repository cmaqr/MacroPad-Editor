using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using RSoft.MacroPad.BLL.Infrasturture.Model;
using RSoft.MacroPad.BLL.Infrasturture.Physical;

namespace RSoft.MacroPad.Controls.Cards
{
    /// <summary>
    /// Cartão "Seu teclado": modelo, camada e o desenho clicável das teclas.
    /// </summary>
    internal class DeviceCard : RoundedPanel
    {
        private readonly Label _title;
        private readonly PillButton _modelButton;
        private readonly ContextMenuStrip _modelMenu;
        private readonly Label _detection;
        private readonly Label _layerLabel;
        private readonly SegmentedControl _layers;
        private readonly PadView _pad;
        private readonly SegmentedControl _knobActions;
        private readonly Label _untestedWarning;
        private readonly RoundedPanel _unknownDevicePanel;
        private readonly Label _unknownDeviceLabel;
        private KeyboardLayout _layout;

        public event EventHandler<KeyboardLayout> LayoutChosen;
        public event EventHandler<byte> LayerChanged;
        public event EventHandler<InputAction> ActionSelected;

        /// <summary>Disparado quando o usuário aceita tentar usar um macropad que não está na lista.</summary>
        public event EventHandler UnknownDeviceAccepted;

        public byte Layer => (byte)(_layers.SelectedIndex + 1);
        public InputAction SelectedAction => _pad.SelectedAction;

        public DeviceCard()
        {
            _title = NewLabel("Seu teclado", Theme.Title, TextRole.Primary);

            _modelMenu = new ContextMenuStrip { ShowImageMargin = false, Font = Theme.Body };
            _modelButton = new PillButton { Style = PillStyle.Secondary, Font = Theme.CalloutSemibold, Icon = "\uE70D", IconAfterText = true, Height = Theme.Scale(this, 30) };
            _modelButton.Click += (s, e) => _modelMenu.Show(_modelButton, new Point(0, _modelButton.Height + Theme.Scale(this, 4)));

            _detection = NewLabel("", Theme.Caption, TextRole.Secondary);

            _layerLabel = NewLabel("Camada", Theme.CalloutSemibold, TextRole.Secondary);
            _layers = new SegmentedControl { Height = Theme.Scale(this, 28) };
            _layers.SelectedIndexChanged += (s, e) => LayerChanged?.Invoke(this, Layer);

            _pad = new PadView();
            _pad.ActionSelected += (s, action) => SelectAction(action);

            _knobActions = new SegmentedControl { Items = new[] { "↺  Girar à esquerda", "Apertar", "Girar à direita  ↻" }, Height = Theme.Scale(this, 30), Visible = false };
            _knobActions.SelectedIndexChanged += (s, e) => SelectKnobAction(_knobActions.SelectedIndex);

            _untestedWarning = Theme.Register(new Label
            {
                Text = "Este modelo ainda não foi testado pelo autor do app. Teste com calma e confira se funciona.",
                Font = Theme.Caption,
                Padding = new Padding(Theme.Scale(this, 8), 0, Theme.Scale(this, 8), 0),
                TextAlign = ContentAlignment.MiddleLeft,
                Visible = false,
            }, TextRole.Warning, SurfaceRole.WarningSoft);

            _unknownDevicePanel = new RoundedPanel { Surface = SurfaceRole.WarningSoft, Radius = 10, Visible = false };
            _unknownDeviceLabel = Theme.Register(new Label { Font = Theme.Caption, UseMnemonic = false }, TextRole.Warning, SurfaceRole.WarningSoft);
            var tryAnywayButton = new PillButton { Text = "Tentar assim mesmo", Style = PillStyle.Primary, Font = Theme.CaptionSemibold, Height = Theme.Scale(this, 26) };
            tryAnywayButton.FitWidthToText();
            tryAnywayButton.Click += (s, e) => UnknownDeviceAccepted?.Invoke(this, EventArgs.Empty);
            _unknownDevicePanel.Controls.AddRange(new Control[] { _unknownDeviceLabel, tryAnywayButton });
            _unknownDevicePanel.Layout += (s, e) =>
            {
                var inner = Theme.Scale(this, 10);
                tryAnywayButton.Location = new Point(_unknownDevicePanel.Width - tryAnywayButton.Width - inner, (_unknownDevicePanel.Height - tryAnywayButton.Height) / 2);
                _unknownDeviceLabel.SetBounds(inner, inner / 2, tryAnywayButton.Left - inner * 2, _unknownDevicePanel.Height - inner);
            };

            // Os primeiros da lista ficam por cima: a camada precisa ficar na frente do título, que ocupa a largura toda
            Controls.AddRange(new Control[] { _layerLabel, _layers, _title, _modelButton, _detection, _pad, _knobActions, _untestedWarning, _unknownDevicePanel });
        }

        public void SetLayouts(IEnumerable<KeyboardLayout> layouts)
        {
            _modelMenu.Items.Clear();
            foreach (var layout in layouts)
            {
                var item = new ToolStripMenuItem(DisplayName(layout)) { Tag = layout };
                item.Click += (s, e) => LayoutChosen?.Invoke(this, layout);
                _modelMenu.Items.Add(item);
            }
        }

        public void ShowLayout(KeyboardLayout layout, bool detected)
        {
            _layout = layout;
            _modelButton.Text = DisplayName(layout);
            _modelButton.FitWidthToText();
            _detection.Text = detected
                ? "Detectado automaticamente pelo USB"
                : "Não é o seu? Clique no nome e escolha o mais parecido.";

            var layerCount = Math.Max(1, (int)layout.LayerCount);
            var previousLayer = _layers.SelectedIndex;
            _layers.Items = Enumerable.Range(1, layerCount).Select(i => i.ToString()).ToArray();
            _layers.SelectedIndex = Math.Min(previousLayer, layerCount - 1);
            _layerLabel.Visible = layerCount > 1;
            _layers.Visible = layerCount > 1;

            _pad.KeyboardLayout = layout;
            SelectAction(layout.Controls.OrderBy(c => c is PhysicalKnob).ThenBy(c => c.Actions.First()).First().Actions.First());
            PerformLayout();
        }

        public void SetTitles(IReadOnlyDictionary<InputAction, string> titles)
        {
            _pad.Titles = titles;
        }

        public void ShowUntestedWarning(bool visible)
        {
            _untestedWarning.Visible = visible;
            PerformLayout();
        }

        /// <summary>Mostra o aviso de macropad ligado que não está na lista de suportados.</summary>
        public void ShowUnknownDevice(string message)
        {
            _unknownDeviceLabel.Text = message;
            _unknownDevicePanel.Visible = true;
            PerformLayout();
        }

        public void HideUnknownDevice()
        {
            if (!_unknownDevicePanel.Visible)
                return;
            _unknownDevicePanel.Visible = false;
            PerformLayout();
        }

        protected override void OnLayout(LayoutEventArgs levent)
        {
            base.OnLayout(levent);
            if (_untestedWarning == null)
                return;

            var padding = Theme.Scale(this, 20);
            var width = ClientSize.Width - padding * 2;
            _title.SetBounds(padding, padding - Theme.Scale(this, 2), width, Theme.Scale(this, 28));
            _modelButton.Location = new Point(padding, _title.Bottom + Theme.Scale(this, 6));
            _detection.SetBounds(padding, _modelButton.Bottom + Theme.Scale(this, 4), width, Theme.Scale(this, 20));

            var layersWidth = Theme.Scale(this, 40) * Math.Max(1, _layers.Items.Count);
            _layers.SetBounds(ClientSize.Width - padding - layersWidth, padding, layersWidth, _layers.Height);
            _layerLabel.SetBounds(_layers.Left - Theme.Scale(this, 64), padding, Theme.Scale(this, 60), _layers.Height);
            _layerLabel.TextAlign = ContentAlignment.MiddleRight;

            var bottom = ClientSize.Height - padding;
            if (_unknownDevicePanel.Visible)
            {
                _unknownDevicePanel.SetBounds(padding, bottom - Theme.Scale(this, 46), width, Theme.Scale(this, 46));
                bottom = _unknownDevicePanel.Top - Theme.Scale(this, 10);
            }
            if (_untestedWarning.Visible)
            {
                _untestedWarning.SetBounds(padding, bottom - Theme.Scale(this, 40), width, Theme.Scale(this, 40));
                bottom = _untestedWarning.Top - Theme.Scale(this, 10);
            }
            if (_knobActions.Visible)
            {
                _knobActions.SetBounds(padding, bottom - _knobActions.Height, width, _knobActions.Height);
                bottom = _knobActions.Top - Theme.Scale(this, 10);
            }

            var padTop = _detection.Bottom + Theme.Scale(this, 10);
            _pad.SetBounds(padding, padTop, width, Math.Max(0, bottom - padTop));
        }

        private void SelectAction(InputAction action)
        {
            _pad.SelectedAction = action;

            var knob = _layout?.Controls.OfType<PhysicalKnob>().FirstOrDefault(k => k.Actions.Contains(action));
            var wasVisible = _knobActions.Visible;
            _knobActions.Visible = knob != null;
            if (knob != null)
            {
                // Dispara SelectKnobAction, que não faz nada porque a ação já é a selecionada
                _knobActions.SelectedIndex = Array.IndexOf(knob.Actions.ToArray(), action);
            }
            if (wasVisible != _knobActions.Visible)
                PerformLayout();

            ActionSelected?.Invoke(this, action);
        }

        private void SelectKnobAction(int index)
        {
            var knob = _layout?.Controls.OfType<PhysicalKnob>().FirstOrDefault(k => k.Actions.Contains(_pad.SelectedAction));
            if (knob == null)
                return;
            var action = knob.Actions.ElementAt(index);
            if (action != _pad.SelectedAction)
                SelectAction(action);
        }

        /// <summary>Os nomes do layouts.txt estão em inglês ("3 buttons 1 knob"); aqui viram português.</summary>
        public static string DisplayName(KeyboardLayout layout)
        {
            return layout.Name.Replace("buttons", "teclas").Replace("button", "tecla").Replace("Btn", " teclas").Replace("Kn)", " knobs)");
        }

        private static Label NewLabel(string text, Font font, TextRole role)
        {
            return Theme.Register(new Label { Text = text, Font = font, UseMnemonic = false }, role, SurfaceRole.Card);
        }
    }
}
