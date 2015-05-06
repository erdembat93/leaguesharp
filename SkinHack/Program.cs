using System;
using System.Collections.Generic;
using LeagueSharp;
using LeagueSharp.Common;

namespace SkinHack
{
    internal class Program
    {
        public static List<ModelUnit> PlayerList = new List<ModelUnit>();
        public static ModelUnit Player;
        public static Menu Config;

        private static void Main(string[] args)
        {
            CustomEvents.Game.OnGameLoad += Game_OnGameLoad;
        }

        private static void Game_OnGameLoad(EventArgs args)
        {
            Config = new Menu("SkinHack", "SkinHack", true);

            foreach (var hero in ObjectManager.Get<Obj_AI_Hero>())
            {
                var champMenu = new Menu(hero.ChampionName, hero.ChampionName);
                var modelUnit = new ModelUnit(hero);

                PlayerList.Add(modelUnit);

                if (hero.IsMe)
                {
                    Player = modelUnit;
                }

                foreach (Dictionary<string, object> skin in ModelManager.GetSkins(hero.ChampionName))
                {
                    var skinName = skin["name"].ToString().Equals("default")
                        ? hero.ChampionName
                        : skin["name"].ToString();
                    var changeSkin = champMenu.AddItem(new MenuItem(skinName, skinName).SetValue(false));

                    if (changeSkin.IsActive())
                    {
                        modelUnit.SetModel(hero.BaseSkinName, (int) skin["num"]);
                    }

                    changeSkin.ValueChanged += (s, e) =>
                    {
                        if (e.GetNewValue<bool>())
                        {
                            champMenu.Items.ForEach(
                                p =>
                                {
                                    if (p.GetValue<bool>() && p.Name != skinName)
                                    {
                                        p.SetValue(false);
                                    }
                                });
                            modelUnit.SetModel(hero.ChampionName, (int) skin["num"]);
                        }
                    };
                }
                Config.AddSubMenu(champMenu);
            }
            Config.AddToMainMenu();

            Game.OnInput += Game_OnInput;
        }

        private static void Game_OnInput(GameInputEventArgs args)
        {
            if (args.Input.StartsWith("/model"))
            {
                args.Process = false;
                var modelName = args.Input.Replace("/model ", string.Empty);
                var model = modelName.GetValidModel();

                if (model == "" || !model.IsValidModel())
                {
                    return;
                }

                Player.SetModel(model);
                return;
            }

            if (args.Input.StartsWith("/skin"))
            {
                args.Process = false;
                var skin = Convert.ToInt32(args.Input.Replace("/skin ", string.Empty));
                Player.SetModel(Player.Unit.BaseSkinName, skin);
            }
        }
    }
}