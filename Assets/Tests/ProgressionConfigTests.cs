using NUnit.Framework;
using AnimalGrid.Core;

namespace AnimalGrid.Tests
{
    [TestFixture]
    public class ProgressionConfigTests
    {
        [Test]
        public void NormalCurve_MatchesWorldBands()
        {
            // Sample NON-boss levels only (bosses are every 10th)
            Assert.AreEqual(5, ProgressionConfig.GetLevel(1).gridSize);
            Assert.AreEqual(5, ProgressionConfig.GetLevel(12).gridSize);
            Assert.AreEqual(6, ProgressionConfig.GetLevel(13).gridSize);
            Assert.AreEqual(6, ProgressionConfig.GetLevel(29).gridSize);
            Assert.AreEqual(7, ProgressionConfig.GetLevel(31).gridSize);
            Assert.AreEqual(7, ProgressionConfig.GetLevel(59).gridSize);
            Assert.AreEqual(8, ProgressionConfig.GetLevel(61).gridSize);
            Assert.AreEqual(8, ProgressionConfig.GetLevel(99).gridSize);
            Assert.AreEqual(9, ProgressionConfig.GetLevel(101).gridSize);
            Assert.AreEqual(9, ProgressionConfig.GetLevel(139).gridSize);
            Assert.AreEqual(10, ProgressionConfig.GetLevel(141).gridSize);
            Assert.AreEqual(10, ProgressionConfig.GetLevel(179).gridSize);
        }

        [Test]
        public void Boss_EveryTenthLevel_GrowsFasterAndCapsAt10()
        {
            Assert.IsTrue(ProgressionConfig.GetLevel(10).isBoss);
            Assert.IsFalse(ProgressionConfig.GetLevel(11).isBoss);
            Assert.AreEqual(6, ProgressionConfig.GetLevel(10).gridSize);
            Assert.AreEqual(7, ProgressionConfig.GetLevel(20).gridSize);
            Assert.AreEqual(8, ProgressionConfig.GetLevel(30).gridSize);
            Assert.AreEqual(9, ProgressionConfig.GetLevel(40).gridSize);
            Assert.AreEqual(10, ProgressionConfig.GetLevel(50).gridSize);
            Assert.AreEqual(10, ProgressionConfig.GetLevel(180).gridSize);
        }

        [Test]
        public void GridSize_NeverExceeds10()
        {
            for (int i = 1; i <= 180; i++)
            {
                Assert.LessOrEqual(ProgressionConfig.GetLevel(i).gridSize, ProgressionConfig.MaxGridSize);
                Assert.GreaterOrEqual(ProgressionConfig.GetLevel(i).gridSize, 5);
            }
        }

        [Test]
        public void NormalCurve_NeverDecreases()
        {
            int prev = 0;
            for (int i = 1; i <= 180; i++)
            {
                if (ProgressionConfig.IsBoss(i)) continue;
                int size = ProgressionConfig.GetLevel(i).gridSize;
                Assert.GreaterOrEqual(size, prev);
                prev = size;
            }
        }

        [Test]
        public void Boss_AlwaysLargerThanNeighbouringNormal()
        {
            for (int b = 10; b <= 180; b += 10)
            {
                var boss = ProgressionConfig.GetLevel(b);
                var normal = ProgressionConfig.GetLevel(b - 1);
                Assert.GreaterOrEqual(boss.gridSize, normal.gridSize);
            }
        }

        [Test]
        public void Difficulty_AndReward_InRange()
        {
            for (int i = 1; i <= 180; i++)
            {
                var c = ProgressionConfig.GetLevel(i);
                Assert.GreaterOrEqual(c.difficultyTarget, 1);
                Assert.LessOrEqual(c.difficultyTarget, 10);
                Assert.AreEqual(c.isBoss ? 2 : 1, c.rewardMultiplier);
            }
        }
    }
}