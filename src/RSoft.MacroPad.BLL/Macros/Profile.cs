using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using RSoft.MacroPad.BLL.Infrasturture.Model;

namespace RSoft.MacroPad.BLL.Macros
{
    /// <summary>
    /// Uma configuração inteira salva com nome: todas as teclas de todas as camadas, mais a luz.
    /// </summary>
    public sealed class Profile
    {
        public string Name { get; set; }
        public string LayoutName { get; set; }
        public DateTime SavedAt { get; set; }
        public List<ProfileAssignment> Assignments { get; set; } = new List<ProfileAssignment>();

        /// <summary>Iluminação salva junto. Fica nulo em perfil gravado antes desta versão.</summary>
        public LightScheme Light { get; set; }
    }

    public sealed class ProfileAssignment
    {
        public byte Layer { get; set; }
        public string Action { get; set; }
        public MacroData Macro { get; set; }
    }

    /// <summary>
    /// Perfis salvos como um arquivo .json cada, dentro de uma pasta ao lado do programa.
    /// </summary>
    public sealed class ProfileStore
    {
        private readonly string _folder;

        public ProfileStore(string folder)
        {
            _folder = folder;
        }

        public IReadOnlyList<Profile> List()
        {
            if (!Directory.Exists(_folder))
                return Array.Empty<Profile>();

            return Directory.GetFiles(_folder, "*.json")
                .Select(Read)
                .Where(profile => profile != null)
                .OrderBy(profile => profile.Name)
                .ToList();
        }

        public void Save(Profile profile)
        {
            Directory.CreateDirectory(_folder);
            profile.SavedAt = DateTime.Now;
            File.WriteAllText(PathOf(profile.Name), JsonSerializer.Serialize(profile, new JsonSerializerOptions { WriteIndented = true }));
        }

        public void Delete(string name)
        {
            var path = PathOf(name);
            if (File.Exists(path))
                File.Delete(path);
        }

        public Profile Read(string filePath)
        {
            try
            {
                return JsonSerializer.Deserialize<Profile>(File.ReadAllText(filePath));
            }
            catch (Exception exception) when (exception is JsonException || exception is IOException)
            {
                return null;
            }
        }

        public void Export(Profile profile, string filePath)
        {
            File.WriteAllText(filePath, JsonSerializer.Serialize(profile, new JsonSerializerOptions { WriteIndented = true }));
        }

        /// <summary>Copia um perfil de fora para a pasta do app. Devolve null se o arquivo não servir.</summary>
        public Profile Import(string filePath)
        {
            var profile = Read(filePath);
            if (profile == null || string.IsNullOrWhiteSpace(profile.Name))
                return null;

            Save(profile);
            return profile;
        }

        private string PathOf(string name) => Path.Combine(_folder, SafeFileName(name) + ".json");

        /// <summary>Nome de perfil vira nome de arquivo, então caractere proibido pelo Windows vira "-".</summary>
        public static string SafeFileName(string name)
        {
            var safe = new string(name.Select(c => Path.GetInvalidFileNameChars().Contains(c) ? '-' : c).ToArray()).Trim();
            return safe.Length == 0 ? "perfil" : safe;
        }
    }
}
