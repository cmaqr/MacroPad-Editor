using System;
using System.IO;
using System.Linq;
using RSoft.MacroPad.BLL.Infrasturture.Model;
using RSoft.MacroPad.BLL.Macros;
using Xunit;

namespace RSoft.MacroPad.Tests
{
    public class UserMacrosTests : IDisposable
    {
        private readonly string _filePath = Path.Combine(Path.GetTempPath(), $"macropad-meus-{Guid.NewGuid()}.txt");

        public void Dispose()
        {
            if (File.Exists(_filePath))
                File.Delete(_filePath);
        }

        // Quebra se o arquivo de atalhos do usuário parar de virar macro utilizável
        [Fact]
        public void Read_turns_each_line_into_a_macro()
        {
            File.WriteAllText(_filePath, "// comentário\n\nPaleta = Ctrl + Shift + P\nSalvar tudo = Ctrl + K > S\n");

            var file = UserMacros.Read(_filePath);

            Assert.Equal(new[] { "Paleta", "Salvar tudo" }, file.Macros.Select(m => m.Title));
            Assert.Equal(2, file.Macros[1].Keys.Count);
            Assert.Equal(UserMacros.Category, file.Macros[0].Category);
        }

        // Quebra se uma linha errada derrubar o arquivo inteiro em vez de só ser apontada
        [Fact]
        public void Read_keeps_good_lines_and_reports_bad_ones()
        {
            File.WriteAllText(_filePath, "Bom = Ctrl + C\nRuim = Ctrl + Banana\nSem igual\n");

            var file = UserMacros.Read(_filePath);

            Assert.Single(file.Macros);
            Assert.Equal(2, file.BadLines.Count);
        }

        // Quebra se o app passar a exigir o arquivo para abrir
        [Fact]
        public void Read_returns_empty_when_file_is_missing()
        {
            var file = UserMacros.Read(_filePath);

            Assert.Empty(file.Macros);
            Assert.Empty(file.BadLines);
        }

        // Quebra se o exemplo criado na primeira vez nascer com erro dentro
        [Fact]
        public void CreateExampleIfMissing_writes_a_file_that_parses()
        {
            UserMacros.CreateExampleIfMissing(_filePath);

            var file = UserMacros.Read(_filePath);
            Assert.NotEmpty(file.Macros);
            Assert.Empty(file.BadLines);
        }

        // Quebra se o exemplo apagar atalhos que o usuário já tinha escrito
        [Fact]
        public void CreateExampleIfMissing_does_not_touch_an_existing_file()
        {
            File.WriteAllText(_filePath, "Meu = Ctrl + J\n");

            UserMacros.CreateExampleIfMissing(_filePath);

            Assert.Equal("Meu", UserMacros.Read(_filePath).Macros[0].Title);
        }
    }
}
