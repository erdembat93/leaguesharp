using System;
using System.Collections.Generic;
using System.Linq;
using LeagueSharp;
using LeagueSharp.Common;

namespace ItemDelayer
{
    internal class Program
    {
        public static Menu Menu;

        public static List<ItemId> SupportedItems = new List<ItemId>
        {
            ItemId.Mikaels_Crucible,
            ItemId.Quicksilver_Sash,
            ItemId.Mercurial_Scimitar
        };

        public static int Delay
        {
            get { return Menu.Item("Delay").GetValue<Slider>().Value; }
        }

        private static void Main(string[] args)
        {
            CustomEvents.Game.OnGameLoad += Game_OnGameLoad;
        }

        private static void Game_OnGameLoad(EventArgs args)
        {
            Menu = new Menu("ItemDelayer", "ItemDelayer", true);
            Menu.AddItem(new MenuItem("Enabled", "Enabled").SetValue(true));
            Menu.AddItem(new MenuItem("Delay", "Delay (ms)").SetValue(new Slider(0, 200, 500)));
            Menu.AddToMainMenu();

            Spellbook.OnCastSpell += Spellbook_OnCastSpell;
        }

        private static void Spellbook_OnCastSpell(Spellbook sender, SpellbookCastSpellEventArgs args)
        {
            if (!Menu.Item("Enabled").IsActive() || !sender.Owner.IsMe || args.Slot < SpellSlot.Item1 ||
                args.Slot > SpellSlot.Trinket)
            {
                return;
            }

            var item = ObjectManager.Player.InventoryItems.FirstOrDefault(i => i.SpellSlot.Equals(args.Slot));
            
            if (item == null || !SupportedItems.Contains(item.Id))
            {
                return;
            }
            
            args.Process = false;
            Utility.DelayAction.Add(Delay, () => Cast(args.Slot, args.Target));
        }

        public static void Cast(SpellSlot slot, GameObject target)
        {
            if (!slot.IsReady())
            {
                return;
            }

            if (target != null && target.IsValid)
            {
                ObjectManager.Player.Spellbook.CastSpell(slot, target, false);
                return;
            }

            ObjectManager.Player.Spellbook.CastSpell(slot, false);
        }
    }
}