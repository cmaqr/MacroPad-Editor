using System;
using System.Collections.Generic;
using System.Linq;
using RSoft.MacroPad.BLL.Infrasturture.Model;

namespace RSoft.MacroPad.BLL.Macros
{
    /// <summary>
    /// A macro no formato em que é gravada em disco (perfis e teclas já enviadas).
    /// Guarda nomes em vez de números para o arquivo continuar legível.
    /// </summary>
    public sealed class MacroData
    {
        public string Id { get; set; }
        public string Title { get; set; }
        public string Category { get; set; }
        public string Icon { get; set; }
        public string Hint { get; set; }
        public string Kind { get; set; }
        public List<KeyChordData> Keys { get; set; } = new List<KeyChordData>();
        public string MediaKey { get; set; }
        public string MouseButton { get; set; }
        public byte MouseModifiers { get; set; }

        public static MacroData From(Macro macro)
        {
            return new MacroData
            {
                Id = macro.Id,
                Title = macro.Title,
                Category = macro.Category,
                Icon = macro.Icon,
                Hint = macro.Hint,
                Kind = macro.Kind.ToString(),
                Keys = macro.Keys.Select(chord => new KeyChordData { Key = chord.Key.ToString(), Modifiers = (byte)chord.Modifiers }).ToList(),
                MediaKey = macro.MediaKey.ToString(),
                MouseButton = macro.MouseButton.ToString(),
                MouseModifiers = (byte)macro.MouseModifiers,
            };
        }

        public Macro ToMacro()
        {
            return new Macro
            {
                Id = Id,
                Title = Title,
                Category = Category,
                Icon = Icon,
                Hint = Hint ?? "",
                Kind = Enum.TryParse<MacroKind>(Kind, out var kind) ? kind : MacroKind.Keys,
                Keys = (Keys ?? new List<KeyChordData>())
                    .Select(chord => (Enum.TryParse<KeyCode>(chord.Key, out var key) ? key : KeyCode.None, (Modifier)chord.Modifiers))
                    .ToList(),
                MediaKey = Enum.TryParse<MediaKey>(MediaKey, out var media) ? media : Infrasturture.Model.MediaKey.PlayPause,
                MouseButton = Enum.TryParse<MouseButton>(MouseButton, out var button) ? button : Infrasturture.Model.MouseButton.Left,
                MouseModifiers = (Modifier)MouseModifiers,
            };
        }
    }

    public sealed class KeyChordData
    {
        public string Key { get; set; }
        public byte Modifiers { get; set; }
    }
}
