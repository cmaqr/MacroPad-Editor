using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using RSoft.MacroPad.BLL.Infrasturture.Model;
using RSoft.MacroPad.BLL.Infrasturture.Protocol.Mappers;
using RSoft.MacroPad.BLL.Macros;
using RSoft.MacroPad.Infrastructure;
using Windows.Win32;
using Windows.Win32.UI.Input.KeyboardAndMouse;
using Windows.Win32.UI.TextServices;

namespace RSoft.MacroPad.Controls.Pages
{
    /// <summary>
    /// Grava uma combinação de teclas apertada no teclado do PC.
    /// </summary>
    internal class RecordPage : Panel
    {
        private const uint ExtendedKeyFlag = 0x01;

        private readonly Label _title;
        private readonly Label _description;
        private readonly RoundedPanel _display;
        private readonly Label _recorded;
        private readonly Label _recordingHint;
        private readonly PillButton _recordButton;
        private readonly PillButton _undoButton;
        private readonly PillButton _clearButton;
        private readonly PillButton _useButton;

        private readonly List<(KeyCode Key, Modifier Modifiers)> _chords = new List<(KeyCode, Modifier)>();
        private readonly HashSet<Keys> _keysHeld = new HashSet<Keys>();
        private KeyboardHook _hook;
        private HKL _usLayout;
        private Modifier _modifiersHeld = Modifier.None;
        private bool _onlyModifiersPressed;
        private int _maxKeys = 18;

        public event EventHandler<Macro> MacroChosen;

        public RecordPage()
        {
            Theme.Register(this, TextRole.Primary, SurfaceRole.Card);

            _title = NewLabel("Gravar um atalho", Theme.Title, TextRole.Primary);
            _description = NewLabel("Clique em Gravar e aperte a combinação no teclado do PC. Enquanto grava, as teclas não chegam a outros programas.", Theme.Callout, TextRole.Secondary);

            _display = new RoundedPanel { Surface = SurfaceRole.Background, Radius = 14 };
            _recorded = Theme.Register(new Label
            {
                Font = Theme.LargeTitle,
                TextAlign = ContentAlignment.MiddleCenter,
                UseMnemonic = false,
                Dock = DockStyle.Fill,
            }, TextRole.Primary, SurfaceRole.Background);
            _recordingHint = Theme.Register(new Label
            {
                Font = Theme.CaptionSemibold,
                TextAlign = ContentAlignment.MiddleCenter,
                Dock = DockStyle.Bottom,
                Height = Theme.Scale(this, 30),
            }, TextRole.Danger, SurfaceRole.Background);
            _display.Controls.Add(_recorded);
            _display.Controls.Add(_recordingHint);

            _recordButton = new PillButton { Style = PillStyle.Primary, Icon = "\uE7C8", Height = Theme.Scale(this, 38) };
            _recordButton.Click += (s, e) => { if (_hook == null) StartRecording(); else StopRecording(); };

            _undoButton = new PillButton { Text = "Apagar último", Style = PillStyle.Secondary, Height = Theme.Scale(this, 38) };
            _undoButton.FitWidthToText();
            _undoButton.Click += (s, e) => { if (_chords.Count > 0) _chords.RemoveAt(_chords.Count - 1); UpdateState(); };

            _clearButton = new PillButton { Text = "Limpar", Style = PillStyle.Secondary, Height = Theme.Scale(this, 38) };
            _clearButton.FitWidthToText();
            _clearButton.Click += (s, e) => { _chords.Clear(); UpdateState(); };

            _useButton = new PillButton { Text = "Usar gravação", Style = PillStyle.Primary, Height = Theme.Scale(this, 38) };
            _useButton.FitWidthToText();
            _useButton.Click += (s, e) => ChooseRecording();

            Controls.AddRange(new Control[] { _title, _description, _display, _recordButton, _undoButton, _clearButton, _useButton });
            UpdateState();
        }

        public void SetMaxKeys(int maxKeys)
        {
            _maxKeys = maxKeys;
            if (_chords.Count > maxKeys)
                _chords.RemoveRange(maxKeys, _chords.Count - maxKeys);
            UpdateState();
        }

        public void StopRecording()
        {
            if (_hook == null)
                return;
            _hook.Dispose();
            _hook = null;
            _keysHeld.Clear();
            _modifiersHeld = Modifier.None;
            UpdateState();
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
                StopRecording();
            base.Dispose(disposing);
        }

        protected override void OnLayout(LayoutEventArgs levent)
        {
            base.OnLayout(levent);
            if (_useButton == null)
                return;

            var width = ClientSize.Width;
            var gap = Theme.Scale(this, 8);
            _title.SetBounds(0, 0, width, Theme.Scale(this, 30));
            _description.SetBounds(0, _title.Bottom, width, Theme.Scale(this, 40));
            _display.SetBounds(0, _description.Bottom + gap, width, Theme.Scale(this, 150));

            var buttonsTop = _display.Bottom + Theme.Scale(this, 16);
            _recordButton.Location = new Point(0, buttonsTop);
            _undoButton.Location = new Point(_recordButton.Right + gap, buttonsTop);
            _clearButton.Location = new Point(_undoButton.Right + gap, buttonsTop);
            _useButton.Location = new Point(width - _useButton.Width, buttonsTop);
        }

        private void StartRecording()
        {
            // O macropad usa a posição física da tecla. Traduzir pelo layout americano faz um teclado ABNT2
            // gravar a mesma tecla que ele toca. Mesma técnica do projeto original (MainForm.tsSend_Click).
            var currentLayout = PInvoke.GetKeyboardLayout(0);
            _usLayout = PInvoke.LoadKeyboardLayout("00000409", ACTIVATE_KEYBOARD_LAYOUT_FLAGS.KLF_ACTIVATE);
            PInvoke.ActivateKeyboardLayout(currentLayout, ACTIVATE_KEYBOARD_LAYOUT_FLAGS.KLF_ACTIVATE);

            _modifiersHeld = Modifier.None;
            _keysHeld.Clear();
            _hook = new KeyboardHook();
            _hook.OnKeyPressRelease += CaptureKey;
            UpdateState();
        }

        private bool CaptureKey(KeyInfo keyInfo)
        {
            var modifier = ModifierOf(keyInfo.key);

            if (modifier != Modifier.None)
            {
                if (keyInfo.IsKeyPress)
                {
                    if ((_modifiersHeld & modifier) == 0 && _keysHeld.Count == 0)
                        _onlyModifiersPressed = true;
                    _modifiersHeld |= modifier;
                }
                else
                {
                    // Soltou um modificador sem apertar outra tecla junto: grava o modificador sozinho (ex.: só Win)
                    if (_onlyModifiersPressed)
                        AddChord(KeyCode.None, _modifiersHeld);
                    _onlyModifiersPressed = false;
                    _modifiersHeld &= ~modifier;
                }
                return false;
            }

            if (keyInfo.IsKeyRelease)
            {
                _keysHeld.Remove(keyInfo.key);
                return false;
            }

            // Segurar a tecla gera vários "apertou" seguidos; só o primeiro conta
            if (!_keysHeld.Add(keyInfo.key))
                return false;

            _onlyModifiersPressed = false;
            var scanCode = (keyInfo.flags & ExtendedKeyFlag) != 0 ? 0xE000 | keyInfo.scanCode : keyInfo.scanCode;
            var virtualKey = PInvoke.MapVirtualKeyEx(scanCode, MAP_VIRTUAL_KEY_TYPE.MAPVK_VSC_TO_VK_EX, _usLayout);
            var keyCode = RecordedKeyMapper.Map((VirtualKey)keyInfo.key, (VirtualKey)virtualKey);
            if (keyCode != KeyCode.None)
                AddChord(keyCode, _modifiersHeld);

            // false = a tecla não segue para os outros programas
            return false;
        }

        private void AddChord(KeyCode key, Modifier modifiers)
        {
            if (_chords.Count >= _maxKeys)
                return;
            _chords.Add((key, modifiers));
            if (_chords.Count >= _maxKeys)
                StopRecording();
            UpdateState();
        }

        private void UpdateState()
        {
            var recording = _hook != null;
            _recordButton.Text = recording ? "Parar" : "Gravar";
            _recordButton.Icon = recording ? "\uE71A" : "\uE7C8";
            _recordButton.Style = recording ? PillStyle.Secondary : PillStyle.Primary;
            _recordButton.FitWidthToText();

            _recorded.Text = _chords.Count == 0
                ? (recording ? "Aperte as teclas…" : "Nada gravado ainda")
                : ShortcutText.Describe(_chords);
            Theme.SetTextRole(_recorded, _chords.Count == 0 ? TextRole.Tertiary : TextRole.Primary);
            _recorded.Font = _chords.Count > 2 ? Theme.Title : Theme.LargeTitle;

            _recordingHint.Text = recording
                ? $"● GRAVANDO  ·  {_chords.Count} de {_maxKeys} teclas  ·  clique em Parar quando terminar"
                : _chords.Count > 0 ? $"{_chords.Count} de {_maxKeys} teclas" : "";
            Theme.SetTextRole(_recordingHint, recording ? TextRole.Danger : TextRole.Secondary);

            _undoButton.Enabled = _chords.Count > 0;
            _clearButton.Enabled = _chords.Count > 0;
            _useButton.Enabled = _chords.Count > 0 && !recording;
            PerformLayout();
        }

        private void ChooseRecording()
        {
            MacroChosen?.Invoke(this, new Macro
            {
                Id = "custom-recording",
                Title = "Atalho gravado",
                Category = "Gravado",
                Icon = "\uE7C8",
                Kind = MacroKind.Keys,
                Keys = _chords.ToArray(),
            });
        }

        private static Modifier ModifierOf(Keys key)
        {
            switch (key)
            {
                case Keys.LControlKey: return Modifier.LeftCtrl;
                case Keys.RControlKey: return Modifier.RightCtrl;
                case Keys.LShiftKey: return Modifier.LeftShift;
                case Keys.RShiftKey: return Modifier.RightShift;
                case Keys.LMenu: return Modifier.LeftAlt;
                case Keys.RMenu: return Modifier.RightAlt;
                case Keys.LWin: return Modifier.LeftWin;
                case Keys.RWin: return Modifier.RightWin;
                default: return Modifier.None;
            }
        }

        private static Label NewLabel(string text, Font font, TextRole role)
        {
            return Theme.Register(new Label { Text = text, Font = font, UseMnemonic = false }, role, SurfaceRole.Card);
        }
    }
}
