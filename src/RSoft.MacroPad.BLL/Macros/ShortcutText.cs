using System.Collections.Generic;
using System.Linq;
using RSoft.MacroPad.BLL.Infrasturture.Model;

namespace RSoft.MacroPad.BLL.Macros
{
    /// <summary>
    /// Monta o texto legível de um atalho, ex.: "Ctrl + Shift + T" ou "Ctrl + K  ›  S".
    /// </summary>
    public static class ShortcutText
    {
        public const string ChordSeparator = "  ›  ";

        public static string Describe(Macro macro)
        {
            switch (macro.Kind)
            {
                case MacroKind.Media:
                    return "Tecla de mídia";
                case MacroKind.Mouse:
                    var modifiers = DescribeModifiers(macro.MouseModifiers);
                    var button = DescribeMouseButton(macro.MouseButton);
                    return modifiers.Count == 0 ? button : string.Join(" + ", modifiers) + " + " + button;
                default:
                    return Describe(macro.Keys);
            }
        }

        public static string Describe(IEnumerable<(KeyCode Key, Modifier Modifiers)> keys)
        {
            return string.Join(ChordSeparator, keys.Select(chord => DescribeChord(chord.Key, chord.Modifiers)));
        }

        public static string DescribeChord(KeyCode key, Modifier modifiers)
        {
            var parts = DescribeModifiers(modifiers);
            if (key != KeyCode.None)
                parts.Add(DescribeKey(key));
            return string.Join(" + ", parts);
        }

        private static List<string> DescribeModifiers(Modifier modifiers)
        {
            // Esquerdo e direito aparecem com o mesmo nome: para quem usa, Ctrl é Ctrl
            var parts = new List<string>();
            if ((modifiers & (Modifier.LeftCtrl | Modifier.RightCtrl)) != 0) parts.Add("Ctrl");
            if ((modifiers & (Modifier.LeftShift | Modifier.RightShift)) != 0) parts.Add("Shift");
            if ((modifiers & (Modifier.LeftAlt | Modifier.RightAlt)) != 0) parts.Add("Alt");
            if ((modifiers & (Modifier.LeftWin | Modifier.RightWin)) != 0) parts.Add("Win");
            return parts;
        }

        private static string DescribeKey(KeyCode key)
        {
            switch (key)
            {
                case KeyCode.D0: return "0";
                case KeyCode.D1: return "1";
                case KeyCode.D2: return "2";
                case KeyCode.D3: return "3";
                case KeyCode.D4: return "4";
                case KeyCode.D5: return "5";
                case KeyCode.D6: return "6";
                case KeyCode.D7: return "7";
                case KeyCode.D8: return "8";
                case KeyCode.D9: return "9";
                case KeyCode.SpaceKey: return "Espaço";
                case KeyCode.Minus: return "-";
                case KeyCode.Plus: return "=";
                case KeyCode.Period: return ".";
                case KeyCode.Clear: return ",";
                case KeyCode.Del: return "Delete";
                case KeyCode.PgUp: return "Page Up";
                case KeyCode.PgDn: return "Page Down";
                case KeyCode.PrtSc: return "Print Screen";
                case KeyCode.ArrowLeft: return "←";
                case KeyCode.ArrowRight: return "→";
                case KeyCode.ArrowUp: return "↑";
                case KeyCode.ArrowDown: return "↓";
                case KeyCode.NumEnter: return "Enter (numérico)";
                default: return key.ToString();
            }
        }

        private static string DescribeMouseButton(MouseButton button)
        {
            switch (button)
            {
                case MouseButton.Left: return "Clique";
                case MouseButton.Right: return "Clique direito";
                case MouseButton.Middle: return "Clique do meio";
                case MouseButton.ScrollUp: return "Rolar ↑";
                default: return "Rolar ↓";
            }
        }
    }
}
