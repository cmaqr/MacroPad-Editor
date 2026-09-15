using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Text;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows.Forms;
using Microsoft.Win32;

namespace RSoft.MacroPad.Controls
{
    /// <summary>Cor de texto de um controle comum, escolhida pelo papel e não pelo valor.</summary>
    internal enum TextRole
    {
        Primary,
        Secondary,
        Tertiary,
        Accent,
        Warning,
        Danger,
        Success,
    }

    /// <summary>Cor de fundo sobre a qual o controle está.</summary>
    internal enum SurfaceRole
    {
        Card,
        Background,
        Fill,
        AccentSoft,
        WarningSoft,
    }

    /// <summary>
    /// Cores, fontes e desenho arredondado usados em todo o app (visual claro ou escuro, no estilo Apple).
    /// </summary>
    internal static class Theme
    {
        private sealed class Palette
        {
            public Color Background, Card, Text, SecondaryText, TertiaryText, Separator, Fill, FillHover;
            public Color Accent, AccentHover, AccentPressed, AccentSoft;
            public Color SegmentSelected;
            public Color Success, Danger, Warning, WarningSoft;
            public Color DeviceBody, DeviceKey;
        }

        private static readonly Palette LightPalette = new Palette
        {
            Background = Color.FromArgb(245, 245, 247),
            Card = Color.White,
            Text = Color.FromArgb(29, 29, 31),
            SecondaryText = Color.FromArgb(110, 110, 115),
            TertiaryText = Color.FromArgb(160, 160, 166),
            Separator = Color.FromArgb(229, 229, 234),
            Fill = Color.FromArgb(232, 232, 237),
            FillHover = Color.FromArgb(222, 222, 228),
            SegmentSelected = Color.White,
            Accent = Color.FromArgb(0, 113, 227),
            AccentHover = Color.FromArgb(0, 119, 237),
            AccentPressed = Color.FromArgb(0, 94, 190),
            AccentSoft = Color.FromArgb(232, 242, 253),
            Success = Color.FromArgb(40, 167, 69),
            Danger = Color.FromArgb(215, 0, 21),
            Warning = Color.FromArgb(201, 108, 0),
            WarningSoft = Color.FromArgb(255, 244, 229),
            DeviceBody = Color.FromArgb(58, 58, 60),
            DeviceKey = Color.FromArgb(250, 250, 252),
        };

        private static readonly Palette DarkPalette = new Palette
        {
            Background = Color.FromArgb(26, 26, 28),
            Card = Color.FromArgb(36, 36, 38),
            Text = Color.FromArgb(245, 245, 247),
            SecondaryText = Color.FromArgb(161, 161, 166),
            TertiaryText = Color.FromArgb(120, 120, 125),
            Separator = Color.FromArgb(56, 56, 58),
            Fill = Color.FromArgb(50, 50, 53),
            FillHover = Color.FromArgb(62, 62, 66),
            SegmentSelected = Color.FromArgb(94, 94, 98),
            Accent = Color.FromArgb(10, 132, 255),
            AccentHover = Color.FromArgb(51, 156, 255),
            AccentPressed = Color.FromArgb(0, 102, 204),
            AccentSoft = Color.FromArgb(22, 45, 70),
            Success = Color.FromArgb(48, 209, 88),
            Danger = Color.FromArgb(255, 69, 58),
            Warning = Color.FromArgb(255, 179, 64),
            WarningSoft = Color.FromArgb(58, 46, 23),
            DeviceBody = Color.FromArgb(15, 15, 16),
            DeviceKey = Color.FromArgb(58, 58, 60),
        };

        // Tema é estado do app inteiro: um só, trocado pelo botão do cabeçalho.
        private static Palette _palette = LightPalette;

        public static bool IsDark { get; private set; }

        public static Color Background => _palette.Background;
        public static Color Card => _palette.Card;
        public static Color Text => _palette.Text;
        public static Color SecondaryText => _palette.SecondaryText;
        public static Color TertiaryText => _palette.TertiaryText;
        public static Color Separator => _palette.Separator;
        public static Color Fill => _palette.Fill;
        /// <summary>Fundo do segmento escolhido: branco no claro, cinza claro no escuro.</summary>
        public static Color SegmentSelected => _palette.SegmentSelected;
        public static Color FillHover => _palette.FillHover;
        public static Color Accent => _palette.Accent;
        public static Color AccentHover => _palette.AccentHover;
        public static Color AccentPressed => _palette.AccentPressed;
        public static Color AccentSoft => _palette.AccentSoft;
        public static Color Success => _palette.Success;
        public static Color Danger => _palette.Danger;
        public static Color Warning => _palette.Warning;
        public static Color WarningSoft => _palette.WarningSoft;
        public static Color DeviceBody => _palette.DeviceBody;
        public static Color DeviceKey => _palette.DeviceKey;

        private static readonly string TextFamily = FirstInstalled("Segoe UI Variable Text", "Segoe UI");
        private static readonly string SemiboldFamily = FirstInstalled("Segoe UI Variable Text Semibold", "Segoe UI Semibold");
        private static readonly string IconFamily = FirstInstalled("Segoe Fluent Icons", "Segoe MDL2 Assets");

        // Tamanho em pontos: o Windows já converte para a escala da tela quando o app é DPI-aware
        public static readonly Font LargeTitle = new Font(SemiboldFamily, 20f, FontStyle.Regular, GraphicsUnit.Point);
        public static readonly Font Title = new Font(SemiboldFamily, 13f, FontStyle.Regular, GraphicsUnit.Point);
        public static readonly Font Headline = new Font(SemiboldFamily, 10f, FontStyle.Regular, GraphicsUnit.Point);
        public static readonly Font Body = new Font(TextFamily, 10f, FontStyle.Regular, GraphicsUnit.Point);
        public static readonly Font Callout = new Font(TextFamily, 9f, FontStyle.Regular, GraphicsUnit.Point);
        public static readonly Font CalloutSemibold = new Font(SemiboldFamily, 9f, FontStyle.Regular, GraphicsUnit.Point);
        public static readonly Font Caption = new Font(TextFamily, 8f, FontStyle.Regular, GraphicsUnit.Point);
        public static readonly Font CaptionSemibold = new Font(SemiboldFamily, 8f, FontStyle.Regular, GraphicsUnit.Point);
        public static readonly Font KeyNumber = new Font(SemiboldFamily, 16f, FontStyle.Regular, GraphicsUnit.Point);
        public static readonly Font IconSmall = new Font(IconFamily, 11f, FontStyle.Regular, GraphicsUnit.Point);
        public static readonly Font IconLarge = new Font(IconFamily, 16f, FontStyle.Regular, GraphicsUnit.Point);

        /// <summary>
        /// Ícones da Segoe Fluent Icons ficam na área de uso privado do Unicode (a partir de U+E000).
        /// Qualquer outra coisa é desenhada como texto comum, ex.: "F13".
        /// </summary>
        public static bool IsIconGlyph(string icon) => icon != null && icon.Length == 1 && icon[0] >= '\uE000';

        public static readonly Color[] CategoryColors =
        {
            Color.FromArgb(0, 122, 255),   // azul
            Color.FromArgb(48, 176, 199),  // turquesa
            Color.FromArgb(88, 86, 214),   // índigo
            Color.FromArgb(99, 99, 102),   // grafite
            Color.FromArgb(255, 45, 85),   // rosa
            Color.FromArgb(255, 149, 0),   // laranja
            Color.FromArgb(52, 199, 89),   // verde
            Color.FromArgb(43, 87, 154),   // azul Office
            Color.FromArgb(175, 82, 222),  // roxo
            Color.FromArgb(255, 59, 48),   // vermelho
            Color.FromArgb(162, 132, 94),  // marrom
        };

        // Quem pinta sozinho (PillButton, PadView...) lê as cores na hora de desenhar.
        // Label, TextBox e afins guardam a cor, então o papel de cada um fica anotado aqui
        // para as cores serem reescritas quando o tema muda.
        private static readonly ConditionalWeakTable<Control, ControlRoles> Roles = new ConditionalWeakTable<Control, ControlRoles>();

        private sealed class ControlRoles
        {
            public TextRole Text;
            public SurfaceRole Surface;
        }

        /// <summary>Anota o papel do controle e já aplica as cores do tema atual.</summary>
        public static T Register<T>(T control, TextRole text, SurfaceRole surface) where T : Control
        {
            Roles.Remove(control);
            Roles.Add(control, new ControlRoles { Text = text, Surface = surface });
            control.ForeColor = ColorOf(text);
            control.BackColor = ColorOf(surface);
            return control;
        }

        public static void SetTextRole(Control control, TextRole text)
        {
            if (Roles.TryGetValue(control, out var roles))
                roles.Text = text;
            control.ForeColor = ColorOf(text);
        }

        public static Color ColorOf(TextRole role)
        {
            switch (role)
            {
                case TextRole.Secondary: return SecondaryText;
                case TextRole.Tertiary: return TertiaryText;
                case TextRole.Accent: return Accent;
                case TextRole.Warning: return Warning;
                case TextRole.Danger: return Danger;
                case TextRole.Success: return Success;
                default: return Text;
            }
        }

        public static Color ColorOf(SurfaceRole role)
        {
            switch (role)
            {
                case SurfaceRole.Background: return Background;
                case SurfaceRole.Fill: return Fill;
                case SurfaceRole.AccentSoft: return AccentSoft;
                case SurfaceRole.WarningSoft: return WarningSoft;
                default: return Card;
            }
        }

        /// <summary>Troca entre claro e escuro e repinta tudo a partir da janela.</summary>
        public static void UseDark(bool dark, Control root)
        {
            IsDark = dark;
            _palette = dark ? DarkPalette : LightPalette;
            Refresh(root);
        }

        /// <summary>
        /// Roda um desenho sempre com as cores claras (a colinha é para imprimir em papel branco).
        /// Não mexe na tela porque nada é repintado durante a chamada.
        /// </summary>
        public static void WithLightPalette(Action draw)
        {
            var previous = _palette;
            _palette = LightPalette;
            try
            {
                draw();
            }
            finally
            {
                _palette = previous;
            }
        }

        public static bool WindowsPrefersDark()
        {
            // O Windows guarda a preferência do usuário aqui; qualquer falha cai no tema claro
            var value = Registry.GetValue(@"HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\Themes\Personalize", "AppsUseLightTheme", 1);
            return value is int useLight && useLight == 0;
        }

        private static void Refresh(Control control)
        {
            if (Roles.TryGetValue(control, out var roles))
            {
                control.ForeColor = ColorOf(roles.Text);
                control.BackColor = ColorOf(roles.Surface);
            }
            control.Invalidate(true);

            foreach (Control child in control.Controls)
                Refresh(child);
        }

        public static GraphicsPath RoundedRectangle(RectangleF bounds, float radius)
        {
            var path = new GraphicsPath();
            var diameter = Math.Min(radius * 2, Math.Min(bounds.Width, bounds.Height));
            if (diameter <= 0)
            {
                path.AddRectangle(bounds);
                return path;
            }
            path.AddArc(bounds.X, bounds.Y, diameter, diameter, 180, 90);
            path.AddArc(bounds.Right - diameter, bounds.Y, diameter, diameter, 270, 90);
            path.AddArc(bounds.Right - diameter, bounds.Bottom - diameter, diameter, diameter, 0, 90);
            path.AddArc(bounds.X, bounds.Bottom - diameter, diameter, diameter, 90, 90);
            path.CloseFigure();
            return path;
        }

        public static void PrepareGraphics(Graphics graphics)
        {
            graphics.SmoothingMode = SmoothingMode.AntiAlias;
            graphics.PixelOffsetMode = PixelOffsetMode.HighQuality;
            graphics.TextRenderingHint = TextRenderingHint.ClearTypeGridFit;
        }

        public static void FillRounded(Graphics graphics, Color color, RectangleF bounds, float radius)
        {
            using var brush = new SolidBrush(color);
            using var path = RoundedRectangle(bounds, radius);
            graphics.FillPath(brush, path);
        }

        public static void DrawRounded(Graphics graphics, Color color, float width, RectangleF bounds, float radius)
        {
            using var pen = new Pen(color, width);
            using var path = RoundedRectangle(bounds, radius);
            graphics.DrawPath(pen, path);
        }

        /// <summary>
        /// Converte um tamanho pensado para 100% de escala para a escala atual da tela.
        /// </summary>
        public static int Scale(Control control, int pixels) => (int)Math.Round(pixels * control.DeviceDpi / 96f);

        public static float Scale(Control control, float pixels) => pixels * control.DeviceDpi / 96f;

        private static string FirstInstalled(params string[] families)
        {
            using var installed = new InstalledFontCollection();
            var names = installed.Families.Select(f => f.Name).ToHashSet();
            return families.FirstOrDefault(names.Contains) ?? families.Last();
        }
    }
}
