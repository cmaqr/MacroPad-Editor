using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Linq;
using RSoft.MacroPad.BLL.Infrasturture.Model;

namespace RSoft.MacroPad.BLL.Macros
{
    /// <summary>
    /// Lê um atalho escrito à mão, como "Ctrl + Shift + T" ou "Ctrl + K > S".
    /// </summary>
    public static class ShortcutParser
    {
        private static readonly char[] ChordSeparators = { '>', '›' };

        private static readonly Dictionary<string, KeyCode> Aliases = new Dictionary<string, KeyCode>
        {
            { "0", KeyCode.D0 }, { "1", KeyCode.D1 }, { "2", KeyCode.D2 }, { "3", KeyCode.D3 }, { "4", KeyCode.D4 },
            { "5", KeyCode.D5 }, { "6", KeyCode.D6 }, { "7", KeyCode.D7 }, { "8", KeyCode.D8 }, { "9", KeyCode.D9 },
            { "espaco", KeyCode.SpaceKey }, { "space", KeyCode.SpaceKey },
            { "escape", KeyCode.Esc },
            { "delete", KeyCode.Del }, { "deletar", KeyCode.Del },
            { "insert", KeyCode.Insert }, { "ins", KeyCode.Insert },
            { "pageup", KeyCode.PgUp }, { "page up", KeyCode.PgUp },
            { "pagedown", KeyCode.PgDn }, { "page down", KeyCode.PgDn },
            { "printscreen", KeyCode.PrtSc }, { "print screen", KeyCode.PrtSc },
            { "scrolllock", KeyCode.ScrollLock }, { "numlock", KeyCode.Num },
            { "pause", KeyCode.PauseBreak }, { "break", KeyCode.PauseBreak },
            { "backspace", KeyCode.Backspace },
            { "esquerda", KeyCode.ArrowLeft }, { "left", KeyCode.ArrowLeft }, { "←", KeyCode.ArrowLeft },
            { "direita", KeyCode.ArrowRight }, { "right", KeyCode.ArrowRight }, { "→", KeyCode.ArrowRight },
            { "cima", KeyCode.ArrowUp }, { "up", KeyCode.ArrowUp }, { "↑", KeyCode.ArrowUp },
            { "baixo", KeyCode.ArrowDown }, { "down", KeyCode.ArrowDown }, { "↓", KeyCode.ArrowDown },
            { ".", KeyCode.Period }, { ",", KeyCode.Clear }, { "-", KeyCode.Minus }, { "=", KeyCode.Plus },
            { "+", KeyCode.Plus }, { "/", KeyCode.Question }, { ";", KeyCode.Colon }, { "[", KeyCode.OpenBracket },
            { "]", KeyCode.CloseBracket }, { "\\", KeyCode.Pipe },
        };

        public static bool TryParse(string text, out List<(KeyCode Key, Modifier Modifiers)> chords)
        {
            chords = new List<(KeyCode, Modifier)>();
            if (string.IsNullOrWhiteSpace(text))
                return false;

            foreach (var chordText in text.Split(ChordSeparators, StringSplitOptions.RemoveEmptyEntries))
            {
                if (!TryParseChord(chordText, out var chord))
                    return false;
                chords.Add(chord);
            }
            return chords.Count > 0;
        }

        private static bool TryParseChord(string text, out (KeyCode Key, Modifier Modifiers) chord)
        {
            chord = (KeyCode.None, Modifier.None);
            var modifiers = Modifier.None;
            var key = KeyCode.None;

            foreach (var rawPart in text.Split('+'))
            {
                var part = Normalize(rawPart);
                if (part.Length == 0)
                {
                    // "Ctrl + +" quer dizer a tecla "+", que some ao separar o texto pelo "+"
                    if (key == KeyCode.None)
                        key = KeyCode.Plus;
                    continue;
                }

                var modifier = ModifierOf(part);
                if (modifier != Modifier.None)
                {
                    modifiers |= modifier;
                    continue;
                }

                if (key != KeyCode.None)
                    return false;
                if (!TryParseKey(part, out key))
                    return false;
            }

            if (key == KeyCode.None && modifiers == Modifier.None)
                return false;

            chord = (key, modifiers);
            return true;
        }

        private static bool TryParseKey(string part, out KeyCode key)
        {
            if (Aliases.TryGetValue(part, out key))
                return true;
            return Enum.TryParse(part, ignoreCase: true, out key) && key != KeyCode.None;
        }

        private static Modifier ModifierOf(string part)
        {
            switch (part)
            {
                case "ctrl":
                case "control":
                case "controle": return Modifier.LeftCtrl;
                case "shift": return Modifier.LeftShift;
                case "alt": return Modifier.LeftAlt;
                case "altgr": return Modifier.RightAlt;
                case "win":
                case "windows":
                case "super":
                case "cmd": return Modifier.LeftWin;
                default: return Modifier.None;
            }
        }

        /// <summary>Tira espaços, acentos e maiúsculas para "Espaço" e "espaco" valerem a mesma coisa.</summary>
        private static string Normalize(string text)
        {
            var trimmed = text.Trim().ToLowerInvariant();
            var withoutAccents = trimmed.Normalize(NormalizationForm.FormD)
                .Where(c => CharUnicodeInfo.GetUnicodeCategory(c) != UnicodeCategory.NonSpacingMark)
                .ToArray();
            return new string(withoutAccents);
        }
    }
}
