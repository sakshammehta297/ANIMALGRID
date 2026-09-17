namespace AnimalGrid.Core
{
    public struct LevelConfig
    {
        public int levelNumber;
        public bool isBoss;
        public int gridSize;
        public int difficultyTarget;
        public int rewardMultiplier;
    }

    /// <summary>
    /// Data-driven progression curve (spec 8). Tunable tables - not hard-coded in gameplay code.
    /// Pure C# - no Unity dependencies!
    /// </summary>
    public static class ProgressionConfig
    {
        private static readonly int[][] NormalCurve =
        {
            new[] { 12, 5 },
            new[] { 25, 6 },
            new[] { 45, 6 },
            new[] { 65, 7 },
            new[] { 85, 8 },
            new[] { int.MaxValue, 9 }
        };

        public const int MaxGridSize = 12;

        public static bool IsBoss(int levelNumber)
        {
            return levelNumber % 10 == 0;
        }

        public static LevelConfig GetLevel(int levelNumber)
        {
            if (levelNumber < 1) levelNumber = 1;
            bool boss = IsBoss(levelNumber);
            int size = boss ? BossSize(levelNumber) : NormalSize(levelNumber);
            if (size > MaxGridSize) size = MaxGridSize;
            if (size < 5) size = 5;

            var config = new LevelConfig();
            config.levelNumber = levelNumber;
            config.isBoss = boss;
            config.gridSize = size;
            config.difficultyTarget = System.Math.Min(10, System.Math.Max(1, size - 3));
            config.rewardMultiplier = boss ? 2 : 1;
            return config;
        }

        private static int NormalSize(int levelNumber)
        {
            foreach (var band in NormalCurve)
            {
                if (levelNumber <= band[0]) return band[1];
            }
            return 9;
        }

        private static int BossSize(int levelNumber)
        {
            return 5 + levelNumber / 10;
        }
    }
}