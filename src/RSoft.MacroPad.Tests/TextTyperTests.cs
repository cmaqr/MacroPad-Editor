using RSoft.MacroPad.BLL.Infrasturture.Model;
using RSoft.MacroPad.BLL.Macros;
using Xunit;

namespace RSoft.MacroPad.Tests
{
    public class TextTyperTests
    {
        // Quebra se as letras minúsculas deixarem de virar a tecla certa sem Shift
        [Fact]
        public void ToKeys_maps_lowercase_letters_without_modifier()
        {
            var text = "abz";

            var typed = TextTyper.ToKeys(text);

            Assert.Equal(new[] { (KeyCode.A, Modifier.None), (KeyCode.B, Modifier.None), (KeyCode.Z, Modifier.None) }, typed.Keys);
        }

        // Quebra se maiúscula parar de mandar Shift junto
        [Fact]
        public void ToKeys_adds_shift_to_uppercase_letters()
        {
            var text = "Ok";

            var typed = TextTyper.ToKeys(text);

            Assert.Equal(new[] { (KeyCode.O, Modifier.LeftShift), (KeyCode.K, Modifier.None) }, typed.Keys);
        }

        // Quebra se a fileira de números sair desalinhada (o 0 fica depois do 9 no enum)
        [Fact]
        public void ToKeys_maps_digits_to_number_row()
        {
            var text = "109";

            var typed = TextTyper.ToKeys(text);

            Assert.Equal(new[] { (KeyCode.D1, Modifier.None), (KeyCode.D0, Modifier.None), (KeyCode.D9, Modifier.None) }, typed.Keys);
        }

        // Quebra se símbolos com Shift (! @ _) forem mapeados para a tecla errada
        [Fact]
        public void ToKeys_maps_shifted_symbols()
        {
            var text = "!@_";

            var typed = TextTyper.ToKeys(text);

            Assert.Equal(new[] { (KeyCode.D1, Modifier.LeftShift), (KeyCode.D2, Modifier.LeftShift), (KeyCode.Minus, Modifier.LeftShift) }, typed.Keys);
        }

        // Quebra se acento ou ? passarem a ser enviados em vez de avisados
        [Fact]
        public void ToKeys_reports_unsupported_characters_once_and_skips_them()
        {
            var text = "é?é";

            var typed = TextTyper.ToKeys(text);

            Assert.Empty(typed.Keys);
            Assert.Equal("é?", typed.UnsupportedCharacters);
        }

        // Quebra se quebra de linha do Windows (\r\n) virar dois Enter
        [Fact]
        public void ToKeys_turns_windows_line_break_into_single_enter()
        {
            var text = "a\r\nb";

            var typed = TextTyper.ToKeys(text);

            Assert.Equal(new[] { (KeyCode.A, Modifier.None), (KeyCode.Enter, Modifier.None), (KeyCode.B, Modifier.None) }, typed.Keys);
        }

        // Quebra se a opção "Enter no final" parar de adicionar a tecla
        [Fact]
        public void ToKeys_appends_enter_when_requested()
        {
            var text = "ok";

            var typed = TextTyper.ToKeys(text, pressEnterAtEnd: true);

            Assert.Equal((KeyCode.Enter, Modifier.None), typed.Keys[typed.Keys.Count - 1]);
        }
    }
}
