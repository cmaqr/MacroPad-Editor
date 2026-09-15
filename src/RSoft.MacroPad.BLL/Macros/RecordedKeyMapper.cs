using RSoft.MacroPad.BLL.Infrasturture.Model;
using RSoft.MacroPad.BLL.Infrasturture.Protocol.Mappers;

namespace RSoft.MacroPad.BLL.Macros
{
    /// <summary>
    /// Decide qual tecla foi gravada a partir do que o Windows informou.
    /// </summary>
    public static class RecordedKeyMapper
    {
        /// <param name="hookKey">Tecla que o Windows informou, já no layout do usuário.</param>
        /// <param name="scanCodeKey">Tecla obtida da posição física traduzida pelo layout americano.</param>
        public static KeyCode Map(VirtualKey hookKey, VirtualKey scanCodeKey)
        {
            // Letra, número e pontuação mudam de lugar entre layouts (ABNT2, AZERTY...),
            // então para essas vale a posição física da tecla.
            if (IsLayoutDependent(hookKey))
                return KeyCodeMapper.Map(scanCodeKey);

            // O resto (F1-F24, setas, Pause, Home...) é igual em qualquer layout, e a posição física engana:
            // Pause e NumLock compartilham o código 0x45, e o Pause virava NumLock (issue #43 do projeto original).
            var fromHook = KeyCodeMapper.Map(hookKey);
            return fromHook != KeyCode.None ? fromHook : KeyCodeMapper.Map(scanCodeKey);
        }

        private static bool IsLayoutDependent(VirtualKey key)
        {
            // Faixas de código virtual do Windows: 0x30-0x39 números, 0x41-0x5A letras, 0xBA-0xE2 pontuação (OEM)
            var code = (int)key;
            return (code >= 0x30 && code <= 0x39)
                || (code >= 0x41 && code <= 0x5A)
                || (code >= 0xBA && code <= 0xE2);
        }
    }
}
