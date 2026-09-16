namespace RSoft.MacroPad.BLL.Infrasturture.Model
{
    public enum LedColor : byte
    {
        Random = 0,
        Red = 0x10,
        Orange = 0x20,
        Yellow = 0x30,
        Green = 0x40,
        Cyan = 0x50,
        Blue = 0x60,
        Purple = 0x70,
        /// <summary>Só existe nos teclados do dialeto 514C, onde os valores altos acendem branco.</summary>
        White = 0x80,
    }
}
