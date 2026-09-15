using System.Collections.Generic;
using System.Linq;
using RSoft.MacroPad.BLL.Infrasturture.Model;
using RSoft.MacroPad.BLL.Infrasturture.Physical;

namespace RSoft.MacroPad.BLL.Macros
{
    public static class KitPlanner
    {
        /// <summary>
        /// Distribui as macros do kit pelos botões e knobs do layout, na ordem dos números das teclas.
        /// Botões ou knobs além do que o kit define ficam de fora.
        /// </summary>
        public static List<(InputAction Action, Macro Macro)> Plan(MacroKit kit, KeyboardLayout layout)
        {
            var plan = new List<(InputAction, Macro)>();

            var buttons = layout.Controls.OfType<PhysicalButton>().OrderBy(b => b.Actions.First()).ToList();
            for (var i = 0; i < buttons.Count && i < kit.ButtonMacroIds.Count; i++)
            {
                plan.Add((buttons[i].Actions.First(), MacroCatalog.Find(kit.ButtonMacroIds[i])));
            }

            var knobs = layout.Controls.OfType<PhysicalKnob>().OrderBy(k => k.Actions.First()).ToList();
            for (var i = 0; i < knobs.Count && i < kit.KnobMacroIds.Count; i++)
            {
                // As ações do knob vêm sempre na ordem: girar à esquerda, apertar, girar à direita
                var actions = knobs[i].Actions.ToArray();
                plan.Add((actions[0], MacroCatalog.Find(kit.KnobMacroIds[i].Left)));
                plan.Add((actions[1], MacroCatalog.Find(kit.KnobMacroIds[i].Push)));
                plan.Add((actions[2], MacroCatalog.Find(kit.KnobMacroIds[i].Right)));
            }

            return plan;
        }
    }
}
