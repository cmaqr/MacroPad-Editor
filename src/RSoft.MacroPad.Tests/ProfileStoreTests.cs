using System;
using System.IO;
using System.Linq;
using RSoft.MacroPad.BLL.Infrasturture.Model;
using RSoft.MacroPad.BLL.Macros;
using Xunit;

namespace RSoft.MacroPad.Tests
{
    public class ProfileStoreTests : IDisposable
    {
        private readonly string _folder = Path.Combine(Path.GetTempPath(), $"macropad-profiles-{Guid.NewGuid()}");

        public void Dispose()
        {
            if (Directory.Exists(_folder))
                Directory.Delete(_folder, recursive: true);
        }

        private static Profile NewProfile(string name) => new Profile
        {
            Name = name,
            LayoutName = "12 buttons 2 knobs",
            Assignments =
            {
                new ProfileAssignment { Layer = 1, Action = InputAction.Key1.ToString(), Macro = MacroData.From(MacroCatalog.Find("copy")) },
            },
        };

        // Quebra se salvar e listar perfis parar de funcionar
        [Fact]
        public void Save_then_List_returns_the_profile()
        {
            var store = new ProfileStore(_folder);

            store.Save(NewProfile("Trabalho"));

            var profiles = store.List();
            Assert.Single(profiles);
            Assert.Equal("Trabalho", profiles[0].Name);
            Assert.Equal("Copiar", profiles[0].Assignments[0].Macro.Title);
        }

        // Quebra se salvar duas vezes com o mesmo nome criar perfil repetido em vez de substituir
        [Fact]
        public void Save_twice_with_same_name_replaces_it()
        {
            var store = new ProfileStore(_folder);

            store.Save(NewProfile("Trabalho"));
            store.Save(NewProfile("Trabalho"));

            Assert.Single(store.List());
        }

        // Quebra se nome com caractere proibido pelo Windows (barra, dois pontos) derrubar o salvamento
        [Fact]
        public void Save_accepts_names_with_invalid_file_characters()
        {
            var store = new ProfileStore(_folder);

            store.Save(NewProfile("Casa/Trabalho: dia a dia"));

            Assert.Equal("Casa/Trabalho: dia a dia", store.List()[0].Name);
        }

        // Quebra se exportar e importar perder as teclas do perfil
        [Fact]
        public void Export_and_Import_keep_the_assignments()
        {
            var store = new ProfileStore(_folder);
            var exportPath = Path.Combine(_folder, "exportado.perfil.json");
            Directory.CreateDirectory(_folder);
            store.Export(NewProfile("Compartilhado"), exportPath);

            var imported = store.Import(exportPath);

            Assert.Equal("Compartilhado", imported.Name);
            Assert.Equal(InputAction.Key1.ToString(), imported.Assignments[0].Action);
        }

        // Quebra se um arquivo qualquer arrastado para a janela derrubar o app
        [Fact]
        public void Import_returns_null_for_a_file_that_is_not_a_profile()
        {
            Directory.CreateDirectory(_folder);
            var path = Path.Combine(_folder, "qualquer.json");
            File.WriteAllText(path, "isso não é perfil");

            Assert.Null(new ProfileStore(_folder).Import(path));
        }

        // Quebra se excluir perfil deixar de funcionar
        [Fact]
        public void Delete_removes_the_profile()
        {
            var store = new ProfileStore(_folder);
            store.Save(NewProfile("Some"));

            store.Delete("Some");

            Assert.Empty(store.List());
        }
    }
}
