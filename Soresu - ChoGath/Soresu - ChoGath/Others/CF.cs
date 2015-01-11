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
using Soresu___ChoGath;

namespace Others
{

    class CF
    {
        public static Items.Item botrk = new Items.Item(3153, 450);
        public static Items.Item bilgewater = new Items.Item(3144, 450);
        public static Items.Item hexgun = new Items.Item(3146, 700);
        public static Items.Item Dfg = new Items.Item(3128, 750);
        public static Items.Item Bft = new Items.Item(3188, 750);
        public static Items.Item odins = new Items.Item(3180, 525);
        public static readonly Obj_AI_Hero player = Program.player;
        public static Spell Q=Program.Q, W=Program.W, E=Program.E, R=Program.R, RFlash=Program.RFlash;
        private static readonly string[] jungleMonsters = { "TT_Spiderboss", "SRU_Blue", "SRU_Red", "SRU_Dragon", "SRU_Baron" };
        private static List<string> dotsHighDmg = new List<string>(new string[] { "karthusfallenonecastsound", "CaitlynAceintheHole", "zedulttargetmark", "timebombenemybuff", "VladimirHemoplague" });
        private static List<string> dotsMedDmg = new List<string>(new string[] { "summonerdot", "cassiopeiamiasmapoison", "cassiopeianoxiousblastpoison", "bantamtraptarget", "explosiveshotdebuff", "swainbeamdamage", "SwainTorment", "AlZaharMaleficVisions", "fizzmarinerdoombomb" });
        private static List<string> dotsSmallDmg = new List<string>(new string[] { "deadlyvenom", "toxicshotparticle", "MordekaiserChildrenOfTheGrave", "DariusNoxianTacticsONH" });
        public static void UseSpells(Obj_AI_Hero target)
        {
            if (player.Distance(target) < 400)
            {
                //tiamat
                if (Items.HasItem(3077) && Items.CanUseItem(3077)) Items.UseItem(3077);
                if (Items.HasItem(3074) && Items.CanUseItem(3074)) Items.UseItem(3074);
            }
            if (player.Distance(target) < 500 && player.Distance(target)>player.AttackRange+100)
            {
                //randuin
                if (Items.HasItem(3143) && Items.CanUseItem(3143)) Items.UseItem(3143);
            }
            if (Items.HasItem(3180) && Items.CanUseItem(3180))
            {
                if (player.Distance(target) < 525 && (player.CountEnemysInRange(525) > 1 || target.Health < Damage.GetItemDamage(player, target, Damage.DamageItems.OdingVeils))) Items.UseItem(3180);
            }

            if (Items.HasItem(3144) && Items.CanUseItem(3144))
            {
                bilgewater.Cast(target);
            }
            if (Items.HasItem(3153) && Items.CanUseItem(3153) && player.Health < player.MaxHealth / 2)
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
            if (player.Spellbook.CanUseSpell(player.GetSpellSlot("summonerdot")) == SpellState.Ready)
            {
                if (player.Distance(target) < 650 && ComboDamage(target) >= target.Health && (float)player.GetSummonerSpellDamage(target, Damage.SummonerSpell.Ignite) >= target.Health)
                {
                    player.Spellbook.CastSpell(player.GetSpellSlot("SummonerDot"), target);
                }
            }
        }

        public static float ComboDamage(Obj_AI_Hero hero)
        {
            float damage = 0;
            if (Q.IsReady()) damage += (float)Damage.GetSpellDamage(player, hero, SpellSlot.Q);
            if (E.IsReady()) damage += (float)Damage.GetSpellDamage(player, hero, SpellSlot.E);
            if (W.IsReady()) damage += (float)Damage.GetSpellDamage(player, hero, SpellSlot.W);
            if ((Items.HasItem(Bft.Id) && Items.CanUseItem(Bft.Id)) ||
                (Items.HasItem(Dfg.Id) && Items.CanUseItem(Dfg.Id)))
                damage = (float)(damage * 1.2);
            if (Items.HasItem(Bft.Id) && Items.CanUseItem(Bft.Id))
            {
                damage += (float)player.GetItemDamage(hero, Damage.DamageItems.Dfg);
            }
            if (Items.HasItem(Dfg.Id) && Items.CanUseItem(Dfg.Id))
            {
                damage += (float)player.GetItemDamage(hero, Damage.DamageItems.Dfg);
            }

            if (R.IsReady()) damage += (float)Damage.GetSpellDamage(player, hero, SpellSlot.R);

            if (player.Spellbook.CanUseSpell(player.GetSpellSlot("summonerdot")) == SpellState.Ready && hero.Health < damage + player.GetSummonerSpellDamage(hero, Damage.SummonerSpell.Ignite))
            {
                damage += (float)player.GetSummonerSpellDamage(hero, Damage.SummonerSpell.Ignite);
            }
            if (Items.HasItem(3077) && Items.CanUseItem(3077))
            {
                damage += (float)player.GetItemDamage(hero, Damage.DamageItems.Tiamat);
            }
            if (Items.HasItem(3074) && Items.CanUseItem(3074))
            {
                damage += (float)player.GetItemDamage(hero, Damage.DamageItems.Hydra);
            }
            if (Items.HasItem(3144) && Items.CanUseItem(3144))
            {
                damage += (float)player.GetItemDamage(hero, Damage.DamageItems.Bilgewater);
            }
            if (Items.HasItem(3153) && Items.CanUseItem(3153))
            {
                damage += (float)player.GetItemDamage(hero, Damage.DamageItems.Botrk);
            }
            if (Items.HasItem(3146) && Items.CanUseItem(3146))
            {
                damage += (float)player.GetItemDamage(hero, Damage.DamageItems.Hexgun);
            }
            if (Items.HasItem(3091) && Items.CanUseItem(3091))
            {
                damage += (float)player.CalcDamage(hero, Damage.DamageType.Magical, 42);
            }
            return damage;
        }
        public static Obj_AI_Minion GetNearest(Vector3 pos)
        {
            var minions =
            ObjectManager.Get<Obj_AI_Minion>()
            .Where(minion => minion.IsValid && jungleMonsters.Any(name => minion.Name.StartsWith(name)) && !jungleMonsters.Any(name => minion.Name.Contains("Mini")) && !jungleMonsters.Any(name => minion.Name.Contains("Spawn")));
            var objAiMinions = minions as Obj_AI_Minion[] ?? minions.ToArray();
            Obj_AI_Minion sMinion = objAiMinions.FirstOrDefault();
            double? nearest = null;
            foreach (Obj_AI_Minion minion in objAiMinions)
            {
                double distance = Vector3.Distance(pos, minion.Position);
                if (nearest == null || nearest > distance)
                {
                    nearest = distance;
                    sMinion = minion;
                }
            }
            return sMinion;
        }

        public static void DrawCircle(string menuItem, float spellRange)
        {
            Circle circle = Program.config.Item(menuItem).GetValue<Circle>();
            if (circle.Active) Utility.DrawCircle(player.Position, spellRange, circle.Color);
        }

        public static double smiteDamage()
        {
            int level = ObjectManager.Player.Level;
            int[] damage =
                {
                20*level + 370,
                30*level + 330,
                40*level + 240,
                50*level + 100
                };
            return damage.Max();
        }
        //Kurisu
        public static readonly int[] SmitePurple = { 3713, 3726, 3725, 3726, 3723 };
        public static readonly int[] SmiteGrey = { 3711, 3722, 3721, 3720, 3719 };
        public static readonly int[] SmiteRed = { 3715, 3718, 3717, 3716, 3714 };
        public static readonly int[] SmiteBlue = { 3706, 3710, 3709, 3708, 3707 };
        public static string smitetype()
        {
            if (SmiteBlue.Any(id => Items.HasItem(id)))
            {
                return "s5_summonersmiteplayerganker";
            }
            if (SmiteRed.Any(id => Items.HasItem(id)))
            {
                return "s5_summonersmiteduel";
            }
            if (SmiteGrey.Any(id => Items.HasItem(id)))
            {
                return "s5_summonersmitequick";
            }
            if (SmitePurple.Any(id => Items.HasItem(id)))
            {
                return "itemsmiteaoe";
            }
            return "summonersmite";
        }

        public static void setSmiteSlot()
        {
            foreach (var spell in ObjectManager.Player.Spellbook.Spells.Where(spell => String.Equals(spell.Name, smitetype(), StringComparison.CurrentCultureIgnoreCase)))
            {
                Program.smiteSlot = spell.Slot;
                Program.smite = new Spell(Program.smiteSlot, 700);
                return;
            }
        }
        public static void CastSmite(Obj_AI_Minion target)
        {
            Program.smite.Slot = Program.smiteSlot;
            ObjectManager.Player.Spellbook.CastSpell(Program.smiteSlot, target);
        }

        public static int countMinionsInrange(Obj_AI_Minion l, float p)
        {
            return ObjectManager.Get<Obj_AI_Minion>().Count(i => !i.IsDead && i.IsEnemy && l.Distance(i) < p);

        }
        public static int countTurretsInRange(Obj_AI_Hero l)
        {
            return ObjectManager.Get<Obj_AI_Turret>().Count(i => !i.IsDead && i.IsEnemy && l.Distance(i) < 750f);

        }
        public static bool CheckCriticalBuffs(Obj_AI_Hero i)
        {
            foreach (BuffInstance buff in i.Buffs)
            {
                if (i.Health <= 30 && dotsSmallDmg.Contains(buff.Name))
                {
                    return true;
                }
                if (i.Health <= 60 && dotsMedDmg.Contains(buff.Name))
                {
                    return true;
                }
                if (i.Health <= 150 && dotsHighDmg.Contains(buff.Name))
                {
                    return true;
                }
            }
            return false;
        }
    }

    













}
