using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Threading;
using System.Web.Script.Serialization;
using LeagueSharp;
using LeagueSharp.Common;

namespace SkinHack
{
    internal class Program
    {
        public static Menu Config;
        public static String DataDragonBase = "http://ddragon.leagueoflegends.com/";

        private static void Main(string[] args)
        {
            CustomEvents.Game.OnGameLoad += Game_OnGameLoad;
        }

        private static void Game_OnGameLoad(EventArgs args)
        {
            new Thread(
                () =>
                {
                    Config = new Menu("SkinHax", "SkinHax", true);
                    var versionJson = new WebClient().DownloadString(DataDragonBase + "realms/na.json");
                    var gameVersion =
                        (String)
                            ((Dictionary<String, Object>)
                                new JavaScriptSerializer().Deserialize<Dictionary<String, Object>>(versionJson)["n"])[
                                    "champion"];
                    foreach (var hero in ObjectManager.Get<Obj_AI_Hero>())
                    {
                        var champJson =
                            new WebClient().DownloadString(
                                DataDragonBase + "cdn/" + gameVersion + "/data/en_US/champion/" + hero.ChampionName +
                                ".json");
                        var skins =
                            (ArrayList)
                                ((Dictionary<String, Object>)
                                    ((Dictionary<String, Object>)
                                        new JavaScriptSerializer().Deserialize<Dictionary<String, Object>>(champJson)[
                                            "data"])[hero.ChampionName])["skins"];
                        var champMenu = new Menu(hero.ChampionName, hero.ChampionName, false);
                        foreach (Dictionary<string, object> skin in skins)
                        {
                            var skinName = skin["name"].ToString();
                            if (skinName.Equals("default"))
                            {
                                skinName = hero.ChampionName;
                            }
                            var changeSkin = champMenu.AddItem(new MenuItem(skinName, skinName).SetValue(false));
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
                                    hero.SetSkin(hero.ChampionName, (int) skin["num"]);
                                }
                            };
                        }
                        Config.AddSubMenu(champMenu);
                    }
                    Config.AddToMainMenu();
                }).Start();

            Game.OnInput += Game_OnInput;
        }

        private static void Game_OnInput(GameInputEventArgs args)
        {
            if (args.Input.StartsWith("/model"))
            {
                args.Process = false;
                ObjectManager.Player.SetSkin(args.Input.Replace("/model ", string.Empty), 0);
                return;
            }

            if (args.Input.StartsWith("/skin"))
            {
                args.Process = false;
                ObjectManager.Player.SetSkin(
                    ObjectManager.Player.BaseSkinName, Convert.ToInt32(args.Input.Replace("/skin ", string.Empty)));
            }
        }
    }
}