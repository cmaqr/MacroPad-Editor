using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using RSoft.MacroPad.BLL.Infrasturture.Model;
using RSoft.MacroPad.BLL.Infrasturture.Physical;

namespace RSoft.MacroPad.Controls.Pages
{
    /// <summary>
    /// Efeito e cor da iluminação. Nos teclados com RGB por tecla dá para pintar uma tecla de cada vez:
    /// escolhe a cor na paleta e clica na tecla no desenho.
    /// </summary>
    internal class LightPage : Panel
    {
        private static readonly (string Name, Color Swatch)[] Colors =
        {
            ("Aleatória", Color.Empty),
            ("Branco", Color.FromArgb(255, 255, 255)),
            ("Gelo", Color.FromArgb(200, 225, 255)),
            ("Vermelho", Color.FromArgb(255, 0, 0)),
            ("Coral", Color.FromArgb(255, 80, 60)),
            ("Rosa", Color.FromArgb(255, 45, 130)),
            ("Magenta", Color.FromArgb(255, 0, 200)),
            ("Roxo", Color.FromArgb(160, 0, 255)),
            ("Violeta", Color.FromArgb(110, 60, 255)),
            ("Índigo", Color.FromArgb(50, 60, 255)),
            ("Azul", Color.FromArgb(0, 80, 255)),
            ("Azul-céu", Color.FromArgb(0, 170, 255)),
            ("Ciano", Color.FromArgb(0, 255, 255)),
            ("Turquesa", Color.FromArgb(0, 255, 180)),
            ("Verde", Color.FromArgb(0, 255, 0)),
            ("Lima", Color.FromArgb(150, 255, 0)),
            ("Amarelo", Color.FromArgb(255, 255, 0)),
            ("Âmbar", Color.FromArgb(255, 180, 0)),
            ("Laranja", Color.FromArgb(255, 100, 0)),
        };

        // Nomes reais dos efeitos do dialeto 514C; nos outros modelos só dá para numerar
        private static readonly string[] Ch57x3ModeNames = { "Apagada", "Fixa", "Ao toque", "Onda", "Arco-íris" };

        private readonly Label _title;
        private readonly Label _description;
        private readonly Label _modeTitle;
        private readonly SegmentedControl _modes;
        private readonly Label _colorTitle;
        private readonly ColorPicker _colors;
        private readonly Label _noColors;
        private readonly PillButton _customColorButton;
        private readonly Label _padTitle;
        private readonly PadView _pad;
        private readonly PillButton _paintAllButton;
        private readonly PillButton _clearPaintButton;
        private readonly PillButton _applyButton;

        private LightScheme _scheme = new LightScheme();
        private bool _supportsPerKey;

        public event EventHandler<LightScheme> LightApplyRequested;

        public LightPage()
        {
            Theme.Register(this, TextRole.Primary, SurfaceRole.Card);
            AutoScroll = true;

            _title = NewLabel("Iluminação", Theme.Title, TextRole.Primary);
            _description = NewLabel("Escolha o efeito e a cor. Se o seu modelo tem RGB por tecla, qualquer cor vale — inclusive branco.", Theme.Callout, TextRole.Secondary);

            _modeTitle = NewLabel("Efeito", Theme.Headline, TextRole.Primary);
            _modes = new SegmentedControl { Height = Theme.Scale(this, 34) };

            _colorTitle = NewLabel("Cor", Theme.Headline, TextRole.Primary);
            _colors = new ColorPicker();
            _noColors = NewLabel("Este modelo não permite escolher a cor.", Theme.Callout, TextRole.Tertiary);

            _customColorButton = new PillButton { Text = "Outra cor…", Style = PillStyle.Secondary, Icon = "", Height = Theme.Scale(this, 34) };
            _customColorButton.FitWidthToText();
            _customColorButton.Click += (s, e) => PickCustomColor();

            _padTitle = NewLabel("Clique numa tecla para pintar só ela", Theme.Headline, TextRole.Primary);
            _pad = new PadView { MaxScale = 2.6f };
            _pad.ActionSelected += (s, action) => PaintKey(action);

            _paintAllButton = new PillButton { Text = "Pintar todas", Style = PillStyle.Secondary, Icon = "", Height = Theme.Scale(this, 34) };
            _paintAllButton.FitWidthToText();
            _paintAllButton.Click += (s, e) => PaintAll();

            _clearPaintButton = new PillButton { Text = "Limpar pintura", Style = PillStyle.Secondary, Icon = "", Height = Theme.Scale(this, 34) };
            _clearPaintButton.FitWidthToText();
            _clearPaintButton.Click += (s, e) => ClearPaint();

            _applyButton = new PillButton { Text = "Aplicar iluminação", Style = PillStyle.Primary, Icon = "", Height = Theme.Scale(this, 38) };
            _applyButton.FitWidthToText();
            _applyButton.Click += (s, e) => Apply();

            Controls.AddRange(new Control[]
            {
                _title, _description, _modeTitle, _modes, _colorTitle, _colors, _noColors, _customColorButton,
                _padTitle, _pad, _paintAllButton, _clearPaintButton, _applyButton,
            });
        }

        public void SetLayout(KeyboardLayout layout, ProtocolType protocol)
        {
            // Só o dialeto 514C tem RGB por tecla; nos outros a tela fica só com efeito e cor geral
            _supportsPerKey = protocol == ProtocolType.Ch57x3;

            if (_supportsPerKey)
                _modes.Items = Ch57x3ModeNames;
            else
                _modes.Items = Enumerable.Range(1, Math.Max(1, (int)layout.LedModeCount)).Select(i => $"Modo {i}").ToArray();
            _modes.SelectedIndex = Math.Min((int)_scheme.Mode, _modes.Items.Count - 1);

            _colors.Visible = layout.SupportsColor;
            _customColorButton.Visible = layout.SupportsColor;
            _noColors.Visible = !layout.SupportsColor;

            _pad.KeyboardLayout = layout;
            _padTitle.Visible = _supportsPerKey;
            _pad.Visible = _supportsPerKey;
            _paintAllButton.Visible = _supportsPerKey;
            _clearPaintButton.Visible = _supportsPerKey;

            RefreshPad();
            PerformLayout();
        }

        /// <summary>Recarrega a tela com um esquema salvo, sem enviar nada para o teclado.</summary>
        public void ShowScheme(LightScheme scheme)
        {
            _scheme = scheme ?? new LightScheme();
            if (_modes.Items.Count > 0)
                _modes.SelectedIndex = Math.Min((int)_scheme.Mode, _modes.Items.Count - 1);
            RefreshPad();
        }

        private void PaintKey(InputAction action)
        {
            // Knob não tem luz nesse teclado, então clicar nele não pinta nada
            if (!_supportsPerKey || action >= InputAction.Knob1Left)
                return;

            _scheme.KeyColors[(int)action] = _colors.SelectedColor;
            RefreshPad();
        }

        private void PaintAll()
        {
            _scheme.BaseColor = _colors.SelectedColor;
            _scheme.KeyColors.Clear();
            RefreshPad();
        }

        private void ClearPaint()
        {
            _scheme.KeyColors.Clear();
            RefreshPad();
        }

        private void Apply()
        {
            _scheme.Mode = (LedMode)Math.Max(0, _modes.SelectedIndex);
            if (!_supportsPerKey)
                _scheme.BaseColor = _colors.SelectedColor;
            LightApplyRequested?.Invoke(this, _scheme);
        }

        private void RefreshPad()
        {
            var colors = new Dictionary<InputAction, Color>();
            foreach (var control in _pad.KeyboardLayout?.Controls ?? Enumerable.Empty<PhysicalControl>())
            {
                var action = control.Actions.First();
                if (action >= InputAction.Knob1Left)
                    continue;
                colors[action] = ToScreenColor(_scheme.ColorOf((int)action));
            }
            _pad.KeyColors = colors;
        }

        /// <summary>A cor "aleatória" não tem como ser mostrada, então no desenho ela vira um cinza.</summary>
        private static Color ToScreenColor(LedRgb color)
        {
            return color.IsRandom ? Color.FromArgb(120, 120, 120) : Color.FromArgb(color.Red, color.Green, color.Blue);
        }

        private void PickCustomColor()
        {
            using var dialog = new ColorDialog { Color = _colors.SelectedSwatch, FullOpen = true, AnyColor = true };
            if (dialog.ShowDialog(this) == DialogResult.OK)
                _colors.SetCustomColor(dialog.Color);
        }

        protected override void OnLayout(LayoutEventArgs levent)
        {
            base.OnLayout(levent);
            if (_applyButton == null)
                return;

            var width = ClientSize.Width;
            var gap = Theme.Scale(this, 8);
            _title.SetBounds(0, 0, width, Theme.Scale(this, 30));
            _description.SetBounds(0, _title.Bottom, width, Theme.Scale(this, 40));

            _modeTitle.SetBounds(0, _description.Bottom + Theme.Scale(this, 16), width, Theme.Scale(this, 26));
            _modes.SetBounds(0, _modeTitle.Bottom + gap, Math.Min(width, Theme.Scale(this, 90) * Math.Max(1, _modes.Items.Count)), _modes.Height);

            _colorTitle.SetBounds(0, _modes.Bottom + Theme.Scale(this, 20), width, Theme.Scale(this, 26));
            _colors.SetBounds(0, _colorTitle.Bottom + gap, width, _colors.PreferredHeightFor(width));
            _noColors.SetBounds(0, _colorTitle.Bottom + gap, width, Theme.Scale(this, 26));

            var afterColors = (_colors.Visible ? _colors.Bottom : _noColors.Bottom) + Theme.Scale(this, 10);
            _customColorButton.Location = new Point(0, afterColors);
            var bottom = (_customColorButton.Visible ? _customColorButton.Bottom : afterColors) + Theme.Scale(this, 20);

            if (_pad.Visible)
            {
                _padTitle.SetBounds(0, bottom, width, Theme.Scale(this, 26));
                _pad.SetBounds(0, _padTitle.Bottom + gap, width, Theme.Scale(this, 150));
                _paintAllButton.Location = new Point(0, _pad.Bottom + gap);
                _clearPaintButton.Location = new Point(_paintAllButton.Right + gap, _pad.Bottom + gap);
                bottom = _paintAllButton.Bottom + Theme.Scale(this, 20);
            }

            _applyButton.Location = new Point(0, bottom);
        }

        private static Label NewLabel(string text, Font font, TextRole role)
        {
            return Theme.Register(new Label { Text = text, Font = font, UseMnemonic = false }, role, SurfaceRole.Card);
        }

        /// <summary>Bolinhas de cor com o nome embaixo, quebrando em linhas; a selecionada ganha um anel azul.</summary>
        private sealed class ColorPicker : Control
        {
            private int _selectedIndex = 1;
            private Color? _customColor;

            public ColorPicker()
            {
                SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw, true);
                Cursor = Cursors.Hand;
            }

            /// <summary>A cor mostrada na bolinha selecionada, para abrir a paleta do Windows já nela.</summary>
            public Color SelectedSwatch
            {
                get
                {
                    if (_customColor.HasValue && _selectedIndex == Colors.Length)
                        return _customColor.Value;
                    var swatch = Colors[_selectedIndex].Swatch;
                    return swatch == Color.Empty ? Color.White : swatch;
                }
            }

            public LedRgb SelectedColor
            {
                get
                {
                    if (_customColor.HasValue && _selectedIndex == Colors.Length)
                        return new LedRgb(_customColor.Value.R, _customColor.Value.G, _customColor.Value.B);
                    var swatch = Colors[_selectedIndex].Swatch;
                    if (swatch == Color.Empty)
                        return LedRgb.Random();
                    return new LedRgb(swatch.R, swatch.G, swatch.B);
                }
            }

            public void SetCustomColor(Color color)
            {
                _customColor = color;
                _selectedIndex = Colors.Length;
                Invalidate();
                Parent?.PerformLayout();
            }

            public int PreferredHeightFor(int width)
            {
                var rows = (int)Math.Ceiling(Count() / (double)ColumnsFor(width));
                return rows * RowHeight();
            }

            protected override void OnPaint(PaintEventArgs e)
            {
                var graphics = e.Graphics;
                graphics.Clear(Parent?.BackColor ?? Theme.Card);
                Theme.PrepareGraphics(graphics);

                var columns = ColumnsFor(Width);
                var cell = CellWidth();
                var rowHeight = RowHeight();
                var diameter = Theme.Scale(this, 34f);

                for (var i = 0; i < Count(); i++)
                {
                    var name = i == Colors.Length ? "Personalizada" : Colors[i].Name;
                    var swatch = i == Colors.Length ? _customColor.Value : Colors[i].Swatch;
                    var left = (i % columns) * cell;
                    var top = (i / columns) * rowHeight + Theme.Scale(this, 5f);
                    var circle = new RectangleF(left + (cell - diameter) / 2, top, diameter, diameter);

                    if (swatch == Color.Empty)
                    {
                        // "Aleatória" é desenhada como uma pizza com quatro cores
                        var slices = new[] { Colors[3].Swatch, Colors[14].Swatch, Colors[10].Swatch, Colors[16].Swatch };
                        for (var s = 0; s < slices.Length; s++)
                        {
                            using var brush = new SolidBrush(slices[s]);
                            graphics.FillPie(brush, circle.X, circle.Y, circle.Width, circle.Height, s * 90, 90);
                        }
                    }
                    else
                    {
                        using var brush = new SolidBrush(swatch);
                        graphics.FillEllipse(brush, circle);
                    }

                    // Cor clara some no fundo claro, então ganha um contorno
                    if (swatch != Color.Empty && swatch.GetBrightness() > 0.8f)
                    {
                        using var outline = new Pen(Theme.Separator, Theme.Scale(this, 1.5f));
                        graphics.DrawEllipse(outline, circle);
                    }

                    if (i == _selectedIndex)
                    {
                        var ringWidth = Theme.Scale(this, 2.5f);
                        var ring = RectangleF.Inflate(circle, ringWidth * 1.6f, ringWidth * 1.6f);
                        using var pen = new Pen(Theme.Accent, ringWidth);
                        graphics.DrawEllipse(pen, ring);
                    }

                    var labelBounds = new Rectangle((int)left, (int)(circle.Bottom + Theme.Scale(this, 6f)), (int)cell, Theme.Scale(this, 20));
                    TextRenderer.DrawText(graphics, name, i == _selectedIndex ? Theme.CaptionSemibold : Theme.Caption, labelBounds,
                        i == _selectedIndex ? Theme.Text : Theme.SecondaryText, TextFormatFlags.HorizontalCenter | TextFormatFlags.Top | TextFormatFlags.NoPrefix);
                }
            }

            protected override void OnMouseDown(MouseEventArgs e)
            {
                var columns = ColumnsFor(Width);
                var column = Math.Clamp((int)(e.X / CellWidth()), 0, columns - 1);
                var row = Math.Max(0, e.Y / RowHeight());
                var index = row * columns + column;
                if (index < Count())
                {
                    _selectedIndex = index;
                    Invalidate();
                }
                base.OnMouseDown(e);
            }

            // A bolinha "Personalizada" só aparece depois que o usuário escolhe uma cor na paleta do Windows
            private int Count() => _customColor.HasValue ? Colors.Length + 1 : Colors.Length;

            private int RowHeight() => Theme.Scale(this, 66);

            private float CellWidth() => Width / (float)ColumnsFor(Width);

            private int ColumnsFor(int width)
            {
                var minimum = Theme.Scale(this, 74);
                return Math.Clamp(width / Math.Max(1, minimum), 1, Count());
            }
        }
    }
}
