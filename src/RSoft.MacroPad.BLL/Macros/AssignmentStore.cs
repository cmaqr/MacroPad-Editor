using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using RSoft.MacroPad.BLL.Infrasturture.Model;

namespace RSoft.MacroPad.BLL.Macros
{
    /// <summary>
    /// Lembra o que foi enviado para cada tecla e as preferências do app.
    /// O macropad não tem como devolver a configuração, então isso só reflete o que saiu deste PC.
    /// </summary>
    public sealed class AssignmentStore
    {
        private const string LastLayoutKey = "last-layout";

        private readonly string _filePath;
        private readonly StoredData _data;

        public AssignmentStore(string filePath)
        {
            _filePath = filePath;
            _data = Load(filePath);
        }

        public Macro GetMacro(string layoutName, byte layer, InputAction action)
        {
            return _data.Keys.TryGetValue(Key(layoutName, layer, action), out var macro) ? macro.ToMacro() : null;
        }

        public string GetTitle(string layoutName, byte layer, InputAction action)
        {
            return _data.Keys.TryGetValue(Key(layoutName, layer, action), out var macro) ? macro.Title : null;
        }

        public void SetMacro(string layoutName, byte layer, InputAction action, Macro macro)
        {
            _data.Keys[Key(layoutName, layer, action)] = MacroData.From(macro);
            Save();
        }

        /// <summary>Tudo que já foi enviado para um modelo, para virar um perfil.</summary>
        public List<ProfileAssignment> Snapshot(string layoutName)
        {
            var prefix = layoutName + "|";
            var assignments = new List<ProfileAssignment>();

            foreach (var entry in _data.Keys.Where(k => k.Key.StartsWith(prefix, StringComparison.Ordinal)))
            {
                var parts = entry.Key.Split('|');
                if (parts.Length != 3 || !byte.TryParse(parts[1], out var layer) || !Enum.TryParse<InputAction>(parts[2], out var action))
                    continue;
                assignments.Add(new ProfileAssignment { Layer = layer, Action = action.ToString(), Macro = entry.Value });
            }
            return assignments;
        }

        public string GetLastLayoutName() => GetSetting(LastLayoutKey);

        public void SetLastLayoutName(string layoutName) => SetSetting(LastLayoutKey, layoutName);

        /// <summary>Preferência solta do app, como o tema ou o perfil que entra sozinho.</summary>
        public string GetSetting(string key)
        {
            return _data.Settings.TryGetValue(key, out var value) ? value : null;
        }

        public void SetSetting(string key, string value)
        {
            _data.Settings[key] = value;
            Save();
        }

        private void Save()
        {
            File.WriteAllText(_filePath, JsonSerializer.Serialize(_data, new JsonSerializerOptions { WriteIndented = true }));
        }

        private static StoredData Load(string filePath)
        {
            if (!File.Exists(filePath))
                return new StoredData();

            try
            {
                return JsonSerializer.Deserialize<StoredData>(File.ReadAllText(filePath)) ?? new StoredData();
            }
            catch (JsonException)
            {
                // Arquivo corrompido não pode impedir o app de abrir; os nomes voltam a aparecer depois do próximo envio
                return new StoredData();
            }
        }

        private static string Key(string layoutName, byte layer, InputAction action) => $"{layoutName}|{layer}|{action}";

        private sealed class StoredData
        {
            public Dictionary<string, string> Settings { get; set; } = new Dictionary<string, string>();
            public Dictionary<string, MacroData> Keys { get; set; } = new Dictionary<string, MacroData>();
        }
    }
}
