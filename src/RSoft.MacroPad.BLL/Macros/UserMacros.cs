using System.Collections.Generic;
using System.IO;

namespace RSoft.MacroPad.BLL.Macros
{
    public sealed record UserMacrosFile(IReadOnlyList<Macro> Macros, IReadOnlyList<string> BadLines);

    /// <summary>
    /// Atalhos que o usuário escreve num arquivo de texto, sem precisar recompilar o app.
    /// Uma linha por atalho, no formato: Nome = Ctrl + Shift + P
    /// </summary>
    public static class UserMacros
    {
        public const string Category = "Meus";

        public static UserMacrosFile Read(string filePath)
        {
            var macros = new List<Macro>();
            var badLines = new List<string>();

            if (!File.Exists(filePath))
                return new UserMacrosFile(macros, badLines);

            var lineNumber = 0;
            foreach (var rawLine in File.ReadAllLines(filePath))
            {
                lineNumber++;
                var line = rawLine.Trim();
                if (line.Length == 0 || line.StartsWith("//") || line.StartsWith("#"))
                    continue;

                var separator = line.IndexOf('=');
                if (separator <= 0)
                {
                    badLines.Add($"Linha {lineNumber}: falta o sinal de igual. Use: Nome = Ctrl + Shift + P");
                    continue;
                }

                var title = line.Substring(0, separator).Trim();
                var shortcut = line.Substring(separator + 1).Trim();
                if (!ShortcutParser.TryParse(shortcut, out var chords))
                {
                    badLines.Add($"Linha {lineNumber}: não entendi o atalho \"{shortcut}\"");
                    continue;
                }

                macros.Add(new Macro
                {
                    Id = "user-" + lineNumber,
                    Title = title,
                    Category = Category,
                    Icon = "\uE734",
                    Hint = "Do seu arquivo meus-atalhos.txt",
                    Kind = MacroKind.Keys,
                    Keys = chords,
                });
            }

            return new UserMacrosFile(macros, badLines);
        }

        /// <summary>Cria o arquivo de exemplo na primeira vez, para o usuário ter por onde começar.</summary>
        public static void CreateExampleIfMissing(string filePath)
        {
            if (File.Exists(filePath))
                return;

            File.WriteAllText(filePath, string.Join("\n", new[]
            {
                "// Seus atalhos. Uma linha por atalho, assim:",
                "//    Nome = Ctrl + Shift + P",
                "// Para uma sequência, separe com >:",
                "//    Salvar tudo = Ctrl + K > S",
                "// Depois de editar, reabra o app e procure pela categoria \"Meus\".",
                "",
                "Print da janela = Alt + Print Screen",
                "Fechar aba = Ctrl + W",
            }));
        }
    }
}
