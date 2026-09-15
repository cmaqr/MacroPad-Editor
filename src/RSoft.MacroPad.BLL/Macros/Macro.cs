using System;
using System.Collections.Generic;
using RSoft.MacroPad.BLL.Infrasturture.Model;

namespace RSoft.MacroPad.BLL.Macros
{
    public enum MacroKind
    {
        Keys,
        Media,
        Mouse,
    }

    /// <summary>
    /// Uma ação pronta para gravar numa tecla: sequência de teclas, tecla de mídia ou botão do mouse.
    /// </summary>
    public sealed class Macro
    {
        public string Id { get; init; }
        public string Title { get; init; }
        public string Category { get; init; }

        /// <summary>
        /// Um caractere da fonte Segoe Fluent Icons (faixa U+E000 em diante) ou um texto curto, como "F13".
        /// </summary>
        public string Icon { get; init; }

        /// <summary>
        /// Observação curta mostrada junto do atalho, ex.: "Chrome e Edge".
        /// </summary>
        public string Hint { get; init; } = "";

        public MacroKind Kind { get; init; }

        public IReadOnlyList<(KeyCode Key, Modifier Modifiers)> Keys { get; init; } = Array.Empty<(KeyCode, Modifier)>();

        public MediaKey MediaKey { get; init; }

        public MouseButton MouseButton { get; init; }
        public Modifier MouseModifiers { get; init; }
    }

    /// <summary>
    /// Configuração de várias teclas de uma vez. Os ids apontam para macros do catálogo.
    /// Botões são preenchidos na ordem; cada knob recebe (girar à esquerda, apertar, girar à direita).
    /// </summary>
    public sealed class MacroKit
    {
        public string Id { get; init; }
        public string Title { get; init; }
        public string Description { get; init; }
        public string Icon { get; init; }
        public IReadOnlyList<string> ButtonMacroIds { get; init; }
        public IReadOnlyList<(string Left, string Push, string Right)> KnobMacroIds { get; init; }
    }
}
