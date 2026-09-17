using NUnit.Framework;
using AnimalGrid.Core;

namespace AnimalGrid.Tests
{
    [TestFixture]
    public class WorldProgressionTests
    {
        [Test]
        public void Thresholds_AreCumulativeAndCapAt180()
        {
            var th = UnlockProgression.Thresholds;
            Assert.AreEqual(9, th.Length);
            Assert.AreEqual(10, th[0]);
            Assert.AreEqual(25, th[1]);
            Assert.AreEqual(45, th[2]);
            Assert.AreEqual(180, th[8]);
            Assert.AreEqual(180, UnlockProgression.MaxPoints);
        }

        [Test]
        public void UnlockedCount_MatchesThresholds()
        {
            Assert.AreEqual(1, UnlockProgression.UnlockedCount(0));
            Assert.AreEqual(1, UnlockProgression.UnlockedCount(9));
            Assert.AreEqual(2, UnlockProgression.UnlockedCount(10));
            Assert.AreEqual(2, UnlockProgression.UnlockedCount(24));
            Assert.AreEqual(3, UnlockProgression.UnlockedCount(25));
            Assert.AreEqual(10, UnlockProgression.UnlockedCount(180));
        }

        [Test]
        public void BarFraction_FillsWithinSegment()
        {
            Assert.AreEqual(0f, UnlockProgression.BarFraction(0), 0.001f);
            Assert.AreEqual(0.5f, UnlockProgression.BarFraction(5), 0.001f);
            Assert.AreEqual(0f, UnlockProgression.BarFraction(10), 0.001f);
            Assert.AreEqual(1f, UnlockProgression.BarFraction(180), 0.001f);
        }

        [Test]
        public void Boss_FillsTwice()
        {
            Assert.AreEqual(1, UnlockProgression.PointsForLevel(false));
            Assert.AreEqual(2, UnlockProgression.PointsForLevel(true));
        }

        [Test]
        public void FreshCampaign_StartsWorld1Level1WithOneAnimal()
        {
            var state = CampaignState.Fresh();
            Assert.AreEqual(0, state.worldIndex);
            Assert.AreEqual(1, state.levelInWorld);
            Assert.AreEqual(1, state.UnlockedInWorld(0));
            Assert.IsFalse(state.gameCompleted);
        }

        [Test]
        public void FirstUnlock_HappensAfter10Points()
        {
            var state = CampaignState.Fresh();
            ClearResult last = null;
            for (int i = 0; i < 10; i++) last = state.RecordLevelClear(false);
            Assert.AreEqual(1, last.newlyUnlocked.Count);
            Assert.AreEqual(1, last.newlyUnlocked[0]); // animal #2 (index 1)
            Assert.IsFalse(last.worldCompleted);
            Assert.AreEqual(2, state.UnlockedInWorld(0));
            Assert.AreEqual(11, state.levelInWorld);
        }

        [Test]
        public void BossClear_AddsTwoPoints()
        {
            var state = CampaignState.Fresh();
            for (int i = 0; i < 9; i++) state.RecordLevelClear(false);
            state.RecordLevelClear(true); // 9 + 2 = 11 points
            Assert.AreEqual(11, state.worldPoints[0]);
            Assert.AreEqual(2, state.UnlockedInWorld(0));
        }

        [Test]
        public void WorldCompletes_At180Points_AndResetsNextWorld()
        {
            var state = CampaignState.Fresh();
            ClearResult result = null;
            int guard = 0;
            while (guard < 400)
            {
                bool boss = state.levelInWorld % 10 == 0;
                result = state.RecordLevelClear(boss);
                guard++;
                if (result.worldCompleted) break;
            }

            Assert.IsNotNull(result);
            Assert.IsTrue(result.worldCompleted);
            Assert.IsFalse(result.gameCompleted);
            Assert.AreEqual(10, state.UnlockedInWorld(0));   // forest complete
            Assert.AreEqual(1, state.worldIndex);            // now sky
            Assert.AreEqual(1, state.levelInWorld);          // level count reset
            Assert.AreEqual(1, state.UnlockedInWorld(1));    // sky starter only
            Assert.LessOrEqual(guard, 180);                  // never exceeds the cap
        }

        [Test]
        public void GameCompletes_AfterThirdWorld()
        {
            var state = CampaignState.Fresh();
            ClearResult result = null;
            int guard = 0;
            while (!state.gameCompleted && guard < 2000)
            {
                bool boss = state.levelInWorld % 10 == 0;
                result = state.RecordLevelClear(boss);
                guard++;
            }
            Assert.IsTrue(state.gameCompleted);
            Assert.IsTrue(result.worldCompleted);
            Assert.AreEqual(2, state.worldIndex);
            Assert.AreEqual(10, state.UnlockedInWorld(2));
        }

        [Test]
        public void DisplayName_CapitalizesFirstLetter()
        {
            Assert.AreEqual("Dog", Worlds.DisplayName("dog"));
            Assert.AreEqual("Clownfish", Worlds.DisplayName("clownfish"));
        }
    }
}