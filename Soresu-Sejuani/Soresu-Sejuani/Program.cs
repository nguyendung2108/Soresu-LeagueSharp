using LeagueSharp;
using LeagueSharp.Common;
using SharpDX;
using System;
using System.Collections.Generic;
using System.Diagnostics.Eventing.Reader;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Color = System.Drawing.Color;

namespace Soresu_Sejuani
{
    class Program
    {
        public static Menu config;
        private static Orbwalking.Orbwalker orbwalker;
        private static readonly Obj_AI_Hero me = ObjectManager.Player;
        public static Spell Q, W, E, R;
        public static Items.Item botrk = new Items.Item(3153, 450);
        public static Items.Item bilgewater = new Items.Item(3144, 450);
        public static Items.Item hexgun = new Items.Item(3146, 700);
        public static Items.Item Dfg = new Items.Item(3128, 750);
        public static Items.Item Bft = new Items.Item(3188, 750);
        static void Main(string[] args)
        {
            CustomEvents.Game.OnGameLoad += Game_OnGameLoad;


        }

        private static void Game_OnGameLoad(EventArgs args)
        {
            if (me.BaseSkinName != "Sejuani") return;
            InitMenu();
            InitSejuani();
            Game.PrintChat("<font color='#9933FF'>Soresu </font><font color='#FFFFFF'>- Sejuani</font>");
            //Game.OnGameInput += Game_GameInput;
            Game.OnGameUpdate += Game_OnGameUpdate;
            Drawing.OnDraw += Game_OnDraw;
            //Obj_AI_Base.OnProcessSpellCast += OnProcessSpell;
            AntiGapcloser.OnEnemyGapcloser += OnEnemyGapcloser;
            Interrupter.OnPossibleToInterrupt += OnPossibleToInterrupt;


        }

        private static void InitSejuani()
        {
            Q = new Spell(SpellSlot.Q, 650);
            W = new Spell(SpellSlot.W, 350);
            E = new Spell(SpellSlot.E, 1000);
            R = new Spell(SpellSlot.R, 1175);
            Q.SetSkillshot(Q.Instance.SData.SpellCastTime, Q.Instance.SData.LineWidth, Q.Instance.SData.MissileSpeed, true, SkillshotType.SkillshotLine);
            R.SetSkillshot(R.Instance.SData.SpellCastTime, R.Instance.SData.LineWidth, R.Instance.SData.MissileSpeed, true, SkillshotType.SkillshotLine);
        }
        private static void InitMenu()
        {
            config = new Menu("Sejuani", "Sejuani", true);
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
            menuD.AddItem(new MenuItem("drawaa", "Draw AA range")).SetValue(new Circle(true, Color.FromArgb(80, 150, 177, 208)));
            menuD.AddItem(new MenuItem("drawqq", "Draw Q range")).SetValue(new Circle(true, Color.FromArgb(80, 150, 177, 208)));
            menuD.AddItem(new MenuItem("drawww", "Draw W range")).SetValue(new Circle(true, Color.FromArgb(80, 150, 177, 208)));
            menuD.AddItem(new MenuItem("drawee", "Draw E range")).SetValue(new Circle(true, Color.FromArgb(80, 150, 177, 208)));
            menuD.AddItem(new MenuItem("drawrr", "Draw R range")).SetValue(new Circle(true, Color.FromArgb(80, 150, 177, 208)));
            menuD.AddItem(new MenuItem("drawcombo", "Draw combo damage")).SetValue(true);
            config.AddSubMenu(menuD);

            // Combo Settings
            Menu menuC = new Menu("Combo ", "csettings");
            menuC.AddItem(new MenuItem("useq", "Use Q")).SetValue(true);
            menuC.AddItem(new MenuItem("usew", "Use W")).SetValue(true);
            menuC.AddItem(new MenuItem("useemin", "Use E min")).SetValue(new Slider(1, 1, 5));
            menuC.AddItem(new MenuItem("useEminr", "E minimum range")).SetValue(new Slider(0, 0, 900));
            menuC.AddItem(new MenuItem("useRmin", "R only if more than")).SetValue(new Slider(1, 1, 5));
            menuC.AddItem(new MenuItem("useRminr", "Ulti minimum range")).SetValue(new Slider(0, 0, 350));
            menuC.AddItem(new MenuItem("manualR", "Cast R asap")).SetValue(new KeyBind("T".ToCharArray()[0], KeyBindType.Press));
            menuC.AddItem(new MenuItem("packets", "Use Packets")).SetValue(false);
            config.AddSubMenu(menuC);
            // Clear/Jungle
            Menu menuJ = new Menu("Clear ", "jsettings");
            menuJ.AddItem(new MenuItem("useqC", "Use Q")).SetValue(true);
            menuJ.AddItem(new MenuItem("usewC", "Use W")).SetValue(true);
            menuJ.AddItem(new MenuItem("useeCmin", "Use E min")).SetValue(new Slider(1, 1, 5));
            menuJ.AddItem(new MenuItem("useiC", "Use Items")).SetValue(true);
            menuJ.AddItem(new MenuItem("minmana", "Keep X% mana")).SetValue(new Slider(1, 1, 100));
            config.AddSubMenu(menuJ);
            // Misc Settings
            Menu menuU = new Menu("Misc ", "usettings");
            menuU.AddItem(new MenuItem("useqgc", "Use Q to anti gap closer")).SetValue(false);
            menuU.AddItem(new MenuItem("useqint", "Use Q to interrupt")).SetValue(true);
            menuU.AddItem(new MenuItem("usergc", "Use R to anti gap closer")).SetValue(false);
            menuU.AddItem(new MenuItem("userint", "Use R to interrupt")).SetValue(false);
            config.AddSubMenu(menuU);
            var sulti = new Menu("Don't ult on ", "dontult");
            foreach (var hero in ObjectManager.Get<Obj_AI_Hero>().Where(hero => hero.IsEnemy))
            {
                sulti.AddItem(new MenuItem("ult" + hero.SkinName, hero.SkinName)).SetValue(false);
            }
            config.AddSubMenu(sulti);
            config.AddToMainMenu();
            
        }

        private static void Game_OnDraw(EventArgs args)
        {
            DrawCircle("drawaa", me.AttackRange);
            DrawCircle("drawqq", Q.Range);
            DrawCircle("drawww", W.Range);
            DrawCircle("drawee", E.Range);
            DrawCircle("drawrr", R.Range);
            Utility.HpBarDamageIndicator.DamageToUnit = ComboDamage;
            Utility.HpBarDamageIndicator.Enabled = config.Item("drawcombo").GetValue<bool>();
        }

        private static void Game_GameInput(GameInputEventArgs args)
        {


        }
        private static void Game_OnGameUpdate(EventArgs args)
        {
            bool minionBlock = false;
            foreach (Obj_AI_Minion minion in MinionManager.GetMinions(me.Position, me.AttackRange, MinionTypes.All, MinionTeam.Enemy, MinionOrderTypes.None))
            {
                if (HealthPrediction.GetHealthPrediction(minion, 3000) <= Damage.GetAutoAttackDamage(me, minion, false))
                    minionBlock = true;
            }
            switch (orbwalker.ActiveMode)
            {
                case Orbwalking.OrbwalkingMode.Combo:
                    Combo();
                    break;
                case Orbwalking.OrbwalkingMode.Mixed:
                    //if (!minionBlock) Harass();
                    break;
                case Orbwalking.OrbwalkingMode.LaneClear:
                    if (!minionBlock) Clear();
                    break;
                case Orbwalking.OrbwalkingMode.LastHit:
                    break;
                default:
                    break;
            }
            if (config.Item("manualR").GetValue<KeyBind>().Active && R.IsReady()) CastR();
        }

        private static void CastR()
        {
            foreach (var enemy in ObjectManager.Get<Obj_AI_Hero>().Where(i => i.IsEnemy && !i.IsDead && me.Distance(i) < R.Range).OrderByDescending(l => countChampsAtrange(l, 350f)))
            {
                R.Cast(enemy, config.Item("packets").GetValue<bool>());
                break;
            }
        }
        private static void OnEnemyGapcloser(ActiveGapcloser gapcloser)
        {
            if (config.Item("useqgc").GetValue<bool>())
            {
                if (gapcloser.Sender.IsValidTarget(Q.Range) && Q.IsReady() && me.Distance(gapcloser.End) < Q.Range) Q.Cast(gapcloser.End, config.Item("packets").GetValue<bool>());
            }
            if (config.Item("usergc").GetValue<bool>())
            {
                if (gapcloser.Sender.IsValidTarget(R.Range) && R.IsReady() && me.Distance(gapcloser.End) < R.Range) R.Cast(gapcloser.End, config.Item("packets").GetValue<bool>());
            }
        }
        private static void OnPossibleToInterrupt(Obj_AI_Base unit, InterruptableSpell spell)
        {
            if (config.Item("useqint").GetValue<bool>())
            {
                if (unit.IsValidTarget(Q.Range) && Q.IsReady() && me.Distance(unit) < Q.Range) Q.Cast(unit.Position, config.Item("packets").GetValue<bool>());
            }
            if (config.Item("userint").GetValue<bool>())
            {
                if (unit.IsValidTarget(R.Range) && R.IsReady() && me.Distance(unit) < R.Range) R.Cast(unit.Position, config.Item("packets").GetValue<bool>());
            }
        }

        private static void Clear()
        {
            float perc = (float)config.Item("minmana").GetValue<Slider>().Value/100f;
            if (me.Mana < me.MaxMana * perc) return;
            Q.SetSkillshot(Q.Instance.SData.SpellCastTime, Q.Instance.SData.LineWidth, Q.Instance.SData.MissileSpeed, false, SkillshotType.SkillshotLine);
            var minions = ObjectManager.Get<Obj_AI_Minion>().Where(m => m.IsValidTarget(400)).ToList();
            if (minions.Count() > 2)
            {
                if (Items.HasItem(3077) && Items.CanUseItem(3077))
                    Items.UseItem(3077);
                if (Items.HasItem(3074) && Items.CanUseItem(3074))
                    Items.UseItem(3074);
            }
            var minionsSpells = ObjectManager.Get<Obj_AI_Minion>().Where(m => m.IsValidTarget(W.Range)).ToList();
            if (W.IsReady() && minionsSpells.Count() > 1 && config.Item("usewC").GetValue<bool>() && me.Spellbook.GetSpell(SpellSlot.W).ManaCost <= me.Mana) W.Cast();
            var minHit = config.Item("useeCmin").GetValue<Slider>().Value;
            if (E.IsReady() && me.Spellbook.GetSpell(SpellSlot.Q).ManaCost <= me.Mana && CountBuffMini(E.Range) >= minHit && (!(!Q.IsReady() && me.Mana - me.Spellbook.GetSpell(SpellSlot.Q).ManaCost < me.MaxMana * perc) || !(!W.IsReady() && me.Mana - me.Spellbook.GetSpell(SpellSlot.W).ManaCost < me.MaxMana * perc)))
            {
                E.Cast();
            }
            if (Q.IsReady() && me.Spellbook.GetSpell(SpellSlot.Q).ManaCost <= me.Mana)
            {
                var minionsForQ = MinionManager.GetMinions(ObjectManager.Player.ServerPosition, Q.Range, MinionTypes.All, MinionTeam.NotAlly);
                MinionManager.FarmLocation bestPosition = Q.GetLineFarmLocation(minionsForQ);
                if (bestPosition.Position.IsValid())
                    if (bestPosition.MinionsHit >= 2)
                        Q.Cast(bestPosition.Position, config.Item("packets").GetValue<bool>());
                //Q.Cast(enemy.Position, config.Item("packets").GetValue<bool>());
            }

            Q.SetSkillshot(Q.Instance.SData.SpellCastTime, Q.Instance.SData.LineWidth, Q.Instance.SData.MissileSpeed, true, SkillshotType.SkillshotLine);
        }

        private static int countMinionsInrange(Obj_AI_Minion l, float p)
        {
            int num=0;
            foreach (var enemy in ObjectManager.Get<Obj_AI_Minion>().Where(i => !i.IsDead && i.IsEnemy && l.Distance(i) < p))
            {
                num++;
            }

            return num;
        }
        private static int countChampsAtrange(Obj_AI_Hero l, float p)
        {
            int num = 0;
            foreach (var enemy in ObjectManager.Get<Obj_AI_Hero>().Where(i => !i.IsDead && i.IsEnemy && i.Distance(l) < p))
            {
                num++;
            }
            
            return num;
        }
        private static void Ulti()
        {

            if (!R.IsReady() || config.Item("useRmin").GetValue<Slider>().Value == 0) return;
            if (config.Item("useRmin").GetValue<Slider>().Value == 1)
            {
                var target = TargetSelector.GetTarget(R.Range, TargetSelector.DamageType.Magical);
                if (!config.Item("ult" + target.SkinName).GetValue<bool>()) R.Cast(target, config.Item("packets").GetValue<bool>());
                }
            else {
                foreach (var enemy in ObjectManager.Get<Obj_AI_Hero>().Where(i => i.IsEnemy && !i.IsDead && me.Distance(i) < R.Range && me.Distance(i) > config.Item("useRminr").GetValue<Slider>().Value && !config.Item("ult" + i.SkinName).GetValue<bool>() && countChampsAtrange(i, 350f) >= config.Item("useRmin").GetValue<Slider>().Value).OrderByDescending(l => countChampsAtrange(l, 350f)))
                {
                    R.Cast(enemy, config.Item("packets").GetValue<bool>());
                    return;
                }
            }
        }
        private static void Harass()
        {
        }
        private static void Combo()
        {
            
            Ulti();
            float perc = (float)config.Item("minmana").GetValue<Slider>().Value / 100f;
            var minHit = config.Item("useemin").GetValue<Slider>().Value;
            Obj_AI_Hero target = TargetSelector.GetTarget(R.Range, TargetSelector.DamageType.Magical);
            UseItems(target);
            if (W.IsReady() && config.Item("usew").GetValue<bool>()&& me.CountEnemysInRange((int)W.Range)>0 && me.Spellbook.GetSpell(SpellSlot.W).ManaCost<=me.Mana)
            {
                W.Cast();
            }
            if (E.IsReady() && me.Distance(target.Position) < E.Range && CountBuff(E.Range) > 0 && (
                (CountBuff(E.Range) >= minHit)
                || (Damage.GetSpellDamage(me, target, SpellSlot.E) >= target.Health)
                || (me.Distance(target) > config.Item("useEminr").GetValue<Slider>().Value && me.Distance(target) < E.Range && CountBuff(E.Range)==1)))
            {
                if (!(Q.IsReady() && me.Mana - me.Spellbook.GetSpell(SpellSlot.Q).ManaCost < me.MaxMana * perc) || !(W.IsReady() && me.Mana - me.Spellbook.GetSpell(SpellSlot.W).ManaCost < me.MaxMana * perc)) E.Cast();
            }
            if (Q.IsReady() && config.Item("useq").GetValue<bool>() && me.Spellbook.GetSpell(SpellSlot.Q).ManaCost <= me.Mana)
            {
                Q.Cast(target, config.Item("packets").GetValue<bool>());
            }
            

           
        }

        private static int CountBuff(float p)
        {
            var num = 0;
            foreach (var enemy in ObjectManager.Get<Obj_AI_Hero>().Where(i => i.IsEnemy && !i.IsDead && me.Distance(i) < p))
            {
                foreach (BuffInstance buff in enemy.Buffs)
                {
                    if (buff.Name == "sejuanifrost") num++;
                }
            }
            return num;
        }
        private static int CountBuffMini(float p)
        {
            var num = 0;
            foreach (var enemy in ObjectManager.Get<Obj_AI_Minion>().Where(i =>!i.IsDead && me.Distance(i) < p))
            {
                foreach (BuffInstance buff in enemy.Buffs)
                {
                    if (buff.Name == "sejuanifrost") num++;
                }
            }
            return num;
        }
        private static float ComboDamage(Obj_AI_Hero hero)
        {
            float damage = 0;
            if (Q.IsReady())damage += (float)Damage.GetSpellDamage(me, hero, SpellSlot.Q);
            if (E.IsReady()) damage += (float)Damage.GetSpellDamage(me, hero, SpellSlot.E);
            if (W.IsReady()) {
                double wdot = new double[] { 40, 70, 100, 130, 160 }[W.Level] + (new double[] { 4, 6, 8, 10, 12 }[W.Level] / 100) * me.MaxHealth;
                damage += (float)me.CalcDamage(hero, Damage.DamageType.Magical, wdot);
                damage += (float)Damage.GetSpellDamage(me, hero, SpellSlot.W);
            }
            if (R.IsReady())damage += (float)Damage.GetSpellDamage(me, hero, SpellSlot.R);
            if ((Items.HasItem(Bft.Id) && Items.CanUseItem(Bft.Id)) ||
                (Items.HasItem(Dfg.Id) && Items.CanUseItem(Dfg.Id)))
                damage = (float)(damage*1.2);
            if (Items.HasItem(Bft.Id) && Items.CanUseItem(Bft.Id))
            {
                damage += (float)me.GetItemDamage(hero, Damage.DamageItems.Dfg);
            }
            if (Items.HasItem(Dfg.Id) && Items.CanUseItem(Dfg.Id))
            {
                damage += (float)me.GetItemDamage(hero, Damage.DamageItems.Dfg);
            }
            if (me.Spellbook.CanUseSpell(me.GetSpellSlot("summonerdot")) == SpellState.Ready && hero.Health < damage + me.GetSummonerSpellDamage(hero, Damage.SummonerSpell.Ignite))
            {
                damage += (float)me.GetSummonerSpellDamage(hero, Damage.SummonerSpell.Ignite);
            }
            if (Items.HasItem(3077) && Items.CanUseItem(3077))
            {
                damage += (float)me.GetItemDamage(hero, Damage.DamageItems.Tiamat);
            }
            if (Items.HasItem(3074) && Items.CanUseItem(3074))
            {
                damage += (float)me.GetItemDamage(hero, Damage.DamageItems.Hydra);
            }
            if (Items.HasItem(3144) && Items.CanUseItem(3144))
            {
                damage += (float)me.GetItemDamage(hero, Damage.DamageItems.Bilgewater);
            }
            if (Items.HasItem(3153) && Items.CanUseItem(3153))
            {
                damage += (float)me.GetItemDamage(hero, Damage.DamageItems.Botrk);
            }
            if (Items.HasItem(3146) && Items.CanUseItem(3146))
            {
                damage += (float)me.GetItemDamage(hero, Damage.DamageItems.Hexgun);
            }
            if (Items.HasItem(3091) && Items.CanUseItem(3091))
            {
                damage += (float)me.CalcDamage(hero, Damage.DamageType.Magical, 42);
            }
            return damage;
        }

        private static void UseItems(Obj_AI_Hero target)
        {
            if (me.Distance(target) < 400)
            {
                if (Items.HasItem(3077) && Items.CanUseItem(3077))
                    Items.UseItem(3077);
                if (Items.HasItem(3074) && Items.CanUseItem(3074))
                    Items.UseItem(3074);
            }
            if (Items.HasItem(3144) && Items.CanUseItem(3144))
            {
                bilgewater.Cast(target);
            }
            if (Items.HasItem(3153) && Items.CanUseItem(3153) && me.Health < me.MaxHealth / 2)
            {
                botrk.Cast(target);
            }
            if (Items.HasItem(3146) && Items.CanUseItem(3146))
            {
                hexgun.Cast(target);
            }
            if (Items.HasItem(Dfg.Id) && Items.CanUseItem(Dfg.Id))
            {
                Dfg.Cast(target);
            }
            if (Items.HasItem(Bft.Id) && Items.CanUseItem(Bft.Id))
            {
                Bft.Cast(target);
            }
            if (me.Spellbook.CanUseSpell(me.GetSpellSlot("summonerdot")) == SpellState.Ready)
            {
                if (me.Distance(target) < 650 && ComboDamage(target) >= target.Health && (float)me.GetSummonerSpellDamage(target, Damage.SummonerSpell.Ignite) >= target.Health)
                {
                    me.Spellbook.CastSpell(me.GetSpellSlot("SummonerDot"), target);
                }
            }
        }

        private static void DrawCircle(string menuItem, float spellRange)
        {
            Circle circle = config.Item(menuItem).GetValue<Circle>();
            if (circle.Active) Utility.DrawCircle(me.Position, spellRange, circle.Color);
        }

        public static List<Obj_AI_Base> CheckingCollision(Obj_AI_Base From, Obj_AI_Base Target, Spell Skill, bool Mid = true, bool OnlyHero = false)
        {
            var ListCol = new List<Obj_AI_Base>();
            foreach (var Obj in ObjectManager.Get<Obj_AI_Base>().Where(i => IsValid(i, Skill.Range) && Skill.GetPrediction(i).Hitchance >= HitChance.Medium && ((!OnlyHero && !(i is Obj_AI_Turret)) || (OnlyHero && i is Obj_AI_Hero)) && i != Target))
            {
                var Segment = (Mid ? Obj : Target).Position.To2D().ProjectOn(From.Position.To2D(), (Mid ? Target : Obj).Position.To2D());
                if (Segment.IsOnSegment && Obj.Position.Distance(new Vector3(Segment.SegmentPoint.X, Obj.Position.Y, Segment.SegmentPoint.Y)) <= Skill.Width + Obj.BoundingRadius) ListCol.Add(Obj);
            }
            return ListCol.Distinct().ToList();
        }

        public static bool IsValid(Obj_AI_Base Target, float Range, bool EnemyOnly = true, Vector3 From = default(Vector3))
        {
            if (Target == null || !Target.IsValid || Target.IsDead || !Target.IsVisible || (EnemyOnly && !Target.IsTargetable) || (EnemyOnly && Target.IsInvulnerable) || Target.IsMe) return false;
            if (EnemyOnly ? Target.IsAlly : Target.IsEnemy) return false;
            if ((From != default(Vector3) ? From : me.Position).Distance(Target.Position) > Range) return false;
            return true;
        }
    }
}
