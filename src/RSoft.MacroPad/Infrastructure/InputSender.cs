using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Threading;
using RSoft.MacroPad.BLL.Infrasturture.Model;
using RSoft.MacroPad.BLL.Infrasturture.Protocol.Mappers;

namespace RSoft.MacroPad.Infrastructure
{
    /// <summary>
    /// Faz o próprio PC digitar: usado para testar uma macro e para as ações das teclas F13 a F24.
    /// </summary>
    internal static class InputSender
    {
        private const uint InputKeyboard = 1;
        private const uint KeyEventKeyUp = 0x0002;
        private const uint KeyEventUnicode = 0x0004;

        /// <summary>
        /// Digita o texto como se fosse o teclado. Vai caractere a caractere em Unicode,
        /// então acento, ç e emoji funcionam, ao contrário do que o macropad consegue guardar.
        /// </summary>
        public static void TypeText(string text)
        {
            if (string.IsNullOrEmpty(text))
                return;

            var inputs = new List<Input>();
            foreach (var character in text)
            {
                if (character == '\n')
                {
                    inputs.Add(KeyInput((ushort)VirtualKey.Return, false));
                    inputs.Add(KeyInput((ushort)VirtualKey.Return, true));
                    continue;
                }
                if (character == '\r')
                    continue;

                inputs.Add(UnicodeInput(character, false));
                inputs.Add(UnicodeInput(character, true));
            }
            Send(inputs);
        }

        /// <summary>
        /// Executa uma sequência de combinações no PC, com uma pausa entre elas.
        /// </summary>
        public static void SendChords(IEnumerable<(KeyCode Key, Modifier Modifiers)> chords, int delayBetweenChords)
        {
            foreach (var (key, modifiers) in chords)
            {
                var held = new List<ushort>();
                foreach (var modifier in ModifierKeys(modifiers))
                {
                    held.Add(modifier);
                    Send(new[] { KeyInput(modifier, false) });
                }

                if (key != KeyCode.None)
                {
                    var virtualKey = (ushort)key.Map();
                    Send(new[] { KeyInput(virtualKey, false), KeyInput(virtualKey, true) });
                }

                held.Reverse();
                foreach (var modifier in held)
                    Send(new[] { KeyInput(modifier, true) });

                Thread.Sleep(Math.Max(10, delayBetweenChords));
            }
        }

        public static void SendMediaKey(MediaKey mediaKey)
        {
            var virtualKey = (ushort)mediaKey.Map();
            Send(new[] { KeyInput(virtualKey, false), KeyInput(virtualKey, true) });
        }

        private static IEnumerable<ushort> ModifierKeys(Modifier modifiers)
        {
            if ((modifiers & (Modifier.LeftCtrl | Modifier.RightCtrl)) != 0) yield return (ushort)VirtualKey.ControlKey;
            if ((modifiers & (Modifier.LeftShift | Modifier.RightShift)) != 0) yield return (ushort)VirtualKey.ShiftKey;
            if ((modifiers & (Modifier.LeftAlt | Modifier.RightAlt)) != 0) yield return (ushort)VirtualKey.Menu;
            if ((modifiers & (Modifier.LeftWin | Modifier.RightWin)) != 0) yield return (ushort)VirtualKey.LWin;
        }

        private static Input KeyInput(ushort virtualKey, bool release)
        {
            return new Input
            {
                Type = InputKeyboard,
                Data = new InputData
                {
                    Keyboard = new KeyboardInput
                    {
                        VirtualKey = virtualKey,
                        Flags = release ? KeyEventKeyUp : 0,
                    },
                },
            };
        }

        private static Input UnicodeInput(char character, bool release)
        {
            return new Input
            {
                Type = InputKeyboard,
                Data = new InputData
                {
                    Keyboard = new KeyboardInput
                    {
                        ScanCode = character,
                        Flags = KeyEventUnicode | (release ? KeyEventKeyUp : 0),
                    },
                },
            };
        }

        private static void Send(IReadOnlyList<Input> inputs)
        {
            if (inputs.Count == 0)
                return;
            var array = new Input[inputs.Count];
            for (var i = 0; i < inputs.Count; i++)
                array[i] = inputs[i];
            SendInput((uint)array.Length, array, Marshal.SizeOf<Input>());
        }

        #region Externals

        [DllImport("user32.dll", SetLastError = true)]
        private static extern uint SendInput(uint count, Input[] inputs, int size);

        [StructLayout(LayoutKind.Sequential)]
        private struct Input
        {
            public uint Type;
            public InputData Data;
        }

        // A API do Windows usa uma união: teclado, mouse e hardware ocupam o mesmo espaço
        [StructLayout(LayoutKind.Explicit)]
        private struct InputData
        {
            [FieldOffset(0)] public MouseInput Mouse;
            [FieldOffset(0)] public KeyboardInput Keyboard;
            [FieldOffset(0)] public HardwareInput Hardware;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct KeyboardInput
        {
            public ushort VirtualKey;
            public ushort ScanCode;
            public uint Flags;
            public uint Time;
            public IntPtr ExtraInfo;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct MouseInput
        {
            public int X;
            public int Y;
            public uint Data;
            public uint Flags;
            public uint Time;
            public IntPtr ExtraInfo;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct HardwareInput
        {
            public uint Message;
            public ushort ParamLow;
            public ushort ParamHigh;
        }

        #endregion
    }
}
