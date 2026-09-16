using RSoft.MacroPad.BLL.Infrasturture.Model;

namespace RSoft.MacroPad.BLL.Infrasturture.Protocol.Mappers
{
    public static class LedColorMapper
    {
        // As únicas cores que os modelos antigos acendem, com o RGB aproximado de cada uma
        private static readonly (LedColor Color, int Red, int Green, int Blue)[] _fixedColors =
        {
            (LedColor.Red, 255, 0, 0),
            (LedColor.Orange, 255, 128, 0),
            (LedColor.Yellow, 255, 255, 0),
            (LedColor.Green, 0, 255, 0),
            (LedColor.Cyan, 0, 255, 255),
            (LedColor.Blue, 0, 0, 255),
            (LedColor.Purple, 160, 0, 255),
            (LedColor.White, 255, 255, 255),
        };

        /// <summary>
        /// Escolhe a cor fixa mais parecida com o RGB pedido, comparando os três canais.
        /// É o que sobra para os teclados que não têm RGB de verdade.
        /// </summary>
        public static LedColor Nearest(LedRgb rgb)
        {
            if (rgb.IsRandom)
                return LedColor.Random;

            var best = LedColor.Red;
            var bestDistance = int.MaxValue;
            foreach (var (color, red, green, blue) in _fixedColors)
            {
                var dr = rgb.Red - red;
                var dg = rgb.Green - green;
                var db = rgb.Blue - blue;
                var distance = dr * dr + dg * dg + db * db;
                if (distance < bestDistance)
                {
                    bestDistance = distance;
                    best = color;
                }
            }
            return best;
        }
    }
}
