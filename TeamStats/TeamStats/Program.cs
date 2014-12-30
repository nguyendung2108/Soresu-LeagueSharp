using LeagueSharp;
using LeagueSharp.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SharpDX.Direct3D9;
using TeamStats.Properties;
using Color = System.Drawing.Color;
namespace TeamStats
{
    class Program
    {
        public static Menu Config;
        public static readonly Obj_AI_Hero player = ObjectManager.Player;
        public static Teams teams;
        public static int range = 2500;
        private static Render.Sprite frame;
        public static int myTeamHpX = (int)(Drawing.Width*0.68);
        public static int myTeamHpY = (int)(Drawing.Height * 0.97);
        public static int enemyTeamHpX = (int)(Drawing.Width * 0.68);
        public static int enemyTeamHpY = (int)(Drawing.Height * 0.97) - 40;
        public static int myTeamDmgX = (int)(Drawing.Width * 0.68);
        public static int myTeamDmgY = (int)(Drawing.Height * 0.97) - 20;
        public static int enemyTeamDmgX = (int)(Drawing.Width * 0.68);
        public static int enemyTeamDmgY = (int)(Drawing.Height * 0.97) - 60;
        static void Main(string[] args)
        {
            CustomEvents.Game.OnGameLoad += Game_OnGameLoad;
        }

        private static void Game_OnGameLoad(EventArgs args)
        {
            Config = new Menu("TeamStats", "TeamStats", true);
            Config.AddItem(new MenuItem("X-pos", "X offset").SetValue(new Slider(0, -1500, 400)));
            Config.AddItem(new MenuItem("Y-pos", "Y offset").SetValue(new Slider(0, 200, -1080)));
            Config.AddItem(new MenuItem("Range", "Range").SetValue(new Slider(2200, 0, 3000)));
            Config.AddItem(new MenuItem("Default", "Default").SetValue(false));
            Config.AddItem(new MenuItem("Chart", "Chart").SetValue(true));
            Config.AddItem(new MenuItem("Enabled", "Enabled").SetValue(true));
            Config.AddItem(new MenuItem("draw", "Draw range")).SetValue(new Circle(false, Color.LightBlue));
            Config.AddToMainMenu();
            //frame = loadFrame();
            Drawing.OnDraw += Drawing_OnDraw;
            Game.OnGameUpdate += Game_OnGameUpdate;
            Game.PrintChat("<font color='#9933FF'>Soresu </font><font color='#FFFFFF'>- TeamStats</font>");
        }

        private static void Game_OnGameUpdate(EventArgs args)
        {
            if (Config.Item("Enabled").GetValue<bool>() && player.CountEnemysInRange(range) > 0) teams = new Teams();
            if (Config.Item("Default").GetValue<bool>())
            {
                Config.Item("Y-pos").SetValue(new Slider(0, 200, -1080));
                Config.Item("X-pos").SetValue(new Slider(0, -1500, 400));
                Config.Item("Default").SetValue(false);
            }
            range = Config.Item("Range").GetValue<Slider>().Value;
            
        }

        private static void Drawing_OnDraw(EventArgs args)
        {
            if (Config.Item("Enabled").GetValue<bool>() && player.CountEnemysInRange(range) > 0)
            {
                
                var OffsetX = Config.Item("X-pos").GetValue<Slider>().Value;
                var OffsetY = Config.Item("Y-pos").GetValue<Slider>().Value;
                int myteamhpBar = teams.myTeamHP;
                int enemyteamhpBar = teams.enemyTeamHP;
                int myteamdmgBar = teams.myTeamDmg;
                int enemyteamdmgBar = teams.enemyTeamDmg;
                int highest1 = Math.Max(myteamhpBar, enemyteamhpBar);
                int highest2 = Math.Max(myteamdmgBar, enemyteamdmgBar);
                int highest = Math.Max(highest1, highest2);
                float scale = 300f / highest;
               if (Config.Item("Chart").GetValue<bool>())
                {
                    Drawing.DrawLine(myTeamHpX + OffsetX, myTeamHpY+28 + OffsetY, enemyTeamDmgX + OffsetX, enemyTeamDmgY + OffsetY, 300, Color.Peru);
                    Drawing.DrawText(myTeamHpX + OffsetX, myTeamHpY + OffsetY - 2, Color.White, "Allies Health(" + myteamhpBar + ")");
                    Drawing.DrawLine(myTeamHpX + OffsetX, myTeamHpY + OffsetY, myTeamHpX + OffsetX, myTeamHpY + OffsetY + 14, (int)(myteamhpBar * scale * -1), Color.ForestGreen);

                    Drawing.DrawText(enemyTeamHpX + OffsetX, enemyTeamHpY + OffsetY - 2, Color.White, "Enemies Healt(" + enemyteamhpBar + ")");
                    Drawing.DrawLine(enemyTeamHpX + OffsetX, enemyTeamHpY + OffsetY, enemyTeamHpX + OffsetX, enemyTeamHpY + OffsetY + 14, (int)(-enemyteamhpBar * scale), Color.ForestGreen);

                    Drawing.DrawText(myTeamDmgX + OffsetX, myTeamDmgY + OffsetY - 2, Color.White, "Allies Damage(" + myteamdmgBar+")");
                    Drawing.DrawLine(myTeamDmgX + OffsetX, myTeamDmgY + OffsetY, myTeamDmgX + OffsetX, myTeamDmgY + OffsetY + 14, (int)(-myteamdmgBar * scale), Color.Firebrick);

                    Drawing.DrawText(enemyTeamDmgX + OffsetX, enemyTeamDmgY + OffsetY - 2, Color.White, "Enemies Damage(" + enemyteamdmgBar + ")");
                    Drawing.DrawLine(enemyTeamDmgX + OffsetX, enemyTeamDmgY + OffsetY, enemyTeamDmgX + OffsetX, enemyTeamDmgY + OffsetY + 14, (int)(-enemyteamdmgBar * scale), Color.Firebrick);
                }
                   if (myteamhpBar-enemyteamdmgBar<enemyteamhpBar-myteamdmgBar)
                {
                    Drawing.DrawText(myTeamHpX+ 60 + OffsetX, myTeamHpY + OffsetY +14, Color.Red, "Enemy team is stronger");
                }
                else
                {
                    Drawing.DrawText(myTeamHpX + 55 + OffsetX, myTeamHpY + OffsetY +14, Color.ForestGreen, "Your team is stronger");
                }
                DrawCircle("draw", range);
            }
        }
        private static Render.Sprite loadFrame()
        {

                var load = new Render.Sprite(Resources.tsframe, new SharpDX.Vector2(0, 0))
                {
                    Color = new SharpDX.ColorBGRA(255f, 255f, 255f, 20f)
                };
                load.Position = new SharpDX.Vector2((int)(Drawing.Width * 0.628), (int)(Drawing.Height - 102));
                load.Show();
                load.Add(0);
                return load;
        }
        private static void DrawCircle(string menuItem, float range)
        {
            Circle circle = Config.Item(menuItem).GetValue<Circle>();
            if (circle.Active) Utility.DrawCircle(player.Position, range, circle.Color);
        }
    }
}
