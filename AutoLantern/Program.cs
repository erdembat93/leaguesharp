#region

using System;
using System.Linq;
using LeagueSharp;
using LeagueSharp.Common;

#endregion

namespace AutoLantern
{
    internal class Program
    {
        private const String LanternName = "ThreshLantern";
        private static Menu _menu;
        private static Obj_AI_Hero _player;

        private static void Main(string[] args)
        {
            CustomEvents.Game.OnGameLoad += OnGameLoad;
        }

        private static void OnGameLoad(EventArgs args)
        {
            if (!ThreshInGame())
            {
                return;
            }

            _menu = new Menu("AutoLantern", "AutoLantern", true);
            _menu.AddItem(new MenuItem("Auto", "Auto-Lantern at Low HP").SetValue(true));
            _menu.AddItem(new MenuItem("Low", "Low HP Percent").SetValue(new Slider(20, 30, 5)));
            _menu.AddItem(new MenuItem("Hotkey", "Hotkey").SetValue(new KeyBind(32, KeyBindType.Press)));
            _menu.AddToMainMenu();

            Game.OnUpdate += OnGameUpdate;

            Game.PrintChat("AutoLantern by Trees loaded.");
            _player = ObjectManager.Player;
        }

        private static void OnGameUpdate(EventArgs args)
        {
            if ((!IsLow() || !_menu.Item("Auto").IsActive()) && (!_menu.Item("Hotkey").IsActive()))
            {
                return;
            }

            var lantern =
                ObjectHandler.Get<Obj_AI_Base>().Allies.FirstOrDefault(o => o.IsValid && o.Name.Equals(LanternName));
            
            if (lantern != null && lantern.IsValidTarget(500))
            {
                lantern.UseObject();
            }
        }

        private static bool IsLow()
        {
            return _player.HealthPercentage() <= _menu.Item("Low").GetValue<Slider>().Value;
        }

        private static bool ThreshInGame()
        {
            return ObjectManager.Get<Obj_AI_Hero>().Any(h => h.IsAlly && !h.IsMe && h.ChampionName == "Thresh");
        }
    }
}