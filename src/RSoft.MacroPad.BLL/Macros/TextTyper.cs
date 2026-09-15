using System.Collections.Generic;
using System.Text;
using RSoft.MacroPad.BLL.Infrasturture.Model;

namespace RSoft.MacroPad.BLL.Macros
{
    public sealed record TypedText(IReadOnlyList<(KeyCode Key, Modifier Modifiers)> Keys, string UnsupportedCharacters);

    /// <summary>
    /// Converte um texto na sequência de teclas que o macropad vai digitar.
    /// </summary>
    public static class TextTyper
    {
        /// <summary>
        /// Só aceita caracteres que saem na mesma tecla nos layouts US e ABNT2.
        /// Acentos, ç e símbolos como ? ; : / mudam de lugar entre layouts, então ficam de fora.
        /// </summary>
        public static TypedText ToKeys(string text, bool pressEnterAtEnd = false)
        {
            var keys = new List<(KeyCode, Modifier)>();
            var unsupported = new StringBuilder();

            foreach (var character in text ?? "")
            {
                if (character == '\r')
                    continue;

                if (TryMap(character, out var key, out var modifiers))
                {
                    keys.Add((key, modifiers));
                    continue;
                }

                if (unsupported.ToString().IndexOf(character) == -1)
                    unsupported.Append(character);
            }

            if (pressEnterAtEnd)
                keys.Add((KeyCode.Enter, Modifier.None));

            return new TypedText(keys, unsupported.ToString());
        }

        private static bool TryMap(char character, out KeyCode key, out Modifier modifiers)
        {
            modifiers = Modifier.None;

            if (character >= 'a' && character <= 'z')
            {
                // No enum, as letras de A a Z têm códigos seguidos
                key = (KeyCode)((int)KeyCode.A + (character - 'a'));
                return true;
            }
            if (character >= 'A' && character <= 'Z')
            {
                key = (KeyCode)((int)KeyCode.A + (character - 'A'));
                modifiers = Modifier.LeftShift;
                return true;
            }
            if (character >= '1' && character <= '9')
            {
                key = (KeyCode)((int)KeyCode.D1 + (character - '1'));
                return true;
            }

            switch (character)
            {
                case '0': key = KeyCode.D0; return true;
                case ' ': key = KeyCode.SpaceKey; return true;
                case '\n': key = KeyCode.Enter; return true;
                case '\t': key = KeyCode.Tab; return true;
                case '.': key = KeyCode.Period; return true;
                // "Clear" é o nome que o projeto original deu à tecla de vírgula
                case ',': key = KeyCode.Clear; return true;
                case '-': key = KeyCode.Minus; return true;
                case '=': key = KeyCode.Plus; return true;
            }

            modifiers = Modifier.LeftShift;
            switch (character)
            {
                case '!': key = KeyCode.D1; return true;
                case '@': key = KeyCode.D2; return true;
                case '#': key = KeyCode.D3; return true;
                case '$': key = KeyCode.D4; return true;
                case '%': key = KeyCode.D5; return true;
                case '&': key = KeyCode.D7; return true;
                case '*': key = KeyCode.D8; return true;
                case '(': key = KeyCode.D9; return true;
                case ')': key = KeyCode.D0; return true;
                case '_': key = KeyCode.Minus; return true;
                case '+': key = KeyCode.Plus; return true;
            }

            key = KeyCode.None;
            modifiers = Modifier.None;
            return false;
        }
    }
}
