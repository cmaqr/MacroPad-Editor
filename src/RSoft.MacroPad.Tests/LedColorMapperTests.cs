using RSoft.MacroPad.BLL.Infrasturture.Model;
using RSoft.MacroPad.BLL.Infrasturture.Protocol.Mappers;
using Xunit;

namespace RSoft.MacroPad.Tests
{
    /// <summary>
    /// Os teclados antigos só acendem oito cores. Aqui é onde a cor livre escolhida na tela
    /// vira uma delas, para esses modelos continuarem funcionando.
    /// </summary>
    public class LedColorMapperTests
    {
        // Quebra se a aproximação errar a cor óbvia: um vermelho puro tem que virar vermelho
        [Theory]
        [InlineData(255, 0, 0, LedColor.Red)]
        [InlineData(0, 255, 0, LedColor.Green)]
        [InlineData(0, 0, 255, LedColor.Blue)]
        [InlineData(255, 255, 255, LedColor.White)]
        public void Nearest_returns_the_matching_fixed_colour(byte red, byte green, byte blue, LedColor expected)
        {
            Assert.Equal(expected, LedColorMapper.Nearest(new LedRgb(red, green, blue)));
        }

        // Quebra se uma cor que não existe nos modelos antigos parar de cair na mais parecida
        [Fact]
        public void Nearest_approximates_a_colour_the_old_keyboards_do_not_have()
        {
            var salmon = new LedRgb(255, 140, 20);

            Assert.Equal(LedColor.Orange, LedColorMapper.Nearest(salmon));
        }

        // Quebra se a cor aleatória deixar de virar o sorteio que o próprio teclado antigo faz
        [Fact]
        public void Nearest_keeps_random_as_random()
        {
            Assert.Equal(LedColor.Random, LedColorMapper.Nearest(LedRgb.Random()));
        }
    }
}
