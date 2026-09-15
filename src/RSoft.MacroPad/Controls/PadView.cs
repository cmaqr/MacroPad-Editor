using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using RSoft.MacroPad.BLL.Infrasturture.Model;
using RSoft.MacroPad.BLL.Infrasturture.Physical;

namespace RSoft.MacroPad.Controls
{
    /// <summary>
    /// Desenho do macropad. Clicar numa tecla ou knob seleciona a ação que vai ser configurada.
    /// </summary>
    internal class PadView : Control
    {
        private KeyboardLayout _layout;
        private InputAction _selectedAction = InputAction.None;
        private IReadOnlyDictionary<InputAction, string> _titles = new Dictionary<InputAction, string>();
        private PhysicalControl _hovered;

        public event EventHandler<InputAction> ActionSelected;

        /// <summary>Limite de ampliação do desenho. A colinha impressa usa um valor bem maior que a tela.</summary>
        public float MaxScale { get; set; } = 4.5f;

        public KeyboardLayout KeyboardLayout
        {
            get => _layout;
            set { _layout = value; Invalidate(); }
        }

        public InputAction SelectedAction
        {
            get => _selectedAction;
            set { _selectedAction = value; Invalidate(); }
        }

        /// <summary>Nome do que já foi enviado para cada ação, mostrado dentro da tecla.</summary>
        public IReadOnlyDictionary<InputAction, string> Titles
        {
            get => _titles;
            set { _titles = value ?? new Dictionary<InputAction, string>(); Invalidate(); }
        }

        public PadView()
        {
            SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw, true);
            SetStyle(ControlStyles.Selectable, false);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            var graphics = e.Graphics;
            graphics.Clear(Parent?.BackColor ?? Theme.Card);
            Theme.PrepareGraphics(graphics);
            if (_layout == null)
                return;

            var (body, scale, shapes) = ComputeShapes();
            Theme.FillRounded(graphics, Theme.DeviceBody, body, 5 * scale);

            foreach (var (bounds, control) in shapes)
            {
                var isSelected = control.Actions.Contains(_selectedAction);
                if (control is PhysicalKnob)
                    PaintKnob(graphics, bounds, control, isSelected);
                else
                    PaintButton(graphics, bounds, control, isSelected, scale);
            }
        }

        private void PaintButton(Graphics graphics, RectangleF bounds, PhysicalControl control, bool isSelected, float scale)
        {
            var fill = KeyFill(control, isSelected);
            // Sombra embaixo da tecla para parecer que ela está elevada
            Theme.FillRounded(graphics, Color.FromArgb(90, 0, 0, 0), new RectangleF(bounds.X, bounds.Y + scale * 0.6f, bounds.Width, bounds.Height), 2.5f * scale);
            Theme.FillRounded(graphics, fill, bounds, 2.5f * scale);

            var foreground = isSelected ? Color.White : Theme.Text;
            var secondary = isSelected ? Color.FromArgb(210, 255, 255, 255) : Theme.TertiaryText;
            var rectangle = Rectangle.Round(bounds);
            var action = control.Actions.First();

            if (!_titles.TryGetValue(action, out var title))
            {
                TextRenderer.DrawText(graphics, control.Name, Theme.KeyNumber, rectangle, isSelected ? Color.White : Theme.SecondaryText,
                    TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter | TextFormatFlags.NoPrefix);
                return;
            }

            var padding = (int)(scale * 1.2f);
            TextRenderer.DrawText(graphics, control.Name, Theme.Caption, new Rectangle(rectangle.X + padding, rectangle.Y + padding / 2, rectangle.Width, rectangle.Height / 3), secondary,
                TextFormatFlags.Left | TextFormatFlags.Top | TextFormatFlags.NoPrefix);
            TextRenderer.DrawText(graphics, title, Theme.CaptionSemibold, new Rectangle(rectangle.X + padding, rectangle.Y + rectangle.Height / 4, rectangle.Width - padding * 2, rectangle.Height * 3 / 4 - padding / 2), foreground,
                TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter | TextFormatFlags.WordBreak | TextFormatFlags.EndEllipsis | TextFormatFlags.NoPrefix);
        }

        private void PaintKnob(Graphics graphics, RectangleF bounds, PhysicalControl control, bool isSelected)
        {
            using (var shadow = new SolidBrush(Color.FromArgb(90, 0, 0, 0)))
                graphics.FillEllipse(shadow, bounds.X, bounds.Y + bounds.Height * 0.03f, bounds.Width, bounds.Height);

            var fill = KeyFill(control, isSelected);
            using (var brush = new SolidBrush(fill))
                graphics.FillEllipse(brush, bounds);

            // Marquinha no topo, como o indicador de um botão giratório
            var notchColor = isSelected ? Color.White : Theme.TertiaryText;
            using (var pen = new Pen(notchColor, Math.Max(2f, bounds.Width * 0.04f)) { StartCap = System.Drawing.Drawing2D.LineCap.Round, EndCap = System.Drawing.Drawing2D.LineCap.Round })
                graphics.DrawLine(pen, bounds.X + bounds.Width / 2, bounds.Y + bounds.Height * 0.1f, bounds.X + bounds.Width / 2, bounds.Y + bounds.Height * 0.24f);

            var rectangle = Rectangle.Round(bounds);
            var third = rectangle.Width / 3;
            var actions = control.Actions.ToArray();
            var foreground = isSelected ? Color.White : Theme.SecondaryText;

            // Setas de girar nos lados; o lado da ação selecionada fica em destaque.
            // Em knob pequeno as setas encostam no número, então só aparecem quando cabem.
            if (rectangle.Width >= Theme.Scale(this, 64))
            {
                var leftColor = ArrowColor(isSelected, _selectedAction == actions[0]);
                var rightColor = ArrowColor(isSelected, _selectedAction == actions[2]);
                TextRenderer.DrawText(graphics, "\uE7A7", Theme.IconSmall, new Rectangle(rectangle.X, rectangle.Y, third, rectangle.Height), leftColor,
                    TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter | TextFormatFlags.NoPadding);
                TextRenderer.DrawText(graphics, "\uE7A6", Theme.IconSmall, new Rectangle(rectangle.X + third * 2, rectangle.Y, third, rectangle.Height), rightColor,
                    TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter | TextFormatFlags.NoPadding);
            }

            TextRenderer.DrawText(graphics, control.Name, Theme.Headline, new Rectangle(rectangle.X + third, rectangle.Y, third, rectangle.Height), foreground,
                TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter | TextFormatFlags.NoPrefix | TextFormatFlags.NoPadding);
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            var hovered = HitTest(e.Location)?.Control;
            if (hovered != _hovered)
            {
                _hovered = hovered;
                Cursor = hovered == null ? Cursors.Default : Cursors.Hand;
                Invalidate();
            }
            base.OnMouseMove(e);
        }

        protected override void OnMouseLeave(EventArgs e)
        {
            _hovered = null;
            Invalidate();
            base.OnMouseLeave(e);
        }

        protected override void OnMouseDown(MouseEventArgs e)
        {
            base.OnMouseDown(e);
            var hit = HitTest(e.Location);
            if (hit == null)
                return;

            var (bounds, control) = hit.Value;
            var actions = control.Actions.ToArray();
            var action = actions[0];
            if (control is PhysicalKnob)
            {
                // Terço esquerdo gira para a esquerda, terço direito para a direita, meio aperta
                var position = (e.X - bounds.X) / bounds.Width;
                if (position < 1f / 3)
                    action = actions[0];
                else if (position > 2f / 3)
                    action = actions[2];
                else
                    action = actions[1];
            }

            SelectedAction = action;
            ActionSelected?.Invoke(this, action);
        }

        private Color KeyFill(PhysicalControl control, bool isSelected)
        {
            if (isSelected)
                return Theme.Accent;
            if (control == _hovered)
                return Theme.Separator;
            return Theme.DeviceKey;
        }

        private static Color ArrowColor(bool knobSelected, bool isSelectedAction)
        {
            if (knobSelected && isSelectedAction)
                return Color.White;
            if (knobSelected)
                return Color.FromArgb(150, 255, 255, 255);
            return Theme.TertiaryText;
        }

        private (RectangleF Bounds, PhysicalControl Control)? HitTest(Point point)
        {
            if (_layout == null)
                return null;

            foreach (var shape in ComputeShapes().Shapes)
            {
                if (shape.Bounds.Contains(point))
                    return shape;
            }
            return null;
        }

        /// <summary>
        /// Converte as posições do layouts.txt (em "mm") para pixels, centralizando o teclado no controle.
        /// </summary>
        private (RectangleF Body, float Scale, List<(RectangleF Bounds, PhysicalControl Control)> Shapes) ComputeShapes()
        {
            const float bodyPadding = 5f;
            const float keyGap = 1.2f;

            var controls = _layout.Controls.ToList();
            var minX = controls.Min(c => c.Position.X) - bodyPadding;
            var minY = controls.Min(c => c.Position.Y) - bodyPadding;
            var maxX = controls.Max(c => c.Position.X + c.Size.X) + bodyPadding;
            var maxY = controls.Max(c => c.Position.Y + c.Size.Y) + bodyPadding;
            var widthInMm = maxX - minX;
            var heightInMm = maxY - minY;

            var scale = Math.Min(Width / (float)widthInMm, Height / (float)heightInMm);
            scale = Math.Min(scale, Theme.Scale(this, MaxScale));
            var offsetX = (Width - widthInMm * scale) / 2;
            var offsetY = (Height - heightInMm * scale) / 2;

            var body = new RectangleF(offsetX, offsetY, widthInMm * scale, heightInMm * scale);
            var shapes = new List<(RectangleF, PhysicalControl)>();
            foreach (var control in controls)
            {
                var bounds = new RectangleF(
                    offsetX + (control.Position.X - minX + keyGap) * scale,
                    offsetY + (control.Position.Y - minY + keyGap) * scale,
                    (control.Size.X - keyGap * 2) * scale,
                    (control.Size.Y - keyGap * 2) * scale);
                shapes.Add((bounds, control));
            }
            return (body, scale, shapes);
        }
    }
}
