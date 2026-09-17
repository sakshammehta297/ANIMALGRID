namespace AnimalGrid.Core
{
    /// <summary>
    /// The unlock bar math: progress points -> animal unlocks.
    /// Gaps are the single tunable table for the whole campaign (spec 8).
    /// Pure C# - no Unity dependencies!
    /// </summary>
    public static class UnlockProgression
    {
        // Points needed between unlocks: starter -> #2 -> #3 ... -> #10
        public static readonly int[] Gaps = { 10, 15, 20, 20, 20, 20, 20, 25, 30 };

        private static int[] _thresholds;

        public static int[] Thresholds
        {
            get
            {
                if (_thresholds == null)
                {
                    _thresholds = new int[Gaps.Length];
                    int sum = 0;
                    for (int i = 0; i < Gaps.Length; i++)
                    {
                        sum += Gaps[i];
                        _thresholds[i] = sum;
                    }
                }
                return _thresholds;
            }
        }

        // 180 = the per-world level cap
        public static int MaxPoints
        {
            get { return Thresholds[Thresholds.Length - 1]; }
        }

        public static int PointsForLevel(bool isBoss)
        {
            return isBoss ? 2 : 1;
        }

        /// <summary>How many animals are unlocked at this point total (starter included).</summary>
        public static int UnlockedCount(int points)
        {
            int count = 1;
            foreach (var t in Thresholds)
            {
                if (points >= t) count++;
            }
            return count;
        }

        /// <summary>0..1 fill of the bar within the current unlock segment.</summary>
        public static float BarFraction(int points)
        {
            var th = Thresholds;
            int prev = 0;
            for (int i = 0; i < th.Length; i++)
            {
                if (points < th[i]) return (points - prev) / (float)(th[i] - prev);
                prev = th[i];
            }
            return 1f;
        }
    }
}