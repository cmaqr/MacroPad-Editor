using System.Collections.Generic;
using System.Linq;
using RSoft.MacroPad.BLL.Infrasturture.Model;
using RSoft.MacroPad.BLL.Infrasturture.Physical;
using RSoft.MacroPad.BLL.Macros;
using Xunit;

namespace RSoft.MacroPad.Tests
{
    public class MacroCatalogTests
    {
        // Maior sequência aceita pelos modelos com protocolo estendido (layouts.txt)
        private const int LargestSequenceSupported = 18;

        // Quebra se alguém copiar e colar uma macro e esquecer de trocar o id
        [Fact]
        public void Macros_have_unique_ids()
        {
            var ids = MacroCatalog.Macros.Select(m => m.Id).ToList();

            var duplicated = ids.GroupBy(id => id).Where(g => g.Count() > 1).Select(g => g.Key).ToList();

            Assert.Empty(duplicated);
        }

        // Quebra se um kit apontar para uma macro que não existe (a tecla ficaria sem nada)
        [Fact]
        public void Kits_reference_existing_macros()
        {
            var knownIds = MacroCatalog.Macros.Select(m => m.Id).ToHashSet();

            var missing = new List<string>();
            foreach (var kit in MacroCatalog.Kits)
            {
                missing.AddRange(kit.ButtonMacroIds.Where(id => !knownIds.Contains(id)));
                foreach (var (left, push, right) in kit.KnobMacroIds)
                    missing.AddRange(new[] { left, push, right }.Where(id => !knownIds.Contains(id)));
            }

            Assert.Empty(missing);
        }

        // Quebra se uma macro de teclas pronta ficar vazia ou maior do que o teclado aceita
        [Fact]
        public void Key_macros_are_not_empty_and_fit_largest_keypad()
        {
            var keyMacros = MacroCatalog.Macros.Where(m => m.Kind == MacroKind.Keys);

            var invalid = keyMacros.Where(m => m.Keys.Count == 0 || m.Keys.Count > LargestSequenceSupported).Select(m => m.Id).ToList();

            Assert.Empty(invalid);
        }

        // Quebra se uma macro usar categoria que não aparece na lista de filtros
        [Fact]
        public void Macros_use_listed_categories()
        {
            var categories = MacroCatalog.Categories.ToHashSet();

            var unlisted = MacroCatalog.Macros.Where(m => !categories.Contains(m.Category)).Select(m => m.Id).ToList();

            Assert.Empty(unlisted);
        }

        // Quebra se um texto pronto tiver caractere que não dá para digitar (ele sumiria sem aviso)
        [Fact]
        public void Text_macros_only_use_supported_characters()
        {
            var textMacros = MacroCatalog.Macros.Where(m => m.Category == "Texto");

            var invalid = textMacros.Where(m => TextTyper.ToKeys(m.Title).UnsupportedCharacters != "").Select(m => m.Id).ToList();

            Assert.Empty(invalid);
        }

        // Quebra se o kit deixar de preencher botões na ordem ou trocar esquerda/direita do knob
        [Fact]
        public void KitPlanner_fills_buttons_in_order_and_knob_left_push_right()
        {
            var layout = new KeyboardLayout
            {
                Name = "3 buttons 1 knob",
                Controls = new List<PhysicalControl> { new PhysicalButton(2), new PhysicalButton(1), new PhysicalButton(3), new PhysicalKnob(1) },
            };
            var kit = MacroCatalog.Kits.First(k => k.Id == "kit-media");

            var plan = KitPlanner.Plan(kit, layout);

            Assert.Equal(new[] { InputAction.Key1, InputAction.Key2, InputAction.Key3, InputAction.Knob1Left, InputAction.Knob1Push, InputAction.Knob1Right }, plan.Select(p => p.Action));
            Assert.Equal(new[] { "previous-track", "play-pause", "next-track", "volume-down", "mute", "volume-up" }, plan.Select(p => p.Macro.Id));
        }
    }
}
