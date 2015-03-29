using System;
using System.Linq;
using LeagueSharp;
using LeagueSharp.Common;
using SharpDX;

namespace TrainingBuddy
{
    internal class Program
    {
        public static Vector3 Position = new Vector2(6922, 2689).To3D();
        public static int LastCast;

        public static Menu Menu;

        public static Obj_AI_Hero Player
        {
            get { return ObjectManager.Player; }
        }


        private static void Main(string[] args)
        {
            CustomEvents.Game.OnGameLoad += Game_OnGameLoad;
        }

        private static void Game_OnGameLoad(EventArgs args)
        {
            Menu = new Menu("TrainingBuddy", "TrainingBuddy", true);
            Menu.AddItem(new MenuItem("0", "Use Q", true).SetValue(true));
            Menu.AddItem(new MenuItem("1", "Use W", true).SetValue(true));
            Menu.AddItem(new MenuItem("2", "Use E", true).SetValue(true));
            Menu.AddItem(new MenuItem("3", "Use R", true).SetValue(true));
            Menu.AddItem(new MenuItem("Delay", "Delay in seconds").SetValue(new Slider(5, 0, 15)));
            Menu.AddToMainMenu();

            Game.OnUpdate += Game_OnUpdate;
        }

        private static void Game_OnUpdate(EventArgs args)
        {
            BuyRanduins();
            Move();
            Cast();
        }

        public static void Cast()
        {
            if (Player.IsDead)
            {
                var trinket = Player.Spellbook.GetSpell(SpellSlot.Trinket);
                
                if (trinket == null || !trinket.IsReady() || trinket.Ammo < 1)
                {
                    return;
                }

                Player.Spellbook.CastSpell(SpellSlot.Trinket);
                return;
            }
            
            if (Player.Distance(Position) >= 50 ||
                Environment.TickCount - LastCast < Menu.Item("Delay").GetValue<Slider>().Value * 1000)
            {
                return;
            }

            var spells = new[] { SpellSlot.Q, SpellSlot.W, SpellSlot.E, SpellSlot.R };
            var target = ObjectManager.Get<Obj_AI_Hero>().FirstOrDefault(h => h.IsValidTarget());

            if (target == null)
            {
                return;
            }

            foreach (var s in
                spells.Where(s => Player.Spellbook.GetSpell(s).IsReady() && Menu.Item(((int) s).ToString(), true).IsActive())
                    .Select(spell => Player.Spellbook.GetSpell(spell)))
            {
                switch (s.SData.TargettingType)
                {
                    case SpellDataTargetType.Self:
                        Player.Spellbook.CastSpell(s.Slot, Player);
                        break;
                    case SpellDataTargetType.Location:
                        Player.Spellbook.CastSpell(s.Slot, target.ServerPosition);
                        break;
                    case SpellDataTargetType.Location2:
                        Player.Spellbook.CastSpell(s.Slot, target.ServerPosition);
                        break;
                    case SpellDataTargetType.Cone:
                        Player.Spellbook.CastSpell(s.Slot, target.ServerPosition);
                        break;
                    case SpellDataTargetType.LocationAoe:
                        Player.Spellbook.CastSpell(s.Slot, target.ServerPosition);
                        break;
                    case SpellDataTargetType.Unit:
                        Player.Spellbook.CastSpell(s.Slot, target);
                        break;
                }
            }

            LastCast = Environment.TickCount;
        }

        public static void Move()
        {
            if (Player.IsDead || Player.GetWaypoints().Last().Distance(Position) < 50)
            {
                return;
            }

            Player.IssueOrder(GameObjectOrder.MoveTo, Position);
        }

        public static void BuyRanduins()
        {
            if (!Player.InShop())
            {
                return;
            }

            var gb = Player.InventoryItems.FirstOrDefault(h => h.Id.Equals(ItemId.Giants_Belt));
            var wm = Player.InventoryItems.FirstOrDefault(h => h.Id.Equals(ItemId.Wardens_Mail));
            var cloth = Player.InventoryItems.Count(h => h.Id.Equals(ItemId.Cloth_Armor));
            var ruby = Player.InventoryItems.FirstOrDefault(h => h.Id.Equals(ItemId.Ruby_Crystal));

            if (wm == null)
            {
                if (cloth < 2)
                {
                    Player.BuyItem(ItemId.Cloth_Armor);
                    return;
                }
                Player.BuyItem(ItemId.Wardens_Mail);
                return;
            }

            if (gb == null)
            {
                if (ruby == null)
                {
                    Player.BuyItem(ItemId.Ruby_Crystal);
                    return;
                }
                Player.BuyItem(ItemId.Giants_Belt);
                return;
            }

            Player.BuyItem(ItemId.Randuins_Omen);
        }
    }
}