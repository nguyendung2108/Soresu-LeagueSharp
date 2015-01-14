using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using LeagueSharp;
using LeagueSharp.Common;
using SharpDX;
using System.Drawing;
using Color = System.Drawing.Color;
using Others;
using SharpDX.Direct3D9;

namespace Soresu_Poppy
{
    class Program
    {
        public static Menu config;
        private static Orbwalking.Orbwalker orbwalker;
        public static readonly Obj_AI_Hero player = ObjectManager.Player;
        public static Spell Q, W, E, R, smite;
        public static double[] ultMod=new double[3]{1.2, 1.3, 1.4};
        public static double[] eSecond = new double[5] { 75, 125, 175, 225, 275};
        public static SpellSlot smiteSlot = SpellSlot.Unknown;
        static void Main(string[] args)
        {
            CustomEvents.Game.OnGameLoad += Game_OnGameLoad;
        }

        private static void Game_OnGameLoad(EventArgs args)
        {
            if (player.BaseSkinName != "Poppy") return;
            InitMenu();
            Initpoppy();
            Game.PrintChat("<font color='#9933FF'>Soresu </font><font color='#FFFFFF'>- Poppy</font>");
            Game.OnGameUpdate += Game_OnGameUpdate;
            Drawing.OnDraw += Game_OnDraw;
            Orbwalking.AfterAttack += AfterAttack;
            AntiGapcloser.OnEnemyGapcloser += OnEnemyGapcloser;
            Interrupter.OnPossibleToInterrupt += OnPossibleToInterrupt;
        }

        private static void AfterAttack(AttackableUnit unit, AttackableUnit target)
        {
            if (unit.IsMe && Q.IsReady() && orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.Combo && config.Item("useq").GetValue<bool>() && target.IsEnemy && target.Team!=player.Team)
            {
                Q.Cast(config.Item("packets").GetValue<bool>());
                Orbwalking.ResetAutoAttackTimer();
            }
        }

        private static void Game_OnGameUpdate(EventArgs args)
        {
            Obj_AI_Hero targetf = TargetSelector.GetTarget(R.Range, TargetSelector.DamageType.Magical);
            var bestpos = CF.bestVectorToPoppyFlash(targetf);
            if (config.Item("useeflashforced").GetValue<KeyBind>().Active)
            {
                bool hasFlash = player.Spellbook.CanUseSpell(player.GetSpellSlot("SummonerFlash")) == SpellState.Ready;
                if (E.IsReady() && hasFlash && !CheckWalls(player, targetf) && CF.bestVectorToPoppyFlash(targetf).IsValid())
                {
                    player.Spellbook.CastSpell(player.GetSpellSlot("SummonerFlash"), bestpos);
                    Utility.DelayAction.Add(100, () => E.CastOnUnit(targetf, config.Item("packets").GetValue<bool>()));
                }
            }
            if (config.Item("useSmite").GetValue<bool>())
            {
                CF.setSmiteSlot();
                var target = CF.GetNearest(player.Position);
                bool smiteReady = ObjectManager.Player.Spellbook.CanUseSpell(smiteSlot) == SpellState.Ready;
                if (target != null)
                {
                    double smitedamage = CF.smiteDamage();
                    if (smite.CanCast(target) && smiteReady && player.Distance(target.Position) <= smite.Range && CF.smiteDamage() >= target.Health)
                    {
                        CF.CastSmite(target);
                    }
                }
            }


            switch (orbwalker.ActiveMode)
            {
                case Orbwalking.OrbwalkingMode.Combo:
                    Combo();
                    break;
                case Orbwalking.OrbwalkingMode.Mixed:
                    break;
                case Orbwalking.OrbwalkingMode.LaneClear:
                    break;
                case Orbwalking.OrbwalkingMode.LastHit:
                    break;
                default:
                    break;
            }
        }

        private static void Combo()
        {
            Obj_AI_Hero target = TargetSelector.GetTarget(R.Range, TargetSelector.DamageType.Magical);
            if (target==null)return;
            if (config.Item("useItems").GetValue<bool>()) CF.UseSpells(target);
            bool hasFlash = player.Spellbook.CanUseSpell(player.GetSpellSlot("SummonerFlash")) == SpellState.Ready;
            //bool hasIgnite = player.Spellbook.CanUseSpell(player.GetSpellSlot("SummonerDot")) == SpellState.Ready;

            if ( config.Item("usew").GetValue<bool>() && player.Distance(target.Position)<R.Range && W.IsReady())
            {
                W.Cast(config.Item("packets").GetValue<bool>());
            }

            if (config.Item("usee").GetValue<bool>() && E.IsReady())
            {

                if (config.Item("useewall").GetValue<bool>())
                {
                    var bestpos = CF.bestVectorToPoppyFlash(target);
                    float damage = (float)(CF.ComboDamage(target) + Damage.CalcDamage(player, target, Damage.DamageType.Magical, (eSecond[E.Level-1] + 0.8f * player.FlatMagicDamageMod)) + (player.GetAutoAttackDamage(target) * 4));
                    float damageno = (float)(CF.ComboDamage(target) + (player.GetAutoAttackDamage(target) * 4));
                    if (config.Item("useeflash").GetValue<bool>() && hasFlash && !CheckWalls(player, target) && damage > target.Health && target.Health > damageno && CF.bestVectorToPoppyFlash(target).IsValid())
                    {
                        player.Spellbook.CastSpell(player.GetSpellSlot("SummonerFlash"), bestpos);
                        Utility.DelayAction.Add(100, () => E.CastOnUnit(target, config.Item("packets").GetValue<bool>()));
                    }
                    if (E.CanCast(target) && CheckWalls(player,target))
                    {
                        E.CastOnUnit(target, config.Item("packets").GetValue<bool>());
                    } 
                }
                else
                {
                    if (E.CanCast(target))
                    {
                        E.CastOnUnit(target, config.Item("packets").GetValue<bool>());
                    }  
                }
            }
            if (config.Item("user").GetValue<bool>())
            {
                if (R.IsReady() && player.Distance(target.Position) < E.Range && CF.ComboDamage(target) + player.GetAutoAttackDamage(target) * 5 < target.Health && (CF.ComboDamage(target) + player.GetAutoAttackDamage(target) * 3) * ultMod[R.Level-1] > target.Health)
                {
                    R.CastOnUnit(target, config.Item("packets").GetValue<bool>());
                }
            }
            if (config.Item("userindanger").GetValue<Slider>().Value < player.CountEnemysInRange(R.Range))
            {
                R.CastOnUnit(target, config.Item("packets").GetValue<bool>());
            }

        }

        private static void Game_OnDraw(EventArgs args)
        {
            CF.DrawCircle("drawaa", player.AttackRange);
            CF.DrawCircle("drawee", E.Range);
            CF.DrawCircle("drawrr", R.Range);
            Utility.HpBarDamageIndicator.DamageToUnit = CF.ComboDamage;
            Utility.HpBarDamageIndicator.Enabled = config.Item("drawcombo").GetValue<bool>();
        }
        private static void OnEnemyGapcloser(ActiveGapcloser gapcloser)
        {
            if (config.Item("useEgap").GetValue<bool>() && E.IsReady() && E.CanCast(gapcloser.Sender) && CheckWalls(player, gapcloser.Sender)) E.CastOnUnit(gapcloser.Sender, config.Item("packets").GetValue<bool>());
        }
        public static bool CheckWalls(Obj_AI_Base player, Obj_AI_Base enemy)
        {
            var distance = player.Position.Distance(enemy.Position);
            for (int i = 1; i < 6; i++)
            {
                if (player.Position.Extend(enemy.Position, distance + 60 * i).IsWall())
                    return true;
            }
            return false;
        }

        private static void OnPossibleToInterrupt(Obj_AI_Base unit, InterruptableSpell spell)
        {
            if (config.Item("useEint").GetValue<bool>() && E.IsReady() && E.CanCast(unit)) E.CastOnUnit(unit, config.Item("packets").GetValue<bool>());
        }

        private static void Initpoppy()
        {
            Q = new Spell(SpellSlot.Q);
            W = new Spell(SpellSlot.W);
            E = new Spell(SpellSlot.E, 525);
            R = new Spell(SpellSlot.R, 900);
        }

        private static void InitMenu()
        {
            config = new Menu("Poppy", "Poppy", true);
            // Target Selector
            Menu menuTS = new Menu("Selector", "tselect");
            TargetSelector.AddToMenu(menuTS);
            config.AddSubMenu(menuTS);

            // Orbwalker
            Menu menuOrb = new Menu("Orbwalker", "orbwalker");
            orbwalker = new Orbwalking.Orbwalker(menuOrb);
            config.AddSubMenu(menuOrb);

            // Draw settings
            Menu menuD = new Menu("Drawings ", "dsettings");
            menuD.AddItem(new MenuItem("drawaa", "Draw AA range")).SetValue(new Circle(false, Color.DarkCyan));
            menuD.AddItem(new MenuItem("drawee", "Draw E range")).SetValue(new Circle(false, Color.DarkCyan));
            menuD.AddItem(new MenuItem("drawrr", "Draw R range")).SetValue(new Circle(false, Color.DarkCyan));
            menuD.AddItem(new MenuItem("drawcombo", "Draw combo damage")).SetValue(true);
            config.AddSubMenu(menuD);
            // Combo Settings
            Menu menuC = new Menu("Combo ", "csettings");
            menuC.AddItem(new MenuItem("useq", "Use Q")).SetValue(true);
            menuC.AddItem(new MenuItem("usew", "Use W")).SetValue(true);
            menuC.AddItem(new MenuItem("usee", "Use E")).SetValue(true);
            menuC.AddItem(new MenuItem("useewall", "Use E only near walls")).SetValue(true);
            menuC.AddItem(new MenuItem("useeflash", "Use flash to positioning")).SetValue(true);
            menuC.AddItem(new MenuItem("useeflashforced", "Forced flash+E if possible")).SetValue(new KeyBind("T".ToCharArray()[0], KeyBindType.Press));
            menuC.AddItem(new MenuItem("user", "Use R to maximize dmg")).SetValue(true);
            menuC.AddItem(new MenuItem("userindanger", "Auto activate if more than")).SetValue(new Slider(3, 1, 6));
            menuC.AddItem(new MenuItem("useItems", "Use items and ignite")).SetValue(true);
            config.AddSubMenu(menuC);
            // Misc Settings
            Menu menuM = new Menu("Misc ", "Msettings");
            menuM.AddItem(new MenuItem("useEint", "Use E interrupt")).SetValue(true);
            menuM.AddItem(new MenuItem("useEgap", "Use E on gapcloser near walls")).SetValue(true);
            menuM.AddItem(new MenuItem("useSmite", "Use Smite")).SetValue(true);
            config.AddSubMenu(menuM);
            config.AddItem(new MenuItem("packets", "Use Packets")).SetValue(false);
            config.AddToMainMenu();
        }
    }
}
