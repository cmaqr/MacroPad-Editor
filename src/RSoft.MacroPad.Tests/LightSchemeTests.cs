using RSoft.MacroPad.BLL.Infrasturture.Model;
using Xunit;

namespace RSoft.MacroPad.Tests
{
    public class LightSchemeTests
    {
        // Quebra se uma tecla sem pintura própria parar de herdar a cor geral
        [Fact]
        public void ColorOf_falls_back_to_the_base_colour()
        {
            var scheme = new LightScheme { BaseColor = new LedRgb(1, 2, 3) };

            var color = scheme.ColorOf(5);

            Assert.Equal(new LedRgb(1, 2, 3), color);
        }

        // Quebra se pintar uma tecla deixar de valer mais que a cor geral
        [Fact]
        public void ColorOf_prefers_the_painted_key()
        {
            var scheme = new LightScheme { BaseColor = new LedRgb(1, 2, 3) };
            scheme.KeyColors[5] = new LedRgb(9, 9, 9);

            var color = scheme.ColorOf(5);

            Assert.Equal(new LedRgb(9, 9, 9), color);
        }
    }
}
