using RSoft.MacroPad.BLL.Infrasturture.Model;
using RSoft.MacroPad.BLL.Macros;
using Xunit;

namespace RSoft.MacroPad.Tests
{
    public class RecordedKeyMapperTests
    {
        // Quebra se a tecla Pause voltar a ser gravada como NumLock (issue #43): as duas têm o mesmo código físico 0x45
        [Fact]
        public void Map_records_pause_instead_of_numlock()
        {
            var hookKey = VirtualKey.Pause;
            var scanCodeKey = VirtualKey.NumLock;

            var keyCode = RecordedKeyMapper.Map(hookKey, scanCodeKey);

            Assert.Equal(KeyCode.PauseBreak, keyCode);
        }

        // Quebra se o NumLock de verdade deixar de ser gravado
        [Fact]
        public void Map_still_records_numlock_itself()
        {
            var keyCode = RecordedKeyMapper.Map(VirtualKey.NumLock, VirtualKey.NumLock);

            Assert.Equal(KeyCode.Num, keyCode);
        }

        // Quebra se letras pararem de valer pela posição física: num teclado AZERTY, a tecla "A" fica onde o americano tem "Q"
        [Fact]
        public void Map_uses_physical_position_for_letters()
        {
            var hookKey = VirtualKey.A;
            var scanCodeKey = VirtualKey.Q;

            var keyCode = RecordedKeyMapper.Map(hookKey, scanCodeKey);

            Assert.Equal(KeyCode.Q, keyCode);
        }

        // Quebra se a pontuação parar de valer pela posição física (o que muda de lugar no ABNT2)
        [Fact]
        public void Map_uses_physical_position_for_punctuation()
        {
            var keyCode = RecordedKeyMapper.Map(VirtualKey.OemSemicolon, VirtualKey.OemQuestion);

            Assert.Equal(KeyCode.Question, keyCode);
        }

        // Quebra se teclas iguais em todo layout (F5) deixarem de ser gravadas
        [Fact]
        public void Map_records_function_keys()
        {
            var keyCode = RecordedKeyMapper.Map(VirtualKey.F5, VirtualKey.F5);

            Assert.Equal(KeyCode.F5, keyCode);
        }

        // Quebra se uma tecla que o macropad não sabe reproduzir passar a ser gravada como outra coisa
        [Fact]
        public void Map_returns_none_for_unsupported_key()
        {
            var keyCode = RecordedKeyMapper.Map(VirtualKey.VolumeUp, VirtualKey.VolumeUp);

            Assert.Equal(KeyCode.None, keyCode);
        }
    }
}
