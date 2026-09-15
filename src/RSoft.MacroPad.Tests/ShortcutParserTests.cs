using RSoft.MacroPad.BLL.Infrasturture.Model;
using RSoft.MacroPad.BLL.Macros;
using Xunit;

namespace RSoft.MacroPad.Tests
{
    public class ShortcutParserTests
    {
        // Quebra se o formato mais comum ("Ctrl + Shift + T") deixar de ser entendido
        [Fact]
        public void TryParse_reads_modifiers_and_key()
        {
            var parsed = ShortcutParser.TryParse("Ctrl + Shift + T", out var chords);

            Assert.True(parsed);
            Assert.Equal(new[] { (KeyCode.T, Modifier.LeftCtrl | Modifier.LeftShift) }, chords);
        }

        // Quebra se o texto escrito sem acento ou em minúsculas parar de funcionar
        [Fact]
        public void TryParse_ignores_case_and_accents()
        {
            var parsed = ShortcutParser.TryParse("ctrl+espaco", out var chords);

            Assert.True(parsed);
            Assert.Equal(new[] { (KeyCode.SpaceKey, Modifier.LeftCtrl) }, chords);
        }

        // Quebra se sequência de duas combinações (atalho do VS Code) parar de ser lida
        [Fact]
        public void TryParse_reads_sequence_separated_by_arrow()
        {
            var parsed = ShortcutParser.TryParse("Ctrl + K > S", out var chords);

            Assert.True(parsed);
            Assert.Equal(new[] { (KeyCode.K, Modifier.LeftCtrl), (KeyCode.S, Modifier.None) }, chords);
        }

        // Quebra se a tecla "+" (usada para zoom) parar de ser reconhecida, já que "+" também separa
        [Fact]
        public void TryParse_reads_plus_as_a_key()
        {
            var parsed = ShortcutParser.TryParse("Ctrl + +", out var chords);

            Assert.True(parsed);
            Assert.Equal(new[] { (KeyCode.Plus, Modifier.LeftCtrl) }, chords);
        }

        // Quebra se nomes escritos de outro jeito ("Page Up", seta) deixarem de valer
        [Fact]
        public void TryParse_accepts_alternative_key_names()
        {
            Assert.True(ShortcutParser.TryParse("Page Up", out var pageUp));
            Assert.True(ShortcutParser.TryParse("←", out var left));

            Assert.Equal(KeyCode.PgUp, pageUp[0].Key);
            Assert.Equal(KeyCode.ArrowLeft, left[0].Key);
        }

        // Quebra se texto sem sentido passar a virar atalho errado em vez de ser recusado
        [Fact]
        public void TryParse_rejects_unknown_text()
        {
            Assert.False(ShortcutParser.TryParse("Ctrl + Banana", out _));
            Assert.False(ShortcutParser.TryParse("", out _));
        }

        // Quebra se duas teclas na mesma combinação ("A + B") passarem a ser aceitas: o macropad não faz isso
        [Fact]
        public void TryParse_rejects_two_keys_in_one_chord()
        {
            Assert.False(ShortcutParser.TryParse("A + B", out _));
        }

        // Quebra se o modificador sozinho (só a tecla Windows) deixar de valer
        [Fact]
        public void TryParse_accepts_modifier_alone()
        {
            var parsed = ShortcutParser.TryParse("Win", out var chords);

            Assert.True(parsed);
            Assert.Equal(new[] { (KeyCode.None, Modifier.LeftWin) }, chords);
        }
    }
}
