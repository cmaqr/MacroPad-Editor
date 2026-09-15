using System;
using System.Drawing;
using System.Windows.Forms;
using RSoft.MacroPad.BLL.Macros;

namespace RSoft.MacroPad.Controls
{
    /// <summary>
    /// Bloco clicável de uma macro pronta: ícone colorido, nome e atalho.
    /// </summary>
    internal class MacroTile : Control
    {
        private bool _selected;
        private bool _available = true;
        private bool _hovered;

        public Macro Macro { get; }
        public Color IconColor { get; }

        public bool Selected
        {
            get => _selected;
            set { _selected = value; Invalidate(); }
        }

        /// <summary>False quando a macro tem mais teclas do que o modelo selecionado aceita.</summary>
        public bool Available
        {
            get => _available;
            set { _available = value; Cursor = value ? Cursors.Hand : Cursors.No; Invalidate(); }
        }

        public MacroTile(Macro macro, Color iconColor)
        {
            Macro = macro;
            IconColor = iconColor;
            SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw, true);
            SetStyle(ControlStyles.Selectable, false);
            Cursor = Cursors.Hand;
            Margin = new Padding(0, 0, Theme.Scale(this, 10), Theme.Scale(this, 10));
            Size = new Size(Theme.Scale(this, 220), Theme.Scale(this, 64));
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            var graphics = e.Graphics;
            graphics.Clear(Parent?.BackColor ?? Theme.Card);
            Theme.PrepareGraphics(graphics);

            var bounds = new RectangleF(1, 1, Width - 2, Height - 2);
            var radius = Theme.Scale(this, 12f);
            var fill = Theme.Background;
            if (_selected)
                fill = Theme.AccentSoft;
            else if (_hovered && _available)
                fill = Theme.Fill;
            Theme.FillRounded(graphics, fill, bounds, radius);
            if (_selected)
                Theme.DrawRounded(graphics, Theme.Accent, Theme.Scale(this, 2f), bounds, radius);

            var iconSize = Theme.Scale(this, 38);
            var padding = Theme.Scale(this, 13);
            var iconBounds = new Rectangle(padding, (Height - iconSize) / 2, iconSize, iconSize);
            var iconColor = _available ? IconColor : Theme.TertiaryText;
            Theme.FillRounded(graphics, iconColor, iconBounds, Theme.Scale(this, 10f));

            var iconFont = Theme.IsIconGlyph(Macro.Icon) ? Theme.IconSmall : Theme.CaptionSemibold;
            TextRenderer.DrawText(graphics, Macro.Icon, iconFont, iconBounds, Color.White,
                TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter | TextFormatFlags.NoPadding | TextFormatFlags.NoPrefix);

            var textLeft = iconBounds.Right + Theme.Scale(this, 11);
            var textWidth = Width - textLeft - padding;
            var titleBounds = new Rectangle(textLeft, Theme.Scale(this, 12), textWidth, Theme.Scale(this, 22));
            var subtitleBounds = new Rectangle(textLeft, Theme.Scale(this, 33), textWidth, Theme.Scale(this, 20));

            TextRenderer.DrawText(graphics, Macro.Title, Theme.Headline, titleBounds, _available ? Theme.Text : Theme.TertiaryText,
                TextFormatFlags.Left | TextFormatFlags.VerticalCenter | TextFormatFlags.EndEllipsis | TextFormatFlags.NoPrefix | TextFormatFlags.NoPadding);

            var subtitle = _available ? ShortcutText.Describe(Macro) : "Grande demais para este modelo";
            TextRenderer.DrawText(graphics, subtitle, Theme.Caption, subtitleBounds, Theme.SecondaryText,
                TextFormatFlags.Left | TextFormatFlags.VerticalCenter | TextFormatFlags.EndEllipsis | TextFormatFlags.NoPrefix | TextFormatFlags.NoPadding);
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
            Invalidate();
            base.OnMouseLeave(e);
        }
    }
}
