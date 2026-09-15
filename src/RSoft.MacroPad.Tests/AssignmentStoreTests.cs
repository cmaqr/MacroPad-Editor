using System;
using System.IO;
using System.Linq;
using RSoft.MacroPad.BLL.Infrasturture.Model;
using RSoft.MacroPad.BLL.Macros;
using Xunit;

namespace RSoft.MacroPad.Tests
{
    public class AssignmentStoreTests : IDisposable
    {
        private readonly string _filePath = Path.Combine(Path.GetTempPath(), $"macropad-test-{Guid.NewGuid()}.json");
        private readonly Macro _copy = MacroCatalog.Find("copy");

        public void Dispose()
        {
            if (File.Exists(_filePath))
                File.Delete(_filePath);
        }

        // Quebra se o que foi enviado para a tecla não sobreviver a fechar e abrir o app
        [Fact]
        public void SetMacro_is_read_back_by_a_new_store()
        {
            var store = new AssignmentStore(_filePath);

            store.SetMacro("3 buttons 1 knob", 1, InputAction.Key2, _copy);
            var reopened = new AssignmentStore(_filePath);

            Assert.Equal("Copiar", reopened.GetTitle("3 buttons 1 knob", 1, InputAction.Key2));
            Assert.Equal(_copy.Keys, reopened.GetMacro("3 buttons 1 knob", 1, InputAction.Key2).Keys);
        }

        // Quebra se a macro voltar do arquivo sem o tipo certo (tecla de mídia virando sequência de teclas)
        [Fact]
        public void GetMacro_keeps_media_macros()
        {
            var store = new AssignmentStore(_filePath);
            var mute = MacroCatalog.Find("mute");

            store.SetMacro("3 buttons 1 knob", 1, InputAction.Key1, mute);
            var restored = new AssignmentStore(_filePath).GetMacro("3 buttons 1 knob", 1, InputAction.Key1);

            Assert.Equal(MacroKind.Media, restored.Kind);
            Assert.Equal(MediaKey.VolMute, restored.MediaKey);
        }

        // Quebra se o nome de uma camada aparecer em outra
        [Fact]
        public void GetTitle_returns_null_for_other_layer()
        {
            var store = new AssignmentStore(_filePath);
            store.SetMacro("3 buttons 1 knob", 1, InputAction.Key2, _copy);

            Assert.Null(store.GetTitle("3 buttons 1 knob", 2, InputAction.Key2));
        }

        // Quebra se a foto da configuração (que vira perfil) pegar teclas de outro modelo junto
        [Fact]
        public void Snapshot_returns_only_the_given_layout()
        {
            var store = new AssignmentStore(_filePath);
            store.SetMacro("3 buttons 1 knob", 1, InputAction.Key1, _copy);
            store.SetMacro("12 buttons 2 knobs", 2, InputAction.Key5, _copy);

            var snapshot = store.Snapshot("12 buttons 2 knobs");

            Assert.Single(snapshot);
            Assert.Equal(2, snapshot[0].Layer);
            Assert.Equal(InputAction.Key5.ToString(), snapshot[0].Action);
        }

        // Quebra se o app esquecer o modelo escolhido à mão (ex.: 12 teclas 2 knobs, que não é detectado sozinho)
        [Fact]
        public void SetLastLayoutName_is_read_back_by_a_new_store()
        {
            var store = new AssignmentStore(_filePath);

            store.SetLastLayoutName("12 buttons 2 knobs");

            Assert.Equal("12 buttons 2 knobs", new AssignmentStore(_filePath).GetLastLayoutName());
        }

        // Quebra se um arquivo corrompido impedir o app de abrir
        [Fact]
        public void Constructor_ignores_corrupted_file()
        {
            File.WriteAllText(_filePath, "{ isso não é json");

            var store = new AssignmentStore(_filePath);

            Assert.Null(store.GetTitle("3 buttons 1 knob", 1, InputAction.Key1));
        }
    }
}
