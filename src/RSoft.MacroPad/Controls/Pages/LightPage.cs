using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using RSoft.MacroPad.BLL.Infrasturture.Model;
using RSoft.MacroPad.BLL.Infrasturture.Physical;

namespace RSoft.MacroPad.Controls.Pages
{
    /// <summary>
    /// Efeito e cor da iluminação do macropad.
    /// </summary>
    internal class LightPage : Panel
    {
        private static readonly (LedColor Value, string Name, Color Swatch)[] Colors =
        {
            (LedColor.Random, "Aleatória", Color.Empty),
            (LedColor.Red, "Vermelho", Color.FromArgb(255, 59, 48)),
            (LedColor.Orange, "Laranja", Color.FromArgb(255, 149, 0)),
            (LedColor.Yellow, "Amarelo", Color.FromArgb(255, 204, 0)),
            (LedColor.Green, "Verde", Color.FromArgb(52, 199, 89)),
            (LedColor.Cyan, "Ciano", Color.FromArgb(50, 173, 230)),
            (LedColor.Blue, "Azul", Color.FromArgb(0, 122, 255)),
            (LedColor.Purple, "Roxo", Color.FromArgb(175, 82, 222)),
        };

        private readonly Label _title;
        private readonly Label _description;
        private readonly Label _modeTitle;
        private readonly SegmentedControl _modes;
        private readonly Label _colorTitle;
        private readonly ColorPicker _colors;
        private readonly Label _noColors;
        private readonly PillButton _applyButton;

        public event EventHandler<(LedMode Mode, LedColor Color)> LightApplyRequested;

        public LightPage()
        {
            Theme.Register(this, TextRole.Primary, SurfaceRole.Card);

            _title = NewLabel("Iluminação", Theme.Title, TextRole.Primary);
            _description = NewLabel("Cada modelo tem efeitos diferentes: teste os modos e veja qual você prefere. No modelo de 3 teclas, um dos modos apaga a luz.", Theme.Callout, TextRole.Secondary);

            _modeTitle = NewLabel("Efeito", Theme.Headline, TextRole.Primary);
            _modes = new SegmentedControl { Height = Theme.Scale(this, 34) };

            _colorTitle = NewLabel("Cor", Theme.Headline, TextRole.Primary);
            _colors = new ColorPicker { Height = Theme.Scale(this, 74) };
            _noColors = NewLabel("Este modelo não permite escolher a cor.", Theme.Callout, TextRole.Tertiary);

            _applyButton = new PillButton { Text = "Aplicar iluminação", Style = PillStyle.Primary, Icon = "\uE793", Height = Theme.Scale(this, 38) };
            _applyButton.FitWidthToText();
            _applyButton.Click += (s, e) => LightApplyRequested?.Invoke(this, ((LedMode)_modes.SelectedIndex, Colors[_colors.SelectedIndex].Value));

            Controls.AddRange(new Control[] { _title, _description, _modeTitle, _modes, _colorTitle, _colors, _noColors, _applyButton });
        }

        public void SetLayout(KeyboardLayout layout)
        {
            _modes.Items = Enumerable.Range(1, Math.Max(1, (int)layout.LedModeCount)).Select(i => $"Modo {i}").ToArray();
            _colors.Visible = layout.SupportsColor;
            _noColors.Visible = !layout.SupportsColor;
            PerformLayout();
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

            _colorTitle.SetBounds(0, _modes.Bottom + Theme.Scale(this, 24), width, Theme.Scale(this, 26));
            _colors.SetBounds(0, _colorTitle.Bottom + gap, width, _colors.Height);
            _noColors.SetBounds(0, _colorTitle.Bottom + gap, width, Theme.Scale(this, 26));

            var buttonTop = (_colors.Visible ? _colors.Bottom : _noColors.Bottom) + Theme.Scale(this, 20);
            _applyButton.Location = new Point(0, buttonTop);
        }

        private static Label NewLabel(string text, Font font, TextRole role)
        {
            return Theme.Register(new Label { Text = text, Font = font, UseMnemonic = false }, role, SurfaceRole.Card);
        }

        /// <summary>Bolinhas de cor com o nome embaixo; a selecionada ganha um anel azul.</summary>
        private sealed class ColorPicker : Control
        {
            public int SelectedIndex { get; private set; }

            public ColorPicker()
            {
                SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw, true);
                Cursor = Cursors.Hand;
            }

            protected override void OnPaint(PaintEventArgs e)
            {
                var graphics = e.Graphics;
                graphics.Clear(Parent?.BackColor ?? Theme.Card);
                Theme.PrepareGraphics(graphics);

                var cell = CellWidth();
                var diameter = Theme.Scale(this, 34f);
                for (var i = 0; i < Colors.Length; i++)
                {
                    var circle = new RectangleF(i * cell + (cell - diameter) / 2, Theme.Scale(this, 5f), diameter, diameter);

                    if (Colors[i].Swatch == Color.Empty)
                    {
                        // "Aleatória" é desenhada como uma pizza com quatro cores
                        var slices = new[] { Colors[1].Swatch, Colors[4].Swatch, Colors[6].Swatch, Colors[3].Swatch };
                        for (var s = 0; s < slices.Length; s++)
                        {
                            using var brush = new SolidBrush(slices[s]);
                            graphics.FillPie(brush, circle.X, circle.Y, circle.Width, circle.Height, s * 90, 90);
                        }
                    }
                    else
                    {
                        using var brush = new SolidBrush(Colors[i].Swatch);
                        graphics.FillEllipse(brush, circle);
                    }

                    if (i == SelectedIndex)
                    {
                        var ringWidth = Theme.Scale(this, 2.5f);
                        var ring = RectangleF.Inflate(circle, ringWidth * 1.6f, ringWidth * 1.6f);
                        using var pen = new Pen(Theme.Accent, ringWidth);
                        graphics.DrawEllipse(pen, ring);
                    }

                    var labelBounds = new Rectangle((int)(i * cell), (int)(circle.Bottom + Theme.Scale(this, 6f)), (int)cell, Theme.Scale(this, 22));
                    TextRenderer.DrawText(graphics, Colors[i].Name, i == SelectedIndex ? Theme.CaptionSemibold : Theme.Caption, labelBounds,
                        i == SelectedIndex ? Theme.Text : Theme.SecondaryText, TextFormatFlags.HorizontalCenter | TextFormatFlags.Top | TextFormatFlags.NoPrefix);
                }
            }

            protected override void OnMouseDown(MouseEventArgs e)
            {
                SelectedIndex = Math.Clamp((int)(e.X / CellWidth()), 0, Colors.Length - 1);
                Invalidate();
                base.OnMouseDown(e);
            }

            private float CellWidth() => Math.Min(Width / (float)Colors.Length, Theme.Scale(this, 76f));
        }
    }
}
