using System.Collections.Generic;
using System.IO;
using System.Text.Json;

namespace RSoft.MacroPad.BLL.Macros
{
    public enum RelayKind
    {
        None,
        /// <summary>Digita um texto, com acento e tudo, porque quem digita é o PC.</summary>
        Text,
        /// <summary>Abre um programa, arquivo, pasta ou site.</summary>
        Open,
    }

    public sealed class RelayAction
    {
        public string Kind { get; set; } = RelayKind.None.ToString();
        public string Value { get; set; } = "";

        public RelayKind ParsedKind =>
            System.Enum.TryParse<RelayKind>(Kind, out var kind) ? kind : RelayKind.None;
    }

    /// <summary>
    /// O que o PC faz quando o macropad manda F13 a F24.
    /// Serve para passar do limite do teclado: texto longo, acento, abrir programa.
    /// </summary>
    public sealed class RelayStore
    {
        private readonly string _filePath;
        private readonly Dictionary<string, RelayAction> _actions;

        public RelayStore(string filePath)
        {
            _filePath = filePath;
            _actions = Load(filePath);
        }

        public RelayAction Get(string keyName)
        {
            return _actions.TryGetValue(keyName, out var action) ? action : new RelayAction();
        }

        public void Set(string keyName, RelayKind kind, string value)
        {
            _actions[keyName] = new RelayAction { Kind = kind.ToString(), Value = value ?? "" };
            File.WriteAllText(_filePath, JsonSerializer.Serialize(_actions, new JsonSerializerOptions { WriteIndented = true }));
        }

        /// <summary>True se alguma tecla tem ação: sem isso o app nem precisa ficar ouvindo o teclado.</summary>
        public bool HasAnyAction()
        {
            foreach (var action in _actions.Values)
            {
                if (action.ParsedKind != RelayKind.None && action.Value.Length > 0)
                    return true;
            }
            return false;
        }

        private static Dictionary<string, RelayAction> Load(string filePath)
        {
            if (!File.Exists(filePath))
                return new Dictionary<string, RelayAction>();

            try
            {
                return JsonSerializer.Deserialize<Dictionary<string, RelayAction>>(File.ReadAllText(filePath))
                    ?? new Dictionary<string, RelayAction>();
            }
            catch (JsonException)
            {
                return new Dictionary<string, RelayAction>();
            }
        }
    }
}
