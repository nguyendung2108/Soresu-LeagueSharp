using System;
using LeagueSharp;
using LeagueSharp.Common;

namespace ViTest
{
    internal class Program
    {
        public static Menu config;
        private static Orbwalking.Orbwalker orbwalker;
        public static readonly Obj_AI_Hero player = ObjectManager.Player;
        public static Spell Q, E, E2, R;
        public static Obj_AI_Base targetQ;
        private static void Main(string[] args)
        {
            if (player.BaseSkinName != "Vi") return;
            CustomEvents.Game.OnGameLoad += OnStart;
        }

        private static void OnStart(EventArgs args)
        {
            
            Q = new Spell(SpellSlot.Q, 860f);
            E = new Spell(SpellSlot.E);
            E2 = new Spell(SpellSlot.E, 600f);
            R = new Spell(SpellSlot.R, 800f);
            Q.SetSkillshot(0.5f, 75f, float.MaxValue, false, SkillshotType.SkillshotLine);
            Q.SetCharged("ViQ", "ViQ", 100, 860, 1f);
            E.SetSkillshot(0.15f, 150f, float.MaxValue, false, SkillshotType.SkillshotLine);
            R.SetTargetted(0.15f, 1500f);
            InitMenu();
            Game.OnUpdate += OnUpdate;
            Orbwalking.AfterAttack += AfterAttack;
            Game.PrintChat("<font color='#9933FF'>Soresu </font><font color='#FFFFFF'>- Vi</font>");
        }

        private static void AfterAttack(AttackableUnit unit, AttackableUnit target)
        {
            if (unit.IsMe && target.IsEnemy && target is Obj_AI_Hero && config.Item("FastCombo").GetValue<KeyBind>().Active)
            {
                if (E.IsReady())
                {
                    E.Cast(config.Item("packets").GetValue<bool>());
                    Orbwalking.ResetAutoAttackTimer();
                    return;
                }
                if (!E.IsReady() && Q.IsReady() && !Q.IsCharging)
                {

                    {
                        Q.StartCharging();
                        targetQ = (Obj_AI_Base)target;
                    }

                }
            }
        }

        private static void OnUpdate(EventArgs args)
        {
             Obj_AI_Hero target = TargetSelector.GetTarget(R.Range, TargetSelector.DamageType.Physical);
            if (config.Item("FastCombo").GetValue<KeyBind>().Active)
            {
                if (config.Item("UseUlt").GetValue<bool>() && R.CanCast(target))
                {
                    R.Cast(target);
                    Orbwalking.ResetAutoAttackTimer();
                }
                player.IssueOrder(GameObjectOrder.AttackUnit, target);
            }
            if (Q.IsCharging)
            {
                if (targetQ!=null)
                {
                    target = (Obj_AI_Hero)targetQ;
                }
                Q.Cast(target.Position);
                targetQ = null;
                Orbwalking.ResetAutoAttackTimer();
            }
        }

        private static void InitMenu()
        {
            config = new Menu("Vi", "Vi", true);
            // Target Selector
            var menuTS = new Menu("Selector", "tselect");
            TargetSelector.AddToMenu(menuTS);
            config.AddSubMenu(menuTS);

            // Orbwalker
            var menuOrb = new Menu("Orbwalker", "orbwalker");
            orbwalker = new Orbwalking.Orbwalker(menuOrb);
            config.AddSubMenu(menuOrb);

            var menuD = new Menu("Combo ", "dsettings");
            menuD.AddItem(new MenuItem("FastCombo", "FastCombo"))
                .SetValue(new KeyBind("T".ToCharArray()[0], KeyBindType.Press));
            menuD.AddItem(new MenuItem("UseUlt", "Use ult")).SetValue(false);
            config.AddSubMenu(menuD);
            config.AddItem(new MenuItem("packets", "Use Packets")).SetValue(false);
            config.AddToMainMenu();
        }
    }
}