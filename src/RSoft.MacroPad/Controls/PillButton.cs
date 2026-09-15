using System;
using System.Drawing;
using System.Windows.Forms;

namespace RSoft.MacroPad.Controls
{
    internal enum PillStyle
    {
        /// <summary>Azul cheio: a ação principal da tela.</summary>
        Primary,
        /// <summary>Cinza claro: ações secundárias.</summary>
        Secondary,
        /// <summary>Só o texto azul, sem fundo.</summary>
        Plain,
        /// <summary>Filtro: cinza quando solto, escuro quando selecionado.</summary>
        Chip,
    }

    /// <summary>
    /// Botão com cantos totalmente arredondados.
    /// </summary>
    internal class PillButton : Control
    {
        private PillStyle _style = PillStyle.Secondary;
        private bool _selected;
        private bool _hovered;
        private bool _pressed;

        public PillStyle Style
        {
            get => _style;
            set { _style = value; Invalidate(); }
        }

        public bool Selected
        {
            get => _selected;
            set { _selected = value; Invalidate(); }
        }

        /// <summary>Ícone opcional da Segoe Fluent Icons desenhado antes do texto.</summary>
        public string Icon { get; set; }

        /// <summary>Quando true, o ícone vai depois do texto (ex.: setinha de menu).</summary>
        public bool IconAfterText { get; set; }

        public PillButton()
        {
            SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer
                | ControlStyles.ResizeRedraw | ControlStyles.SupportsTransparentBackColor, true);
            SetStyle(ControlStyles.Selectable, false);
            Cursor = Cursors.Hand;
            Font = Theme.Headline;
        }

        /// <summary>
        /// Ajusta a largura ao texto, mantendo a altura atual.
        /// </summary>
        public void FitWidthToText()
        {
            var textWidth = TextRenderer.MeasureText(Text, Font, Size.Empty, TextFormatFlags.NoPadding).Width;
            var iconWidth = string.IsNullOrEmpty(Icon) ? 0 : Theme.Scale(this, 22);
            Width = textWidth + iconWidth + Theme.Scale(this, Style == PillStyle.Plain ? 8 : 32);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            var graphics = e.Graphics;
            graphics.Clear(Parent?.BackColor ?? Theme.Background);
            Theme.PrepareGraphics(graphics);

            var (fill, foreground) = PickColors();
            if (fill != Color.Empty)
                Theme.FillRounded(graphics, fill, new RectangleF(0, 0, Width, Height), Height / 2f);

            var textBounds = ClientRectangle;
            if (!string.IsNullOrEmpty(Icon))
            {
                // Ícone e texto centralizados juntos, como um bloco só
                var textWidth = TextRenderer.MeasureText(Text, Font, Size.Empty, TextFormatFlags.NoPadding).Width;
                var iconWidth = Theme.Scale(this, 22);
                var left = (Width - textWidth - iconWidth) / 2;
                var iconLeft = IconAfterText ? left + textWidth + Theme.Scale(this, 8) : left;
                var textLeft = IconAfterText ? left : left + iconWidth;
                TextRenderer.DrawText(graphics, Icon, Theme.IconSmall, new Rectangle(iconLeft, 0, iconWidth, Height), foreground,
                    TextFormatFlags.VerticalCenter | TextFormatFlags.Left | TextFormatFlags.NoPadding);
                textBounds = new Rectangle(textLeft, 0, textWidth + 2, Height);
            }

            TextRenderer.DrawText(graphics, Text, Font, textBounds, foreground,
                TextFormatFlags.VerticalCenter | TextFormatFlags.HorizontalCenter | TextFormatFlags.NoPadding | TextFormatFlags.NoPrefix | TextFormatFlags.EndEllipsis);
        }

        private (Color Fill, Color Foreground) PickColors()
        {
            if (!Enabled)
            {
                return Style == PillStyle.Plain ? (Color.Empty, Theme.TertiaryText) : (Theme.Fill, Theme.TertiaryText);
            }

            switch (Style)
            {
                case PillStyle.Primary:
                    if (_pressed)
                        return (Theme.AccentPressed, Color.White);
                    if (_hovered)
                        return (Theme.AccentHover, Color.White);
                    return (Theme.Accent, Color.White);
                case PillStyle.Plain:
                    return (Color.Empty, _pressed ? Theme.AccentPressed : Theme.Accent);
                case PillStyle.Chip:
                    // Azul quando escolhido: no tema escuro o texto do app já é quase branco,
                    // então usar a cor do texto como fundo sumiria com o rótulo
                    if (_selected)
                        return (Theme.Accent, Color.White);
                    return (_hovered ? Theme.FillHover : Theme.Fill, Theme.Text);
                default:
                    return (_pressed || _hovered ? Theme.FillHover : Theme.Fill, Theme.Text);
            }
        }

        protected override void OnMouseEnter(EventArgs e)
        {
            _hovered = true;
            Invalidate();
            base.OnMouseEnter(e);
        }

        protected override void OnMouseLeave(EventArgs e)
        {
            _hovered = false;
            _pressed = false;
            Invalidate();
            base.OnMouseLeave(e);
        }

        protected override void OnMouseDown(MouseEventArgs e)
        {
            _pressed = true;
            Invalidate();
            base.OnMouseDown(e);
        }

        protected override void OnMouseUp(MouseEventArgs e)
        {
            _pressed = false;
            Invalidate();
            base.OnMouseUp(e);
        }

        protected override void OnEnabledChanged(EventArgs e)
        {
            Cursor = Enabled ? Cursors.Hand : Cursors.Default;
            Invalidate();
            base.OnEnabledChanged(e);
        }

        protected override void OnTextChanged(EventArgs e)
        {
            Invalidate();
            base.OnTextChanged(e);
        }
    }
}
