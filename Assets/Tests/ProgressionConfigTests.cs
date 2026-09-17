using NUnit.Framework;
using AnimalGrid.Core;

namespace AnimalGrid.Tests
{
    [TestFixture]
    public class ProgressionConfigTests
    {
        [Test]
        public void Level1_IsNormal_5x5()
        {
            var c = ProgressionConfig.GetLevel(1);
            Assert.IsFalse(c.isBoss);
            Assert.AreEqual(5, c.gridSize);
        }

        [Test]
        public void Level12_Still5x5()
        {
            Assert.AreEqual(5, ProgressionConfig.GetLevel(12).gridSize);
        }

        [Test]
        public void Level13_Is6x6()
        {
            var c = ProgressionConfig.GetLevel(13);
            Assert.IsFalse(c.isBoss);
            Assert.AreEqual(6, c.gridSize);
        }

        [Test]
        public void Level10_IsBoss_6x6()
        {
            var c = ProgressionConfig.GetLevel(10);
            Assert.IsTrue(c.isBoss);
            Assert.AreEqual(6, c.gridSize);
        }

        [Test]
        public void Level20_IsBoss_7x7()
        {
            var c = ProgressionConfig.GetLevel(20);
            Assert.IsTrue(c.isBoss);
            Assert.AreEqual(7, c.gridSize);
        }

        [Test]
        public void Level50_IsBoss_10x10()
        {
            var c = ProgressionConfig.GetLevel(50);
            Assert.IsTrue(c.isBoss);
            Assert.AreEqual(10, c.gridSize);
        }

        [Test]
        public void Level80_Boss_CappedAtMax()
        {
            Assert.AreEqual(ProgressionConfig.MaxGridSize, ProgressionConfig.GetLevel(80).gridSize);
        }

        [Test]
        public void Boss_HasDoubleRewardMultiplier()
        {
            Assert.AreEqual(2, ProgressionConfig.GetLevel(10).rewardMultiplier);
            Assert.AreEqual(1, ProgressionConfig.GetLevel(1).rewardMultiplier);
        }

        [Test]
        public void NormalCurve_NeverDecreases()
        {
            int prev = 0;
            for (int i = 1; i <= 100; i++)
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
            for (int b = 10; b <= 70; b += 10)
            {
                var boss = ProgressionConfig.GetLevel(b);
                var normal = ProgressionConfig.GetLevel(b - 1);
                Assert.Greater(boss.gridSize, normal.gridSize);
            }
        }

        [Test]
        public void Difficulty_AlwaysInRange()
        {
            for (int i = 1; i <= 120; i++)
            {
                var c = ProgressionConfig.GetLevel(i);
                Assert.GreaterOrEqual(c.difficultyTarget, 1);
                Assert.LessOrEqual(c.difficultyTarget, 10);
            }
        }
    }
}