using System.Collections.Generic;
using NUnit.Framework;
using AnimalGrid.Core;

namespace AnimalGrid.Tests
{
    [TestFixture]
    public class RosterDistributorTests
    {
        private List<string> Unlocked(params string[] ids)
        {
            return new List<string>(ids);
        }

        [Test]
        public void SingleUnlocked_FillsAllRegions()
        {
            var result = RosterDistributor.Distribute(Unlocked("dog"), 5, 42);
            Assert.AreEqual(5, result.Count);
            foreach (var id in result) Assert.AreEqual("dog", id);
        }

        [Test]
        public void FewerUnlockedThanRegions_EveryAnimalAppears()
        {
            var result = RosterDistributor.Distribute(Unlocked("dog", "cat"), 5, 7);
            Assert.AreEqual(5, result.Count);
            Assert.IsTrue(result.Contains("dog"));
            Assert.IsTrue(result.Contains("cat"));
        }

        [Test]
        public void MoreUnlockedThanRegions_DistinctSubset()
        {
            var unlocked = Unlocked("a", "b", "c", "d", "e", "f", "g");
            var result = RosterDistributor.Distribute(unlocked, 5, 99);
            Assert.AreEqual(5, result.Count);
            var seen = new HashSet<string>();
            foreach (var id in result)
            {
                Assert.IsTrue(unlocked.Contains(id));
                Assert.IsTrue(seen.Add(id)); // distinct
            }
        }

        [Test]
        public void Distribution_IsDeterministicPerSeed()
        {
            var unlocked = Unlocked("dog", "cat", "fox");
            var a = RosterDistributor.Distribute(unlocked, 6, 123);
            var b = RosterDistributor.Distribute(unlocked, 6, 123);
            Assert.AreEqual(a.Count, b.Count);
            for (int i = 0; i < a.Count; i++) Assert.AreEqual(a[i], b[i]);
        }

        [Test]
        public void Count_AlwaysEqualsRegions()
        {
            for (int n = 5; n <= 10; n++)
            {
                var result = RosterDistributor.Distribute(Unlocked("dog", "cat", "fox"), n, n);
                Assert.AreEqual(n, result.Count);
            }
        }
    }
}