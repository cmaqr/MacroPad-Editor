using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace RSoft.MacroPad.Controls
{
    /// <summary>
    /// Botões lado a lado onde só um fica selecionado, como as abas do iOS.
    /// </summary>
    internal class SegmentedControl : Control
    {
        private IReadOnlyList<string> _items = Array.Empty<string>();
        private int _selectedIndex;
        private int _hoveredIndex = -1;

        public event EventHandler SelectedIndexChanged;

        public IReadOnlyList<string> Items
        {
            get => _items;
            set
            {
                _items = value ?? Array.Empty<string>();
                _selectedIndex = Math.Min(_selectedIndex, Math.Max(0, _items.Count - 1));
                Invalidate();
            }
        }

        public int SelectedIndex
        {
            get => _selectedIndex;
            set
            {
                if (value == _selectedIndex || value < 0 || value >= _items.Count)
                    return;
                _selectedIndex = value;
                Invalidate();
                SelectedIndexChanged?.Invoke(this, EventArgs.Empty);
            }
        }

        public SegmentedControl()
        {
            SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw, true);
            SetStyle(ControlStyles.Selectable, false);
            Cursor = Cursors.Hand;
            Font = Theme.Callout;
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            var graphics = e.Graphics;
            graphics.Clear(Parent?.BackColor ?? Theme.Background);
            Theme.PrepareGraphics(graphics);

            var radius = Theme.Scale(this, 9f);
            Theme.FillRounded(graphics, Theme.Fill, new RectangleF(0, 0, Width, Height), radius);
            if (_items.Count == 0)
                return;

            var inset = Theme.Scale(this, 2f);
            var segmentWidth = (Width - inset * 2) / _items.Count;

            for (var i = 0; i < _items.Count; i++)
            {
                var segment = new RectangleF(inset + i * segmentWidth, inset, segmentWidth, Height - inset * 2);

                if (i == _selectedIndex)
                {
                    // Sombra de 1px embaixo do segmento branco para dar a sensação de relevo
                    Theme.FillRounded(graphics, Color.FromArgb(28, 0, 0, 0), new RectangleF(segment.X, segment.Y + 1, segment.Width, segment.Height), radius - inset);
                    Theme.FillRounded(graphics, Theme.SegmentSelected, segment, radius - inset);
                }
                else if (i == _hoveredIndex)
                {
                    Theme.FillRounded(graphics, Theme.FillHover, segment, radius - inset);
                }

                var font = i == _selectedIndex ? Theme.CalloutSemibold : Font;
                TextRenderer.DrawText(graphics, _items[i], font, Rectangle.Round(segment), Theme.Text,
                    TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter | TextFormatFlags.NoPrefix | TextFormatFlags.EndEllipsis);
            }
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            var index = IndexAt(e.X);
            if (index != _hoveredIndex)
            {
                _hoveredIndex = index;
                Invalidate();
            }
            base.OnMouseMove(e);
        }

        protected override void OnMouseLeave(EventArgs e)
        {
            _hoveredIndex = -1;
            Invalidate();
            base.OnMouseLeave(e);
        }

        protected override void OnMouseDown(MouseEventArgs e)
        {
            var index = IndexAt(e.X);
            if (index >= 0)
                SelectedIndex = index;
            base.OnMouseDown(e);
        }

        private int IndexAt(int x)
        {
            if (_items.Count == 0 || Width <= 0)
                return -1;
            return Math.Clamp(x * _items.Count / Width, 0, _items.Count - 1);
        }
    }
}
