using System.Collections.Generic;
using RSoft.MacroPad.BLL.Infrasturture.Model;
using RSoft.MacroPad.BLL.Infrasturture.Protocol;
using RSoft.MacroPad.BLL.Infrasturture.UsbDevice;

namespace RSoft.MacroPad.BLL.Macros
{
    public static class MacroWriter
    {
        /// <summary>
        /// Monta os reports da macro e grava no macropad. Devolve false se alguma escrita falhar.
        /// </summary>
        public static bool Write(IUsb usb, IReportComposer composer, InputAction action, byte layer, ushort delay, Macro macro)
        {
            IEnumerable<Report> reports;
            switch (macro.Kind)
            {
                case MacroKind.Media:
                    reports = composer.Media(action, layer, macro.MediaKey);
                    break;
                case MacroKind.Mouse:
                    reports = composer.Mouse(action, layer, macro.MouseButton, macro.MouseModifiers);
                    break;
                default:
                    reports = composer.Key(action, layer, delay, macro.Keys);
                    break;
            }

            foreach (var report in reports)
            {
                if (!usb.Write(report))
                    return false;
            }
            return true;
        }

        public static bool WriteLed(IUsb usb, IReportComposer composer, byte layer, LedMode mode, LedColor color)
        {
            foreach (var report in composer.Led(layer, mode, color))
            {
                if (!usb.Write(report))
                    return false;
            }
            return true;
        }
    }
}
