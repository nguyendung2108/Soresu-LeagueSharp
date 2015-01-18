using System.Collections.Generic;
using System.Linq;

using LeagueSharp;
using LeagueSharp.Common;
using SharpDX;

namespace UnderratedAIO.Helpers
{
    public class Environment
    {
        public static Obj_AI_Hero player = ObjectManager.Player;

        public class Minion
        {
            public static int countMinionsInrange(Obj_AI_Minion l, float p)
            {
                return ObjectManager.Get<Obj_AI_Minion>().Count(i => !i.IsDead && i.IsEnemy && l.Distance(i) < p);
            }
            public static int countMinionsInrange(Vector3 l, float p)
            {
                return ObjectManager.Get<Obj_AI_Minion>().Count(i => !i.IsDead && i.IsEnemy && i.Distance(l) < p);
            }
            public static Vector3 bestVectorToAoeFarm(List<Obj_AI_Base> minions, float spellrange)
            {
                Vector3 bestPos = new Vector3();
                int hits = 0;
                foreach (var minion in minions)
                {

                    if (countMinionsInrange(minion.Position, 170f) > hits) bestPos = minion.Position;
                    Vector3 newPos = new Vector3(minion.Position.X + 80, minion.Position.Y + 80, minion.Position.Z);
                    for (int i = 1; i < 4; i++)
                    {
                        var rotated = newPos.To2D().RotateAroundPoint(newPos.To2D(), 90 * i).To3D();
                        if (countMinionsInrange(rotated, 170f) > hits && player.Distance(rotated) <= spellrange) bestPos = newPos;
                    }
                }

                return bestPos;
            }
        }

        public class Hero
        {
            public static int countChampsAtrange(Obj_AI_Hero l, float p)
            {
                return ObjectManager.Get<Obj_AI_Hero>().Count(i => !i.IsDead && i.IsEnemy && i.Distance(l) < p);
            }
        }

        public class Turret
        {
            public static int countTurretsInRange(Obj_AI_Hero l)
            {
                return ObjectManager.Get<Obj_AI_Turret>().Count(i => !i.IsDead && i.IsEnemy && l.Distance(i) < 750f);

            }
        }

        public class Map
        {
            public static bool CheckWalls(Vector3 player, Vector3 enemy)
            {
                var distance = player.Distance(enemy);
                for (int i = 1; i < 6; i++)
                {
                    if (player.Extend(enemy, distance + 60 * i).IsWall())
                        return true;
                }
                return false;
            }
        }
    }
}