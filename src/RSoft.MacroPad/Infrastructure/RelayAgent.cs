using System;
using System.Diagnostics;
using System.Linq;
using System.Windows.Forms;
using RSoft.MacroPad.BLL.Macros;

namespace RSoft.MacroPad.Infrastructure
{
    /// <summary>
    /// Ouve as teclas F13 a F24 (que teclado comum não tem) e faz o PC executar a ação escolhida.
    /// É o que permite passar do limite do macropad: texto longo, com acento, ou abrir um programa.
    /// Só fica ativo enquanto existir alguma ação configurada e o app estiver aberto.
    /// </summary>
    internal sealed class RelayAgent : IDisposable
    {
        private static readonly Keys[] WatchedKeys =
        {
            Keys.F13, Keys.F14, Keys.F15, Keys.F16, Keys.F17, Keys.F18,
            Keys.F19, Keys.F20, Keys.F21, Keys.F22, Keys.F23, Keys.F24,
        };

        private readonly RelayStore _store;
        private readonly Control _uiThreadOwner;
        private KeyboardHook _hook;

        public bool IsListening => _hook != null;

        public event EventHandler<string> ActionExecuted;

        public RelayAgent(RelayStore store, Control uiThreadOwner)
        {
            _store = store;
            _uiThreadOwner = uiThreadOwner;
        }

        /// <summary>Liga ou desliga conforme existir ação configurada.</summary>
        public void Refresh()
        {
            if (_store.HasAnyAction())
                Start();
            else
                Stop();
        }

        public void Dispose() => Stop();

        private void Start()
        {
            if (_hook != null)
                return;
            _hook = new KeyboardHook();
            _hook.OnKeyPressRelease += HandleKey;
        }

        private void Stop()
        {
            if (_hook == null)
                return;
            _hook.Dispose();
            _hook = null;
        }

        private bool HandleKey(KeyInfo keyInfo)
        {
            if (!WatchedKeys.Contains(keyInfo.key))
                return true;

            var action = _store.Get(keyInfo.key.ToString());
            if (action.ParsedKind == RelayKind.None || action.Value.Length == 0)
                return true;

            // Só o "apertou" executa; o "soltou" é engolido para a tecla não vazar para outros programas
            if (keyInfo.IsKeyPress)
            {
                // Sai do gancho antes de digitar: função de gancho precisa responder rápido,
                // senão o Windows desliga o gancho no meio de um texto longo
                _uiThreadOwner.BeginInvoke((Action)(() => Execute(keyInfo.key.ToString(), action)));
            }
            return false;
        }

        private void Execute(string keyName, RelayAction action)
        {
            try
            {
                if (action.ParsedKind == RelayKind.Text)
                {
                    InputSender.TypeText(action.Value);
                    ActionExecuted?.Invoke(this, $"{keyName}: texto digitado");
                    return;
                }

                Process.Start(new ProcessStartInfo(action.Value) { UseShellExecute = true });
                ActionExecuted?.Invoke(this, $"{keyName}: abriu {action.Value}");
            }
            catch (Exception exception)
            {
                ActionExecuted?.Invoke(this, $"{keyName}: não deu certo ({exception.Message})");
            }
        }
    }
}
