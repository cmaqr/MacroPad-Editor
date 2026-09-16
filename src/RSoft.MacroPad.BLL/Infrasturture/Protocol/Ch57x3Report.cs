using System;
using System.Collections.Generic;
using System.Linq;
using RSoft.MacroPad.BLL.Infrasturture.Model;
using RSoft.MacroPad.BLL.Infrasturture.Protocol.Mappers;

namespace RSoft.MacroPad.BLL.Infrasturture.Protocol
{
    /// <summary>
    /// Report do dialeto usado pelos macropads do fabricante 514C (conhecido como ch57x-3).
    /// Formato: [0] 0xFD, [1] slot, [2] camada, [3] tipo, e daí os dados.
    /// </summary>
    internal class Ch57x3Report : Report
    {
        public const byte WriteCommand = 0xFD;

        private Ch57x3Report() { }

        public static Ch57x3Report Create(byte reportId, params byte[] data)
        {
            var report = new Ch57x3Report { ReportId = reportId };
            for (var i = 0; i < data.Length && i < report.Data.Length; i++)
                report.Data[i] = data[i];
            return report;
        }

        /// <summary>Todo envio termina com este report; sem ele o teclado não guarda nada.</summary>
        public static Ch57x3Report CreateEnd(byte reportId) => Create(reportId, WriteCommand, 0xFE, 0xFF);
    }

    /// <summary>
    /// Monta os reports no dialeto do fabricante 514C. Difere do protocolo do projeto original em tudo
    /// que importa: cabeçalho 0xFD, modificador como passo próprio, 3 bytes por passo e report de encerramento.
    /// </summary>
    public class Ch57x3ReportComposer : IReportComposer
    {
        /// <summary>Máximo de passos que cabe numa tecla, contando os modificadores.</summary>
        public const int MaxSteps = 18;

        private const byte KeyboardType = 1;
        private const byte MediaType = 2;
        private const byte MouseType = 3;

        // A iluminação fala outro comando: cabeçalho 0xFE em vez de 0xFD, com destino fixo 0xB0
        private const byte LedCommand = 0xFE;
        private const byte LedSlot = 0xB0;
        private const byte LedKeyCount = 17;
        private const byte EnterConfig = 0xFB;
        private const byte CommitMark = 0xAA;

        // Nesse dialeto o modificador não é um bit: é um passo com código próprio
        private const byte CtrlStep = 0xF1;
        private const byte ShiftStep = 0xF2;
        private const byte AltStep = 0xF3;
        private const byte WinStep = 0xF4;
        private const byte RightCtrlStep = 0xF5;
        private const byte RightShiftStep = 0xF6;
        private const byte RightAltStep = 0xF7;
        private const byte RightWinStep = 0xF8;

        private readonly byte _reportId;

        public Ch57x3ReportComposer(byte reportId)
        {
            _reportId = reportId;
        }

        public IEnumerable<Report> Key(InputAction action, byte layerNo, ushort delay, IEnumerable<(KeyCode Key, Modifier Modifiers)> sequence)
        {
            var steps = new List<byte>();
            foreach (var (key, modifiers) in sequence)
            {
                foreach (var modifierStep in ModifierSteps(modifiers))
                    steps.Add(modifierStep);
                if (key != KeyCode.None)
                    steps.Add((byte)key);
            }

            // Este teclado não tem ajuste de atraso entre as teclas, por isso o parâmetro delay é ignorado
            var data = new List<byte> { Ch57x3Report.WriteCommand, Slot(action), layerNo, KeyboardType, 0, 0 };
            var count = 0;
            foreach (var step in steps.Take(MaxSteps))
            {
                // Cada passo ocupa três bytes e só o último carrega o código
                data.Add(0);
                data.Add(0);
                data.Add(step);
                count++;
            }
            data[5] = (byte)count;

            return new Report[] { Ch57x3Report.Create(_reportId, data.ToArray()), Ch57x3Report.CreateEnd(_reportId) };
        }

        public IEnumerable<Report> Media(InputAction action, byte layerNo, MediaKey key)
        {
            // Os valores da "versão 3" do projeto original são os códigos de consumo do HID, que é o que este teclado espera
            var low = key.B1(3);
            var high = key.B2(3);

            return new Report[]
            {
                Ch57x3Report.Create(_reportId, Ch57x3Report.WriteCommand, Slot(action), layerNo, MediaType, 0, 2, 0, 0, low, 0, 0, high),
                Ch57x3Report.CreateEnd(_reportId),
            };
        }

        public IEnumerable<Report> Mouse(InputAction action, byte layerNo, MouseButton func, Modifier modifiers)
        {
            var data = new byte[23];
            data[0] = Ch57x3Report.WriteCommand;
            data[1] = Slot(action);
            data[2] = layerNo;
            data[3] = MouseType;
            data[4] = 1;
            data[5] = 4;
            data[9] = MouseModifierStep(modifiers);
            data[13] = func.Button();
            data[22] = func.Scroll();

            return new Report[] { Ch57x3Report.Create(_reportId, data), Ch57x3Report.CreateEnd(_reportId) };
        }

        public IEnumerable<Report> Led(byte layerNo, LightScheme scheme)
        {
            // O comando de luz tem cabeçalho próprio (0xFE, e não 0xFD) e leva um trio RGB por tecla,
            // sempre 17 trios, mesmo em teclado com menos teclas. A camada aqui conta a partir do zero.
            var data = new List<byte> { LedCommand, LedSlot, (byte)(layerNo > 0 ? layerNo - 1 : 0), (byte)scheme.Mode };
            for (var keyNumber = 1; keyNumber <= LedKeyCount; keyNumber++)
            {
                var color = scheme.ColorOf(keyNumber);
                if (color.IsRandom)
                {
                    // Cada tecla ganha uma cor sorteada, já que o teclado não sorteia sozinho
                    data.Add((byte)Random.Shared.Next(256));
                    data.Add((byte)Random.Shared.Next(256));
                    data.Add((byte)Random.Shared.Next(256));
                }
                else
                {
                    data.Add(color.Red);
                    data.Add(color.Green);
                    data.Add(color.Blue);
                }
            }

            return new Report[]
            {
                // Sem este preâmbulo o teclado aceita o comando de luz e joga fora sem avisar
                Ch57x3Report.Create(_reportId, EnterConfig, EnterConfig, EnterConfig),
                Ch57x3Report.Create(_reportId, data.ToArray()),
                Ch57x3Report.Create(_reportId, CommitMark, CommitMark),
                Ch57x3Report.CreateEnd(_reportId),
                Ch57x3Report.Create(_reportId, CommitMark, CommitMark),
            };
        }

        /// <summary>
        /// Neste dialeto as teclas ocupam os slots 1 a 15 e os knobs vêm a partir do 16,
        /// três slots por knob, na ordem girar à esquerda, apertar, girar à direita.
        /// Medido no aparelho em 15/09/2026: a documentação pública dizia 17, e está errada para este modelo.
        /// </summary>
        public const byte FirstKnobSlot = 16;

        public static byte Slot(InputAction action)
        {
            if (action >= InputAction.Knob1Left)
                return (byte)(action - InputAction.Knob1Left + FirstKnobSlot);
            return (byte)action;
        }

        private static IEnumerable<byte> ModifierSteps(Modifier modifiers)
        {
            if ((modifiers & Modifier.LeftCtrl) != 0) yield return CtrlStep;
            if ((modifiers & Modifier.LeftShift) != 0) yield return ShiftStep;
            if ((modifiers & Modifier.LeftAlt) != 0) yield return AltStep;
            if ((modifiers & Modifier.LeftWin) != 0) yield return WinStep;
            if ((modifiers & Modifier.RightCtrl) != 0) yield return RightCtrlStep;
            if ((modifiers & Modifier.RightShift) != 0) yield return RightShiftStep;
            if ((modifiers & Modifier.RightAlt) != 0) yield return RightAltStep;
            if ((modifiers & Modifier.RightWin) != 0) yield return RightWinStep;
        }

        private static byte MouseModifierStep(Modifier modifiers)
        {
            // No clique do mouse só cabe um modificador, e só Ctrl, Shift ou Alt
            if ((modifiers & (Modifier.LeftCtrl | Modifier.RightCtrl)) != 0) return CtrlStep;
            if ((modifiers & (Modifier.LeftShift | Modifier.RightShift)) != 0) return ShiftStep;
            if ((modifiers & (Modifier.LeftAlt | Modifier.RightAlt)) != 0) return AltStep;
            return 0;
        }
    }
}
