using RSoft.MacroPad.BLL.Infrasturture.Model;
using RSoft.MacroPad.BLL.Macros;
using Xunit;

namespace RSoft.MacroPad.Tests
{
    public class ShortcutTextTests
    {
        // Quebra se a ordem ou o formato "Ctrl + Shift + T" mudar
        [Fact]
        public void DescribeChord_lists_modifiers_before_key()
        {
            var modifiers = Modifier.LeftShift | Modifier.LeftCtrl;

            var text = ShortcutText.DescribeChord(KeyCode.T, modifiers);

            Assert.Equal("Ctrl + Shift + T", text);
        }

        // Quebra se Ctrl direito aparecer com nome diferente do esquerdo
        [Fact]
        public void DescribeChord_names_right_modifiers_like_left_ones()
        {
            var modifiers = Modifier.RightCtrl;

            var text = ShortcutText.DescribeChord(KeyCode.C, modifiers);

            Assert.Equal("Ctrl + C", text);
        }

        // Quebra se uma gravação só com a tecla Windows sair vazia
        [Fact]
        public void DescribeChord_shows_modifier_alone_when_key_is_none()
        {
            var modifiers = Modifier.LeftWin;

            var text = ShortcutText.DescribeChord(KeyCode.None, modifiers);

            Assert.Equal("Win", text);
        }

        // Quebra se sequências de mais de uma combinação perderem o separador
        [Fact]
        public void Describe_joins_sequence_with_separator()
        {
            var keys = new[] { (KeyCode.K, Modifier.LeftCtrl), (KeyCode.S, Modifier.None) };

            var text = ShortcutText.Describe(keys);

            Assert.Equal("Ctrl + K" + ShortcutText.ChordSeparator + "S", text);
        }

        // Quebra se a vírgula (enum "Clear") voltar a aparecer como "Clear" para o usuário
        [Fact]
        public void DescribeChord_shows_comma_symbol_for_clear_key()
        {
            var text = ShortcutText.DescribeChord(KeyCode.Clear, Modifier.None);

            Assert.Equal(",", text);
        }

        // Quebra se macro de mouse com Ctrl deixar de mostrar o modificador
        [Fact]
        public void Describe_mouse_macro_includes_modifiers()
        {
            var macro = new Macro { Kind = MacroKind.Mouse, MouseButton = MouseButton.ScrollUp, MouseModifiers = Modifier.LeftCtrl };

            var text = ShortcutText.Describe(macro);

            Assert.Equal("Ctrl + Rolar ↑", text);
        }
    }
}
