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

        // Quebra se a numeração dos knobs sair errada. Estes números foram medidos no aparelho:
        // girar o knob 1 para a esquerda responde ao slot 16, e não ao 17 como diz a documentação pública.
        [Theory]
        [InlineData(InputAction.Key1, 1)]
        [InlineData(InputAction.Key12, 12)]
        [InlineData(InputAction.Knob1Left, 16)]
        [InlineData(InputAction.Knob1Push, 17)]
        [InlineData(InputAction.Knob1Right, 18)]
        [InlineData(InputAction.Knob2Left, 19)]
        [InlineData(InputAction.Knob2Push, 20)]
        [InlineData(InputAction.Knob2Right, 21)]
        [InlineData(InputAction.Knob3Right, 24)]
        public void Slot_numbers_keys_then_knob_actions(InputAction action, byte expectedSlot)
        {
            Assert.Equal(expectedSlot, Ch57x3ReportComposer.Slot(action));
        }

        // Quebra se a tecla de mídia deixar de usar o código de consumo do HID (volume + é 0xE9)
        [Fact]
        public void Media_writes_the_consumer_code()
        {
            var reports = _composer.Media(InputAction.Knob1Right, 1, MediaKey.VolUp).ToList();

            Assert.Equal(new byte[] { 0xFD, 18, 1, 2, 0, 2, 0, 0, 0xE9, 0, 0, 0 }, reports[0].Data.Take(12));
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

        // Quebra se o preâmbulo sumir: sem o FB FB FB o teclado recebe a luz e descarta sem avisar
        [Fact]
        public void Led_starts_with_the_config_preamble()
        {
            var reports = _composer.Led(1, WhiteScheme()).ToList();

            Assert.Equal(new byte[] { 0xFB, 0xFB, 0xFB }, reports[0].Data.Take(3));
        }

        // Quebra se o comando de luz voltar ao cabeçalho das teclas. Ele é 0xFE, destino 0xB0,
        // camada contando do zero, e o efeito no quarto byte. Conferido no aparelho em 16/09/2026.
        [Fact]
        public void Led_writes_the_header_with_a_zero_based_layer()
        {
            var reports = _composer.Led(1, WhiteScheme()).ToList();

            Assert.Equal(new byte[] { 0xFE, 0xB0, 0, 1 }, reports[1].Data.Take(4));
        }

        // Quebra se o branco deixar de sair branco: são 17 trios RGB seguidos, um por tecla
        [Fact]
        public void Led_repeats_the_base_colour_for_the_seventeen_keys()
        {
            var reports = _composer.Led(1, WhiteScheme()).ToList();

            var colours = reports[1].Data.Skip(4).Take(17 * 3);
            Assert.All(colours, channel => Assert.Equal(255, channel));
        }

        // Quebra se a cor de uma tecla sozinha for parar no trio errado: a tecla 3 é o terceiro trio
        [Fact]
        public void Led_puts_a_painted_key_in_its_own_triplet()
        {
            var scheme = WhiteScheme();
            scheme.KeyColors[3] = new LedRgb(10, 20, 30);

            var reports = _composer.Led(1, scheme).ToList();

            Assert.Equal(new byte[] { 10, 20, 30 }, reports[1].Data.Skip(4 + 2 * 3).Take(3));
            Assert.Equal(new byte[] { 255, 255, 255 }, reports[1].Data.Skip(4 + 3 * 3).Take(3));
        }

        // Quebra se a sequência de gravação da luz mudar: AA AA, FD FE FF, AA AA
        [Fact]
        public void Led_ends_with_the_commit_sequence()
        {
            var reports = _composer.Led(1, WhiteScheme()).ToList();

            Assert.Equal(5, reports.Count);
            Assert.Equal(new byte[] { 0xAA, 0xAA }, reports[2].Data.Take(2));
            Assert.Equal(new byte[] { 0xFD, 0xFE, 0xFF }, reports[3].Data.Take(3));
            Assert.Equal(new byte[] { 0xAA, 0xAA }, reports[4].Data.Take(2));
        }

        private static LightScheme WhiteScheme()
        {
            return new LightScheme { Mode = LedMode.Mode1, BaseColor = new LedRgb(255, 255, 255) };
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
