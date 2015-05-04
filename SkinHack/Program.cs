using System;
using System.Collections.Generic;
using LeagueSharp;
using LeagueSharp.Common;

namespace SkinHack
{
    internal class Program
    {
        public static Menu Config;
        public static bool HaveDied;
        public static string CurrentModel;
        public static int CurrentSkin;

        private static void Main(string[] args)
        {
            CustomEvents.Game.OnGameLoad += Game_OnGameLoad;
        }

        private static void Game_OnGameLoad(EventArgs args)
        {
            CurrentModel = ObjectManager.Player.BaseSkinName;

            Config = new Menu("SkinHack", "SkinHack", true);

            foreach (var hero in ObjectManager.Get<Obj_AI_Hero>())
            {
                var champMenu = new Menu(hero.ChampionName, hero.ChampionName, false);
                foreach (Dictionary<string, object> skin in ModelManager.GetSkins(hero.ChampionName))
                {
                    var skinName = skin["name"].ToString();
                    if (skinName.Equals("default"))
                    {
                        skinName = hero.ChampionName;
                    }
                    var changeSkin = champMenu.AddItem(new MenuItem(skinName, skinName).SetValue(false));

                    if (changeSkin.IsActive())
                    {
                        Utility.DelayAction.Add(500, () => hero.SetSkin(hero.BaseSkinName, (int) skin["num"]));
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
                            CurrentModel = hero.ChampionName;
                            CurrentSkin = (int) skin["num"];
                            hero.SetSkin(CurrentModel, CurrentSkin);
                        }
                    };
                }
                Config.AddSubMenu(champMenu);
            }
            Config.AddToMainMenu();

            GameObject.OnCreate += Obj_AI_Base_OnCreate;
            Game.OnInput += Game_OnInput;
            Game.OnUpdate += Game_OnUpdate;
        }

        private static void Obj_AI_Base_OnCreate(GameObject sender, EventArgs args)
        {
            var unit = sender as Obj_AI_Base;

            if (unit == null || !unit.IsValid || !unit.Name.Equals(ObjectManager.Player.Name))
            {
                return;
            }

            unit.SetSkin(CurrentModel, CurrentSkin);
        }

        private static void UpdateModel(string model, int skin = 0)
        {
            CurrentModel = model;
            CurrentSkin = skin;
            ObjectManager.Player.SetSkin(CurrentModel, CurrentSkin);
        }

        private static void Game_OnUpdate(EventArgs args)
        {
            if (!HaveDied || ObjectManager.Player.IsDead)
            {
                HaveDied = ObjectManager.Player.IsDead;
                return;
            }

            ObjectManager.Player.SetSkin(CurrentModel, CurrentSkin);
        }

        private static void Game_OnInput(GameInputEventArgs args)
        {
            if (args.Input.StartsWith("/model"))
            {
                args.Process = false;
                var model = args.Input.Replace("/model ", string.Empty);

                if (model.IsValidModel())
                {
                    return;
                }

                UpdateModel(model);
                return;
            }

            if (args.Input.StartsWith("/skin"))
            {
                args.Process = false;
                var skin = Convert.ToInt32(args.Input.Replace("/skin ", string.Empty));
                UpdateModel(ObjectManager.Player.BaseSkinName, skin);
            }
        }
    }
}