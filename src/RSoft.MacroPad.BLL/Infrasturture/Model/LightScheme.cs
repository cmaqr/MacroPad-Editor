using System.Collections.Generic;

namespace RSoft.MacroPad.BLL.Infrasturture.Model
{
    /// <summary>
    /// Como a iluminação deve ficar: o efeito, a cor geral e as teclas que foram pintadas à parte.
    /// Só os teclados com RGB por tecla usam <see cref="KeyColors"/>; nos outros vale só a cor geral.
    /// </summary>
    public sealed class LightScheme
    {
        public LedMode Mode { get; set; } = LedMode.Mode1;

        /// <summary>Cor das teclas que o usuário não pintou uma a uma.</summary>
        public LedRgb BaseColor { get; set; } = new LedRgb(255, 255, 255);

        /// <summary>Cor de cada tecla, pelo número dela (1 a 12). Tecla que não está aqui usa a cor geral.</summary>
        public Dictionary<int, LedRgb> KeyColors { get; set; } = new Dictionary<int, LedRgb>();

        public LedRgb ColorOf(int keyNumber)
        {
            return KeyColors.TryGetValue(keyNumber, out var color) ? color : BaseColor;
        }
    }
}
