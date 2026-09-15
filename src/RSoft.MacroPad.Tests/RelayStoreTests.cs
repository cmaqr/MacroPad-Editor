using System;
using System.IO;
using RSoft.MacroPad.BLL.Macros;
using Xunit;

namespace RSoft.MacroPad.Tests
{
    public class RelayStoreTests : IDisposable
    {
        private readonly string _filePath = Path.Combine(Path.GetTempPath(), $"macropad-relay-{Guid.NewGuid()}.json");

        public void Dispose()
        {
            if (File.Exists(_filePath))
                File.Delete(_filePath);
        }

        // Quebra se a ação ligada ao F13 não sobreviver a fechar e abrir o app
        [Fact]
        public void Set_is_read_back_by_a_new_store()
        {
            var store = new RelayStore(_filePath);

            store.Set("F13", RelayKind.Text, "Olá, tudo bem?");

            var reopened = new RelayStore(_filePath).Get("F13");
            Assert.Equal(RelayKind.Text, reopened.ParsedKind);
            Assert.Equal("Olá, tudo bem?", reopened.Value);
        }

        // Quebra se tecla sem ação passar a devolver nulo e derrubar o app
        [Fact]
        public void Get_returns_empty_action_for_unknown_key()
        {
            var action = new RelayStore(_filePath).Get("F20");

            Assert.Equal(RelayKind.None, action.ParsedKind);
            Assert.Equal("", action.Value);
        }

        // Quebra se o app passar a ficar ouvindo o teclado mesmo sem nenhuma ação configurada
        [Fact]
        public void HasAnyAction_is_false_until_something_is_set()
        {
            var store = new RelayStore(_filePath);
            Assert.False(store.HasAnyAction());

            store.Set("F14", RelayKind.Open, "notepad.exe");

            Assert.True(store.HasAnyAction());
        }

        // Quebra se limpar a ação de uma tecla deixar o app ouvindo o teclado à toa
        [Fact]
        public void HasAnyAction_is_false_again_after_clearing()
        {
            var store = new RelayStore(_filePath);
            store.Set("F14", RelayKind.Open, "notepad.exe");

            store.Set("F14", RelayKind.None, "");

            Assert.False(store.HasAnyAction());
        }
    }
}
