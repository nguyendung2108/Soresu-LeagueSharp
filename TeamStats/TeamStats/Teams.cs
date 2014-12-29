using LeagueSharp;
using LeagueSharp.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TeamStats
{
    internal class Teams
    {
        public List<Obj_AI_Hero> myTeam = new List<Obj_AI_Hero>();
        public List<Obj_AI_Hero> EnemyTeam = new List<Obj_AI_Hero>();
        public int myTeamNum;
        public int enemyTeamNum;
        public int myTeamDmg;
        public int enemyTeamDmg;
        public int myTeamHP { get; set; }
        public int enemyTeamHP { get; set; }

        public Teams()
        {
            SetNums();
        }
        public void SetNums()
        {
            foreach (var player in ObjectManager.Get<Obj_AI_Hero>().Where(i => !i.IsDead && !i.IsMinion && Program.player.Distance(i) < Program.range))
            {
                //list
                if (player.IsEnemy) EnemyTeam.Add(player);
                if (player.IsAlly || player.IsMe) myTeam.Add(player);
                //num
                if (player.IsEnemy) enemyTeamNum++;
                if (player.IsAlly || player.IsMe) myTeamNum++;
            }
            myTeam = myTeam.OrderByDescending(i => i.ChampionsKilled).ToList();
            EnemyTeam = EnemyTeam.OrderByDescending(i => i.ChampionsKilled).ToList();
            var e = 0;
            var eHP = EnemyTeam[0].Health;
            foreach (var enemy in EnemyTeam)
            {
                if (eHP < 0)
                {
                    e++;
                    eHP = myTeam[e].Health;
                }
                enemyTeamDmg += (int)ComboDamage(enemy, myTeam[e]);
                eHP -=(int)enemy.GetComboDamage(myTeam[e], new[] {SpellSlot.Q, SpellSlot.W, SpellSlot.E, SpellSlot.R});
                enemyTeamHP += (int)((enemy.Health * ((1 + (1 - (100 / (enemy.Armor + 100))))) + enemy.Health * (0.3 + 1)) / 2);

            }
            var t = 0;
            var tHP = EnemyTeam[0].Health;
            foreach (var teammate in myTeam)
            {
                if (tHP < 0)
                {
                    t++;
                    tHP = EnemyTeam[t].Health;
                }
                myTeamDmg += (int)ComboDamage(teammate, EnemyTeam[t]);
                tHP -= (int)teammate.GetComboDamage(EnemyTeam[t], new[] { SpellSlot.Q, SpellSlot.W, SpellSlot.E, SpellSlot.R });
                myTeamHP += (int)((teammate.Health * ((1 + (1 - (100 / (teammate.Armor + 100))))) + teammate.Health * (0.3 + 1)) / 2);
            }
        }
        private static float ComboDamage(Obj_AI_Hero src, Obj_AI_Hero dsc)
        {
            Random rnd = new Random();
            float basicDmg = 0;
            int attacks = (int)Math.Floor(src.AttackSpeedMod*5);
            for (int i = 0; i < attacks; i++)
            {
                if (src.Crit>0)
                {
                    basicDmg += (src.BaseAttackDamage + src.FlatPhysicalDamageMod)*(1 + src.Crit);
                }
                else
                {
                    basicDmg += src.BaseAttackDamage + src.FlatPhysicalDamageMod;
                }  
            };
            float damage = basicDmg;
            var spells = src.Spellbook.Spells;
            foreach (var spell in spells)
            {
                var t = spell.CooldownExpires - Game.Time;
                
                if (t < 0.5 && spell.Level > 0 && spell.SData.SpellCastTime < 2f)
                {
                    if (spell.Cooldown < 2.5f && spell.Cooldown>0)
                    {
                        int count = (int)Math.Floor(5/spell.Cooldown);
                        for (int i = 0; i < count; i++)
                        {
                            damage += (float)Damage.GetSpellDamage(src, dsc, spell.Slot); 
                        }
                        
                    }
                    
                }
            }
            if (src.Spellbook.CanUseSpell(src.GetSpellSlot("summonerdot")) == SpellState.Ready)
            {
                damage += (float)src.GetSummonerSpellDamage(dsc, Damage.SummonerSpell.Ignite);
            }
            return damage;
        }
     }
 }
