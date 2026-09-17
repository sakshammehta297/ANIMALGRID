using System.Collections.Generic;

namespace AnimalGrid.Core
{
    /// <summary>
    /// Decides which animal appears in which color region for a level.
    /// K <= N: every unlocked animal appears at least once, rest filled evenly.
    /// K >  N: a seeded random subset of N unlocked animals.
    /// Deterministic per seed. Pure C# - no Unity dependencies!
    /// </summary>
    public static class RosterDistributor
    {
        public static List<string> Distribute(List<string> unlocked, int regions, int seed)
        {
            var result = new List<string>();
            if (unlocked == null || unlocked.Count == 0 || regions <= 0) return result;

            var random = new System.Random(seed);

            if (unlocked.Count >= regions)
            {
                var pool = new List<string>(unlocked);
                Shuffle(pool, random);
                for (int i = 0; i < regions; i++) result.Add(pool[i]);
                return result;
            }

            var bag = new List<string>(unlocked);
            Shuffle(bag, random);
            for (int i = 0; i < regions; i++)
            {
                if (bag.Count == 0)
                {
                    bag = new List<string>(unlocked);
                    Shuffle(bag, random);
                }
                result.Add(bag[0]);
                bag.RemoveAt(0);
            }
            return result;
        }

        private static void Shuffle(List<string> list, System.Random random)
        {
            for (int i = list.Count - 1; i > 0; i--)
            {
                int j = random.Next(i + 1);
                var tmp = list[i]; list[i] = list[j]; list[j] = tmp;
            }
        }
    }
}