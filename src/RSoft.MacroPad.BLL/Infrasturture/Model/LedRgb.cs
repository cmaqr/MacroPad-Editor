namespace RSoft.MacroPad.BLL.Infrasturture.Model
{
    /// <summary>
    /// Cor da iluminação em RGB. Os teclados do dialeto 514C acendem exatamente esta cor;
    /// os modelos antigos só têm oito cores fixas e recebem a mais parecida.
    /// </summary>
    public struct LedRgb
    {
        public LedRgb(byte red, byte green, byte blue)
        {
            Red = red;
            Green = green;
            Blue = blue;
            IsRandom = false;
        }

        public byte Red { get; set; }
        public byte Green { get; set; }
        public byte Blue { get; set; }

        /// <summary>Nos modelos antigos o próprio teclado sorteia a cor; no 514C sorteamos uma cor por tecla.</summary>
        public bool IsRandom { get; set; }

        public static LedRgb Random() => new LedRgb { IsRandom = true };
    }
}
