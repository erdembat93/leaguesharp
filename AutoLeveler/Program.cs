#region

using System;
using AutoLeveler.Properties;
using LeagueSharp;
using LeagueSharp.Common;
using SharpDX;

#endregion

namespace AutoLeveler
{
    internal class Program
    {
        public static Menu Menu;
        public static int[] Sequence;
        public static Save Save;

        private static void Main(string[] args)
        {
            Utils.ClearConsole();
            CustomEvents.Game.OnGameLoad += Game_OnGameLoad;
        }

        private static void Game_OnGameLoad(EventArgs args)
        {
            Save = new Save("AutoLevel.xml", Resources.Levels);

            if (Save == null || Save.Sequence == null)
            {
                Game.PrintChat("AutoLevelSpells: Failed to load " + ObjectManager.Player.ChampionName);
                return;
            }

            Sequence = Save.Sequence;

            Menu = new Menu("AutoLevelSpells", "AutoLevelSpells", true);
            Menu.AddItem(new MenuItem("Enabled", "Enabled", true).SetValue(false));
            Menu.AddItem(new MenuItem("Edit", "Editing Mode").SetValue(false));

            var hud = Menu.AddSubMenu(new Menu("HUD", "HUD"));
            hud.AddItem(new MenuItem("X", "X").SetValue(new Slider(Drawing.Width / 2 - 384, 0, Drawing.Width)));
            hud.AddItem(new MenuItem("Y", "Y").SetValue(new Slider(Drawing.Height / 2 - 97, 0, Drawing.Height)));

            Menu.AddToMainMenu();

            var b = new SpriteBox(Resources.HUD, new Vector2(hud.Item("X").GetValue<Slider>().Value, hud.Item("Y").GetValue<Slider>().Value));

            for (var i = 0; i <= Sequence.Length - 1; i++)
            {
                var spell = Sequence[i];
                b.AddSprite(new SpriteObject(Resources.Button, new Vector2(43 + 19 * i, 22 + 19 * spell), i));
            }

            b.Draw();



            Game.OnUpdate += Game_OnGameUpdate;
        }

        private static void Game_OnGameUpdate(EventArgs args)
        {
            if (!Menu.Item("Enabled", true).IsActive())
            {
                return;
            }

            AutoLevel.Enable();

            if (SequenceDiffers(AutoLevel.GetSequence(), Sequence))
            {
                AutoLevel.UpdateSequence(Sequence);
            }
        }

        public static void UpdateSequence(int level, int spell)
        {
            Sequence[level] = spell;
            Save.UpdateSequence(Sequence);
            AutoLevel.UpdateSequence(Sequence);
        }

        public static bool SequenceDiffers(int[] one, int[] two)
        {
            if (one.Length != two.Length)
            {
                return true;
            }

            for (var i = 0; i <= one.Length - 1; i++)
            {
                if (one[i] != two[i])
                {
                    return true;
                }
            }

            return false;
        }

        public static bool EditingMode
        {
            get { return Menu.Item("Edit").GetValue<bool>(); }
        }
    }
}