using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using RSoft.MacroPad.BLL.Infrasturture.Physical;
using RSoft.MacroPad.BLL.Macros;

namespace RSoft.MacroPad.Controls.Pages
{
    /// <summary>
    /// Kits que configuram todas as teclas e knobs de uma vez.
    /// </summary>
    internal class KitsPage : Panel
    {
        private readonly Label _title;
        private readonly Label _description;
        private readonly FlowLayoutPanel _list;
        private readonly List<(MacroKit Kit, RoundedPanel Card, Label ButtonsPreview, Label KnobsPreview, PillButton Apply)> _cards = new List<(MacroKit, RoundedPanel, Label, Label, PillButton)>();

        public event EventHandler<MacroKit> KitApplyRequested;

        public KitsPage()
        {
            Theme.Register(this, TextRole.Primary, SurfaceRole.Card);

            _title = NewLabel("Kits prontos", Theme.Title, TextRole.Primary, SurfaceRole.Card);
            _description = NewLabel("Um clique configura todas as teclas e knobs da camada escolhida. Depois você troca só o que quiser.", Theme.Callout, TextRole.Secondary, SurfaceRole.Card);

            _list = Theme.Register(new FlowLayoutPanel { AutoScroll = true, WrapContents = false, FlowDirection = FlowDirection.TopDown }, TextRole.Primary, SurfaceRole.Card);
            var colorIndex = 0;
            foreach (var kit in MacroCatalog.Kits)
            {
                var iconColor = Theme.CategoryColors[colorIndex++ % Theme.CategoryColors.Length];
                _cards.Add(CreateCard(kit, iconColor));
            }
            _list.Controls.AddRange(_cards.Select(c => (Control)c.Card).ToArray());
            _list.Resize += (s, e) => ResizeCards();

            Controls.AddRange(new Control[] { _title, _description, _list });
        }

        /// <summary>Atualiza a prévia de cada kit para os botões e knobs do modelo selecionado.</summary>
        public void SetLayout(KeyboardLayout layout, byte layer, bool hasLayers)
        {
            foreach (var (kit, _, buttonsPreview, knobsPreview, apply) in _cards)
            {
                var plan = KitPlanner.Plan(kit, layout);
                var buttons = plan.Where(p => layout.Controls.OfType<PhysicalButton>().Any(b => b.Actions.Contains(p.Action))).Select(p => p.Macro.Title);
                var knobs = kit.KnobMacroIds.Take(layout.Controls.OfType<PhysicalKnob>().Count())
                    .Select(k => $"{MacroCatalog.Find(k.Left).Title} / {MacroCatalog.Find(k.Push).Title} / {MacroCatalog.Find(k.Right).Title}");

                buttonsPreview.Text = "Teclas: " + string.Join(", ", buttons);
                knobsPreview.Text = knobs.Any() ? "Knobs: " + string.Join("   ·   ", knobs) : "";

                apply.Text = hasLayers ? $"Aplicar na camada {layer}" : "Aplicar kit";
                apply.FitWidthToText();
            }
            ResizeCards();
        }

        protected override void OnLayout(LayoutEventArgs levent)
        {
            base.OnLayout(levent);
            if (_list == null)
                return;

            var width = ClientSize.Width;
            _title.SetBounds(0, 0, width, Theme.Scale(this, 30));
            _description.SetBounds(0, _title.Bottom, width, Theme.Scale(this, 40));
            var listTop = _description.Bottom + Theme.Scale(this, 8);
            _list.SetBounds(0, listTop, width, Math.Max(0, ClientSize.Height - listTop));
        }

        private (MacroKit, RoundedPanel, Label, Label, PillButton) CreateCard(MacroKit kit, Color iconColor)
        {
            var card = new RoundedPanel { Surface = SurfaceRole.Background, Radius = 14, Margin = new Padding(0, 0, 0, Theme.Scale(this, 10)) };

            var icon = new IconBadge { Glyph = kit.Icon, BadgeColor = iconColor };
            var title = NewLabel(kit.Title, Theme.Headline, TextRole.Primary, SurfaceRole.Background);
            var description = NewLabel(kit.Description, Theme.Callout, TextRole.Secondary, SurfaceRole.Background);
            var buttonsPreview = NewLabel("", Theme.Caption, TextRole.Tertiary, SurfaceRole.Background);
            var knobsPreview = NewLabel("", Theme.Caption, TextRole.Tertiary, SurfaceRole.Background);
            var apply = new PillButton { Text = "Aplicar kit", Style = PillStyle.Primary, Font = Theme.CalloutSemibold, Height = Theme.Scale(this, 32) };
            apply.FitWidthToText();
            apply.Click += (s, e) => KitApplyRequested?.Invoke(this, kit);

            card.Controls.AddRange(new Control[] { icon, title, description, buttonsPreview, knobsPreview, apply });
            card.Layout += (s, e) =>
            {
                var padding = Theme.Scale(this, 16);
                icon.SetBounds(padding, padding, Theme.Scale(this, 44), Theme.Scale(this, 44));
                apply.Location = new Point(card.Width - apply.Width - padding, padding + Theme.Scale(this, 6));
                var textLeft = icon.Right + Theme.Scale(this, 14);
                var textWidth = apply.Left - textLeft - Theme.Scale(this, 12);
                title.SetBounds(textLeft, padding - Theme.Scale(this, 2), textWidth, Theme.Scale(this, 24));
                description.SetBounds(textLeft, title.Bottom, textWidth, Theme.Scale(this, 38));
                var previewWidth = card.Width - textLeft - padding;
                buttonsPreview.SetBounds(textLeft, description.Bottom + Theme.Scale(this, 2), previewWidth, Theme.Scale(this, 17));
                knobsPreview.SetBounds(textLeft, buttonsPreview.Bottom, previewWidth, Theme.Scale(this, 17));
            };
            return (kit, card, buttonsPreview, knobsPreview, apply);
        }

        private void ResizeCards()
        {
            var width = _list.ClientSize.Width - SystemInformation.VerticalScrollBarWidth - Theme.Scale(this, 2);
            foreach (var (_, card, _, _, _) in _cards)
                card.Size = new Size(Math.Max(Theme.Scale(this, 300), width), Theme.Scale(this, 132));
        }

        private static Label NewLabel(string text, Font font, TextRole role, SurfaceRole surface)
        {
            return Theme.Register(new Label { Text = text, Font = font, UseMnemonic = false, AutoEllipsis = true }, role, surface);
        }

        /// <summary>Quadrado colorido com um ícone branco no meio.</summary>
        private sealed class IconBadge : Control
        {
            public string Glyph { get; set; }
            public Color BadgeColor { get; set; }

            public IconBadge()
            {
                SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw, true);
            }

            protected override void OnPaint(PaintEventArgs e)
            {
                e.Graphics.Clear(Parent?.BackColor ?? Theme.Background);
                Theme.PrepareGraphics(e.Graphics);
                Theme.FillRounded(e.Graphics, BadgeColor, new RectangleF(0, 0, Width, Height), Width * 0.26f);
                TextRenderer.DrawText(e.Graphics, Glyph, Theme.IconLarge, ClientRectangle, Color.White,
                    TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter | TextFormatFlags.NoPadding);
            }
        }
    }
}
