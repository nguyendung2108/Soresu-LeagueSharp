using LeagueSharp;
using LeagueSharp.Common;
using System.Linq;
using SharpDX;
using System.Drawing;
using Color = System.Drawing.Color;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Others;
using SharpDX.Direct3D9;

namespace Soresu___ChoGath
{
    class Program
    {
        public static Menu config;
        private static Orbwalking.Orbwalker orbwalker;
        public static readonly Obj_AI_Hero player = ObjectManager.Player;
        public static Spell Q, W, E, R, RFlash, smite;
        public static SpellSlot smiteSlot = SpellSlot.Unknown;
        public static List<int> silence = new List<int>(new int[] { 1500, 1750, 2000, 2250, 2500});
        public static int knockUp = 1000;
        public static bool flashRblock = false; 
        public static bool vSpikes=false;
        static void Main(string[] args)
        {
            CustomEvents.Game.OnGameLoad += Game_OnGameLoad;
        }

        private static void Game_OnGameLoad(EventArgs args)
        {
            if (player.BaseSkinName != "Chogath") return;
            InitMenu();
            InitChoGath();
            Game.PrintChat("<font color='#9933FF'>Soresu </font><font color='#FFFFFF'>- Cho'Gath</font>");
            Game.OnGameUpdate += Game_OnGameUpdate;
            Drawing.OnDraw += Game_OnDraw;
            AntiGapcloser.OnEnemyGapcloser += OnEnemyGapcloser;
            Interrupter.OnPossibleToInterrupt += OnPossibleToInterrupt;
        }
        private static void Game_OnGameUpdate(EventArgs args)
        {
            vSpikes = false;
            foreach (var buff in player.Buffs)
            {
                if (buff.Name == "VorpalSpikes") vSpikes = true;
            }
            if (CF.countTurretsInRange(player) > 0 && vSpikes && E.GetHitCount() > 0)
            {
                E.Cast();
            }
            bool minionBlock = false;
            foreach (Obj_AI_Minion minion in MinionManager.GetMinions(player.Position, player.AttackRange, MinionTypes.All, MinionTeam.Enemy, MinionOrderTypes.None))
            {
                if (HealthPrediction.GetHealthPrediction(minion, 3000) <= Damage.GetAutoAttackDamage(player, minion, false))
                    minionBlock = true;
            }
            if (config.Item("useRJ").GetValue<bool>() || config.Item("useSmite").GetValue<bool>())
            {
                Jungle();
            }
            switch (orbwalker.ActiveMode)
            {
                case Orbwalking.OrbwalkingMode.Combo:
                    Combo();
                    break;
                case Orbwalking.OrbwalkingMode.Mixed:
                    if (!minionBlock)
                    {
                        Harass();
                    }

                    break;
                case Orbwalking.OrbwalkingMode.LaneClear:
                    Clear();
                    break;
                case Orbwalking.OrbwalkingMode.LastHit:
                    break;
                default:
                    if (!minionBlock)
                    {
                    }
                    break;
            }
        }

        private static void Jungle()
        {
            var target = CF.GetNearest(player.Position);
            bool hasFlash = player.Spellbook.CanUseSpell(player.GetSpellSlot("SummonerFlash")) == SpellState.Ready;
            bool smiteReady = ObjectManager.Player.Spellbook.CanUseSpell(smiteSlot) == SpellState.Ready;
            if (target != null)
            {
                CF.setSmiteSlot();
                double smitedamage = CF.smiteDamage();
                if (target.CountEnemysInRange(smite.Range)>0)
                {
                    if (config.Item("useRJ").GetValue<bool>() && config.Item("useFlashJ").GetValue<bool>() && R.IsReady() && hasFlash && 1000+player.FlatMagicDamageMod >= target.Health &&  player.GetSpell(SpellSlot.R).ManaCost <= player.Mana &&
                        player.Distance(target.Position) > 400 && player.Distance(target.Position) <= RFlash.Range &&
                        !player.Position.Extend(target.Position, 400).IsWall())
                    {
                        player.Spellbook.CastSpell(player.GetSpellSlot("SummonerFlash"), player.Position.Extend(target.Position, 400));
                        //Utility.DelayAction.Add(50, () => R.Cast(target, config.Item("packets").GetValue<bool>()));
                    }
                }
                if (config.Item("useRJ").GetValue<bool>() && R.CanCast(target) && !(config.Item("priorizeSmite").GetValue<bool>() && smiteReady) && player.GetSpell(SpellSlot.R).ManaCost <= player.Mana && 1000 + player.FlatMagicDamageMod >= target.Health)
                {
                    R.Cast(target, config.Item("packets").GetValue<bool>());
                } 
                
                if (config.Item("useSmite").GetValue<bool>() && smite.CanCast(target) && smiteReady && player.Distance(target) <= smite.Range && CF.smiteDamage() >= target.Health)
                {
                    
                    CF.CastSmite(target);
                }
            }
        }

        private static void Clear()
        {
            var minions = ObjectManager.Get<Obj_AI_Minion>().Where(m => m.IsValidTarget(400)).ToList();
            if (minions.Count() > 2)
            {
                if (Items.HasItem(3077) && Items.CanUseItem(3077))
                    Items.UseItem(3077);
                if (Items.HasItem(3074) && Items.CanUseItem(3074))
                    Items.UseItem(3074);
            }
            float perc = (float)config.Item("minmana").GetValue<Slider>().Value / 100f;
            if (player.Mana < player.MaxMana * perc) return;
            if (W.IsReady() && player.Spellbook.GetSpell(SpellSlot.W).ManaCost <= player.Mana)
            {
                var minionsForW = MinionManager.GetMinions(ObjectManager.Player.ServerPosition, W.Range, MinionTypes.All, MinionTeam.NotAlly);
                MinionManager.FarmLocation bestPositionW = W.GetLineFarmLocation(minionsForW);
                if (bestPositionW.Position.IsValid())
                    if (bestPositionW.MinionsHit >= 2)
                        W.Cast(bestPositionW.Position, config.Item("packets").GetValue<bool>());
            }
            if (Q.IsReady() && player.Spellbook.GetSpell(SpellSlot.Q).ManaCost <= player.Mana)
            {
                var minionsForQ =
                    ObjectManager.Get<Obj_AI_Minion>()
                        .Where(i => i.IsEnemy && !i.IsDead && player.Distance(i.Position) < Q.Range && CF.countMinionsInrange(i, 170f)>1)
                        .OrderByDescending(i => CF.countMinionsInrange(i, 170f))
                        .FirstOrDefault();
                if (minionsForQ.IsValid)Q.Cast(minionsForQ.Position, config.Item("packets").GetValue<bool>());
            }
        }

        private static void Harass()
        {
            Obj_AI_Hero target = TargetSelector.GetTarget(Q.Range, TargetSelector.DamageType.Magical);
            if (config.Item("useqH").GetValue<bool>())
            {
                if (target.IsValidTarget(Q.Range) && Q.IsReady()) Q.Cast(target, config.Item("packets").GetValue<bool>());
            }
            if (config.Item("useeH").GetValue<bool>())
            {
                if (target.IsValidTarget(W.Range) && W.IsReady()) W.Cast(target, config.Item("packets").GetValue<bool>());
            }
            if (config.Item("useeH").GetValue<bool>() && !vSpikes && E.GetHitCount() > 0)
            {
                E.Cast();
            }
        }

        private static void Combo()
        {

            Obj_AI_Hero target = TargetSelector.GetTarget(Q.Range, TargetSelector.DamageType.Magical);
            bool hasFlash = player.Spellbook.CanUseSpell(player.GetSpellSlot("SummonerFlash")) == SpellState.Ready;
            bool hasIgnite = player.Spellbook.CanUseSpell(player.GetSpellSlot("SummonerDot")) == SpellState.Ready;
            var combodmg = CF.ComboDamage(target);
            if (combodmg > target.Health && hasIgnite)
            {
                player.Spellbook.CastSpell(player.GetSpellSlot("SummonerDot"), target);
            }
            CF.UseSpells(target);
                //VorpalSpikes
                //rupturelaunch
                //Silence

            if (hasIgnite)
            {
                flashRblock = true;
            }
            else
            {
                flashRblock = false;
            }
            if (config.Item("useq").GetValue<bool>())
            {
                if (target.IsValidTarget(Q.Range))
                {
                    int qHit = config.Item("qHit", true).GetValue<Slider>().Value;
                    var hitC = HitChance.High;
                    switch (qHit)
                    {
                        case 1:
                            hitC = HitChance.Low;
                            break;
                        case 2:
                            hitC = HitChance.Medium;
                            break;
                        case 3:
                            hitC = HitChance.High;
                            break;
                        case 4:
                            hitC = HitChance.VeryHigh;
                            break;
                    }
                    Q.CastIfHitchanceEquals(target, hitC, config.Item("packets").GetValue<bool>());
                }
            }
            if (config.Item("usew").GetValue<bool>())
            {
                if (target.IsValidTarget(W.Range) && W.IsReady())
                {
                    W.Cast(target, config.Item("packets").GetValue<bool>());
                }
            }

            if (config.Item("usee").GetValue<bool>() && !vSpikes && E.GetHitCount() > 0 && (CF.countTurretsInRange(player) < 1 || target.Health < 150))
            {
                
                E.Cast();
            }
            if (config.Item("UseFlashC").GetValue<bool>() && !flashRblock && R.IsReady() && hasFlash && !CF.CheckCriticalBuffs(target) && player.GetSpell(SpellSlot.R).ManaCost <= player.Mana && player.Distance(target.Position) >= 400 && player.GetSpellDamage(target, SpellSlot.R) > target.Health && !Q.IsReady() && !W.IsReady() && player.Distance(target.Position) <= RFlash.Range && !player.Position.Extend(target.Position, 400).IsWall())
            {
                player.Spellbook.CastSpell(player.GetSpellSlot("SummonerFlash"), player.Position.Extend(target.Position, 400));
                Utility.DelayAction.Add(50, () => R.Cast(target, config.Item("packets").GetValue<bool>()));
            }
            if (config.Item("UseFlashC").GetValue<bool>() && !flashRblock && W.IsReady() && hasFlash && !CF.CheckCriticalBuffs(target) && player.GetSpell(SpellSlot.W).ManaCost <= player.Mana && player.Distance(target.Position) > W.Range + 300 && player.GetSpellDamage(target, SpellSlot.W) > target.Health && !Q.IsReady() && !R.IsReady() && player.Distance(target.Position) <= W.Range + 400 && !player.Position.Extend(target.Position, 400).IsWall())
            {
                player.Spellbook.CastSpell(player.GetSpellSlot("SummonerFlash"), player.Position.Extend(target.Position, 400));
                Utility.DelayAction.Add(50, () => W.Cast(target, config.Item("packets").GetValue<bool>()));
            }
            if (config.Item("user").GetValue<bool>() && player.GetSpellDamage(target, SpellSlot.R)>target.Health)
            {
                if (target.IsValidTarget(R.Range) && R.IsReady()) R.Cast(target, config.Item("packets").GetValue<bool>());
            }
            
        }
        private static void OnPossibleToInterrupt(Obj_AI_Base unit, InterruptableSpell spell)
        {
            if (config.Item("useQint").GetValue<bool>())
            {
                if (unit.IsValidTarget(Q.Range) && Q.IsReady()) Q.Cast(unit, config.Item("packets").GetValue<bool>());
            }
            if (config.Item("useWint").GetValue<bool>())
            {
                if (unit.IsValidTarget(W.Range) && W.IsReady()) W.Cast(unit, config.Item("packets").GetValue<bool>());
            }
        }

        private static void OnEnemyGapcloser(ActiveGapcloser gapcloser)
        {
            
            if (config.Item("useQgc").GetValue<bool>())
            {
                if (gapcloser.Sender.IsValidTarget(Q.Range) && Q.IsReady()) Q.Cast(gapcloser.End, config.Item("packets").GetValue<bool>());
            }
            if (config.Item("useWgc").GetValue<bool>())
            {
                if (gapcloser.Sender.IsValidTarget(W.Range) && W.IsReady()) W.Cast(gapcloser.End, config.Item("packets").GetValue<bool>());
            }
        }

        private static void Game_OnDraw(EventArgs args)
        {
            CF.DrawCircle("drawaa", player.AttackRange);
            CF.DrawCircle("drawqq", Q.Range);
            CF.DrawCircle("drawww", W.Range);
            CF.DrawCircle("drawrrflash", RFlash.Range);
            Utility.HpBarDamageIndicator.DamageToUnit = CF.ComboDamage;
            Utility.HpBarDamageIndicator.Enabled = config.Item("drawcombo").GetValue<bool>();
        }

        private static void InitChoGath()
        {
            Q = new Spell(SpellSlot.Q, 950);
            Q.SetSkillshot(Q.Instance.SData.SpellCastTime, Q.Instance.SData.LineWidth, Q.Speed, false, SkillshotType.SkillshotCircle);
            W = new Spell(SpellSlot.W, 700);
            W.SetSkillshot(W.Instance.SData.SpellCastTime, W.Instance.SData.LineWidth, W.Speed, false, SkillshotType.SkillshotCone);
            E = new Spell(SpellSlot.E, 500);
            E.SetSkillshot(E.Instance.SData.SpellCastTime, E.Instance.SData.LineWidth, E.Speed, false, SkillshotType.SkillshotLine);
            R = new Spell(SpellSlot.R, 175);
            RFlash = new Spell(SpellSlot.R, 555);
        }

        private static void InitMenu()
        {
            config = new Menu("Cho'Gath", "ChoGath", true);
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
            menuD.AddItem(new MenuItem("drawaa", "Draw AA range")).SetValue(new Circle(false, Color.FromArgb(180, 200, 46, 66)));
            menuD.AddItem(new MenuItem("drawqq", "Draw Q range")).SetValue(new Circle(false, Color.FromArgb(180, 200, 46, 66)));
            menuD.AddItem(new MenuItem("drawww", "Draw W range")).SetValue(new Circle(false, Color.FromArgb(180, 200, 46, 66)));
            menuD.AddItem(new MenuItem("drawee", "Draw E range")).SetValue(new Circle(false, Color.FromArgb(180, 200, 46, 66)));
            menuD.AddItem(new MenuItem("drawrrflash", "Draw R+flash range")).SetValue(new Circle(true, Color.FromArgb(150, 250, 248, 110)));
            menuD.AddItem(new MenuItem("drawcombo", "Draw combo damage")).SetValue(true);
            config.AddSubMenu(menuD);
            // Combo Settings
            Menu menuC = new Menu("Combo ", "csettings");
            menuC.AddItem(new MenuItem("useq", "Use Q")).SetValue(true);
            menuC.AddItem(new MenuItem("qHit", "Q hitChance", true).SetValue(new Slider(3, 1, 4)));
            menuC.AddItem(new MenuItem("usew", "Use W")).SetValue(true);
            menuC.AddItem(new MenuItem("usee", "Use E")).SetValue(true);
            menuC.AddItem(new MenuItem("user", "Use R")).SetValue(true);
            menuC.AddItem(new MenuItem("UseFlashC", "Use flash")).SetValue(false);
            config.AddSubMenu(menuC);
            // Harass Settings
            Menu menuH = new Menu("Harass ", "Hsettings");
            menuH.AddItem(new MenuItem("useqH", "Use Q")).SetValue(true);
            menuH.AddItem(new MenuItem("usewH", "Use W")).SetValue(true);
            menuH.AddItem(new MenuItem("useeH", "Use E")).SetValue(true);
            config.AddSubMenu(menuH);
            // LaneClear Settings
            Menu menuLC = new Menu("LaneClear ", "Lcsettings");
            menuLC.AddItem(new MenuItem("useqLC", "Use Q")).SetValue(true);
            menuLC.AddItem(new MenuItem("usewLC", "Use W")).SetValue(true);
            menuLC.AddItem(new MenuItem("useeLC", "Use E")).SetValue(true);
            menuLC.AddItem(new MenuItem("minmana", "Keep X% mana")).SetValue(new Slider(1, 1, 100));
            config.AddSubMenu(menuLC);
            // Jungle Settings
            Menu menuJ = new Menu("Jungle ", "Jsettings");
            menuJ.AddItem(new MenuItem("useSmite", "Use Smite")).SetValue(true);
            menuJ.AddItem(new MenuItem("useRJ", "Use R")).SetValue(true);
            menuJ.AddItem(new MenuItem("priorizeSmite", "Use smite if possible")).SetValue(false);
            menuJ.AddItem(new MenuItem("useFlashJ", "Use Flash to steal")).SetValue(true);
            config.AddSubMenu(menuJ);
            // Misc Settings
            Menu menuM = new Menu("Misc ", "Msettings");
            menuM.AddItem(new MenuItem("useQint", "Use Q interrupt")).SetValue(true);
            menuM.AddItem(new MenuItem("useQgc", "Use Q on gapclosers")).SetValue(false);
            menuM.AddItem(new MenuItem("useWint", "Use W interrupt")).SetValue(true);
            menuM.AddItem(new MenuItem("useWgc", "Use W on gapclosers")).SetValue(false);
            config.AddSubMenu(menuM);
            config.AddItem(new MenuItem("packets", "Use Packets")).SetValue(false);
            config.AddToMainMenu();
        }
    }
}
