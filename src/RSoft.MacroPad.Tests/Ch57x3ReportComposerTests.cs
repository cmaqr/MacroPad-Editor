using System.Linq;
using RSoft.MacroPad.BLL.Infrasturture.Model;
using RSoft.MacroPad.BLL.Infrasturture.Protocol;
using Xunit;

namespace RSoft.MacroPad.Tests
{
    /// <summary>
    /// Os bytes esperados aqui são os mesmos que funcionaram no macropad 514C:8850 em 15/09/2026.
    /// </summary>
    public class Ch57x3ReportComposerTests
    {
        private const byte ReportId = 3;
        private readonly Ch57x3ReportComposer _composer = new Ch57x3ReportComposer(ReportId);

        // Quebra se o formato que o teclado aceita mudar: cabeçalho 0xFD, slot, camada, tipo, qtd e 3 bytes por passo
        [Fact]
        public void Key_writes_one_step_with_three_bytes()
        {
            var sequence = new[] { (KeyCode.M, Modifier.None) };

            var reports = _composer.Key(InputAction.Key1, 1, 100, sequence).ToList();

            Assert.Equal(new byte[] { 0xFD, 1, 1, 1, 0, 1, 0, 0, (byte)KeyCode.M }, reports[0].Data.Take(9));
        }

        // Quebra se o modificador voltar a ser um bit em vez de um passo próprio (era o erro que impedia de gravar)
        [Fact]
        public void Key_writes_each_modifier_as_its_own_step()
        {
            var sequence = new[] { (KeyCode.C, Modifier.LeftCtrl) };

            var reports = _composer.Key(InputAction.Key1, 1, 100, sequence).ToList();

            // dois passos: primeiro o Ctrl (0xF1), depois a tecla
            Assert.Equal(new byte[] { 0xFD, 1, 1, 1, 0, 2, 0, 0, 0xF1, 0, 0, (byte)KeyCode.C }, reports[0].Data.Take(12));
        }

        // Quebra se o report de encerramento sumir: sem ele o teclado não guarda a gravação
        [Fact]
        public void Every_binding_ends_with_the_terminator()
        {
            var reports = _composer.Key(InputAction.Key1, 1, 100, new[] { (KeyCode.A, Modifier.None) }).ToList();

            Assert.Equal(2, reports.Count);
            Assert.Equal(new byte[] { 0xFD, 0xFE, 0xFF }, reports[1].Data.Take(3));
        }

        // Quebra se a numeração dos knobs sair errada: teclas vão de 1 a 16 e os knobs começam em 17
        [Theory]
        [InlineData(InputAction.Key1, 1)]
        [InlineData(InputAction.Key12, 12)]
        [InlineData(InputAction.Knob1Left, 17)]
        [InlineData(InputAction.Knob1Push, 18)]
        [InlineData(InputAction.Knob1Right, 19)]
        [InlineData(InputAction.Knob2Left, 20)]
        [InlineData(InputAction.Knob3Right, 25)]
        public void Slot_numbers_keys_then_knob_actions(InputAction action, byte expectedSlot)
        {
            Assert.Equal(expectedSlot, Ch57x3ReportComposer.Slot(action));
        }

        // Quebra se a tecla de mídia deixar de usar o código de consumo do HID (volume + é 0xE9)
        [Fact]
        public void Media_writes_the_consumer_code()
        {
            var reports = _composer.Media(InputAction.Knob1Right, 1, MediaKey.VolUp).ToList();

            Assert.Equal(new byte[] { 0xFD, 19, 1, 2, 0, 2, 0, 0, 0xE9, 0, 0, 0 }, reports[0].Data.Take(12));
        }

        // Quebra se o clique do mouse mudar de lugar no report (o botão fica no byte 13)
        [Fact]
        public void Mouse_writes_the_button_bitmap()
        {
            var reports = _composer.Mouse(InputAction.Key5, 1, MouseButton.Right, Modifier.None).ToList();

            var data = reports[0].Data;
            Assert.Equal(new byte[] { 0xFD, 5, 1, 3, 1, 4 }, data.Take(6));
            Assert.Equal(2, data[13]);
        }

        // Quebra se a rolagem parar de ir no último byte do bloco
        [Fact]
        public void Mouse_writes_the_wheel_in_the_last_byte()
        {
            var reports = _composer.Mouse(InputAction.Key6, 1, MouseButton.ScrollUp, Modifier.None).ToList();

            Assert.Equal(1, reports[0].Data[22]);
        }

        // Quebra se Ctrl + rolagem (o zoom) perder o modificador
        [Fact]
        public void Mouse_writes_the_modifier_step()
        {
            var reports = _composer.Mouse(InputAction.Key6, 1, MouseButton.ScrollUp, Modifier.LeftCtrl).ToList();

            Assert.Equal(0xF1, reports[0].Data[9]);
        }

        // Quebra se a luz mudar de formato: slot 0xB0, tipo 8, e cor e efeito juntos no último byte
        [Fact]
        public void Led_joins_color_and_mode_in_one_byte()
        {
            var reports = _composer.Led(1, LedMode.Mode1, LedColor.White).ToList();

            Assert.Equal(new byte[] { 0xFD, 0xB0, 1, 8, 0, 0, 0, 0, 0, 1, 0, 0x81 }, reports[0].Data.Take(12));
        }

        // Quebra se uma macro longa passar do limite do aparelho e ele recusar a gravação inteira
        [Fact]
        public void Key_stops_at_the_maximum_number_of_steps()
        {
            var sequence = Enumerable.Repeat((KeyCode.A, Modifier.LeftCtrl), 20).ToArray();

            var reports = _composer.Key(InputAction.Key1, 1, 100, sequence).ToList();

            Assert.Equal(Ch57x3ReportComposer.MaxSteps, reports[0].Data[5]);
        }
    }
}
