namespace AnimalGrid.Core
{
    public struct LevelConfig
    {
        public int levelNumber;   // level inside the current world
        public bool isBoss;
        public int gridSize;
        public int difficultyTarget;
        public int rewardMultiplier;
    }

    /// <summary>
    /// Data-driven size curve per world (spec 8). Grid cap = 10x10.
    /// Pure C# - no Unity dependencies!
    /// </summary>
    public static class ProgressionConfig
    {
        // [upToLevelInWorld, gridSize]
        private static readonly int[][] NormalCurve =
        {
            new[] { 12, 5 },
            new[] { 30, 6 },
            new[] { 60, 7 },
            new[] { 100, 8 },
            new[] { 140, 9 },
            new[] { int.MaxValue, 10 }
        };

        public const int MaxGridSize = 10;
        public const int MaxLevelInWorld = 180;

        public static bool IsBoss(int levelInWorld)
        {
            return levelInWorld % 10 == 0;
        }

        public static LevelConfig GetLevel(int levelInWorld)
        {
            if (levelInWorld < 1) levelInWorld = 1;
            if (levelInWorld > MaxLevelInWorld) levelInWorld = MaxLevelInWorld;
            bool boss = IsBoss(levelInWorld);
            int size = boss ? BossSize(levelInWorld) : NormalSize(levelInWorld);
            if (size > MaxGridSize) size = MaxGridSize;
            if (size < 5) size = 5;

            var config = new LevelConfig();
            config.levelNumber = levelInWorld;
            config.isBoss = boss;
            config.gridSize = size;
            config.difficultyTarget = System.Math.Min(10, System.Math.Max(1, size - 3));
            config.rewardMultiplier = boss ? 2 : 1;
            return config;
        }

        private static int NormalSize(int levelInWorld)
        {
            foreach (var band in NormalCurve)
            {
                if (levelInWorld <= band[0]) return band[1];
            }
            return 10;
        }

        private static int BossSize(int levelInWorld)
        {
            return System.Math.Min(MaxGridSize, 5 + levelInWorld / 10);
        }
    }
}