namespace RSoft.MacroPad.BLL.Infrasturture.Model
{
    public enum ProtocolType
    {
        Legacy = 0,
        Extended = 1,
        /// <summary>Dialeto dos macropads do fabricante 514C, com cabeçalho 0xFD.</summary>
        Ch57x3 = 2
    }
}
