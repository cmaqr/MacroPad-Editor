using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using RSoft.MacroPad.BLL.Infrasturture.Model;
using RSoft.MacroPad.BLL.Infrasturture.Physical;

namespace RSoft.MacroPad.Controls
{
    /// <summary>
    /// Gera uma imagem com o desenho do teclado e a lista do que cada tecla faz,
    /// para imprimir, colar na mesa ou usar de papel de parede.
    /// </summary>
    internal static class CheatSheet
    {
        public static string Save(KeyboardLayout layout, byte layer, IReadOnlyDictionary<InputAction, string> titles, string folder)
        {
            string filePath = null;
            Theme.WithLightPalette(() => filePath = Draw(layout, layer, titles, folder));
            return filePath;
        }

        private static string Draw(KeyboardLayout layout, byte layer, IReadOnlyDictionary<InputAction, string> titles, string folder)
        {
            const int width = 1400;
            const int padding = 60;
            const int padHeight = 430;

            var lines = DescribeActions(layout, titles).ToList();
            var lineHeight = 34;
            var columns = lines.Count > 12 ? 2 : 1;
            var rowsPerColumn = (int)Math.Ceiling(lines.Count / (double)columns);
            var height = padding * 2 + 120 + padHeight + rowsPerColumn * lineHeight + 60;

            using var bitmap = new Bitmap(width, height);
            using (var graphics = Graphics.FromImage(bitmap))
            {
                graphics.Clear(Color.White);
                Theme.PrepareGraphics(graphics);
                // ClearType deixa franja colorida numa imagem para imprimir
                graphics.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAliasGridFit;

                var deviceName = layout.Name.Replace("buttons", "teclas").Replace("button", "tecla").Trim();
                var subtitle = layout.LayerCount > 1 ? $"Camada {layer}" : "Configuração atual";
                TextRenderer.DrawText(graphics, deviceName, new Font(Theme.LargeTitle.FontFamily, 30f), new Point(padding, padding), Color.Black);
                TextRenderer.DrawText(graphics, subtitle, new Font(Theme.Body.FontFamily, 15f), new Point(padding, padding + 52), Color.Gray);

                DrawPad(graphics, layout, layer, titles, new Rectangle(padding, padding + 110, width - padding * 2, padHeight));

                var top = padding + 110 + padHeight;
                var columnWidth = (width - padding * 2) / columns;
                for (var i = 0; i < lines.Count; i++)
                {
                    var column = i / rowsPerColumn;
                    var row = i % rowsPerColumn;
                    var bounds = new Rectangle(padding + column * columnWidth, top + row * lineHeight, columnWidth - 20, lineHeight);
                    TextRenderer.DrawText(graphics, lines[i], new Font(Theme.Body.FontFamily, 12f), bounds, Color.FromArgb(40, 40, 40),
                        TextFormatFlags.Left | TextFormatFlags.VerticalCenter | TextFormatFlags.EndEllipsis);
                }

                TextRenderer.DrawText(graphics, $"Gerado pelo MacroPad em {DateTime.Now:dd/MM/yyyy}", new Font(Theme.Caption.FontFamily, 10f),
                    new Point(padding, height - padding / 2 - 20), Color.Silver);
            }

            Directory.CreateDirectory(folder);
            var filePath = Path.Combine(folder, $"colinha-{DateTime.Now:yyyy-MM-dd-HHmm}.png");
            bitmap.Save(filePath, ImageFormat.Png);
            return filePath;
        }

        /// <summary>
        /// Desenha o teclado reaproveitando o mesmo controle da tela, fora da janela.
        /// </summary>
        private static void DrawPad(Graphics graphics, KeyboardLayout layout, byte layer, IReadOnlyDictionary<InputAction, string> titles, Rectangle bounds)
        {
            using var pad = new PadView
            {
                Size = bounds.Size,
                MaxScale = 16f,
                KeyboardLayout = layout,
                Titles = titles,
                SelectedAction = InputAction.None,
            };
            using var padImage = new Bitmap(bounds.Width, bounds.Height);
            pad.DrawToBitmap(padImage, new Rectangle(Point.Empty, bounds.Size));
            graphics.DrawImage(padImage, bounds.Location);
        }

        private static IEnumerable<string> DescribeActions(KeyboardLayout layout, IReadOnlyDictionary<InputAction, string> titles)
        {
            foreach (var control in layout.Controls.OrderBy(c => c is PhysicalKnob).ThenBy(c => c.Actions.First()))
            {
                var actions = control.Actions.ToArray();
                if (control is PhysicalButton)
                {
                    yield return $"Tecla {control.Name}   —   {TitleOf(titles, actions[0])}";
                    continue;
                }

                yield return $"Knob {control.Name} girar à esquerda   —   {TitleOf(titles, actions[0])}";
                yield return $"Knob {control.Name} apertar   —   {TitleOf(titles, actions[1])}";
                yield return $"Knob {control.Name} girar à direita   —   {TitleOf(titles, actions[2])}";
            }
        }

        private static string TitleOf(IReadOnlyDictionary<InputAction, string> titles, InputAction action)
        {
            return titles.TryGetValue(action, out var title) ? title : "(livre)";
        }
    }
}
