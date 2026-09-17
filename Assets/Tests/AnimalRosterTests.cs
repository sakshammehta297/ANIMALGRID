using NUnit.Framework;
using AnimalGrid.Core;

namespace AnimalGrid.Tests
{
    [TestFixture]
    public class AnimalRosterTests
    {
        [Test]
        public void IdsFor_ReturnsRequestedCount()
        {
            for (int n = 5; n <= 12; n++)
            {
                Assert.AreEqual(n, AnimalRoster.IdsFor(n).Count);
            }
        }

        [Test]
        public void IdsFor_CyclesThroughBaseRoster()
        {
            var ids = AnimalRoster.IdsFor(7);
            Assert.AreEqual("cat", ids[0]);
            Assert.AreEqual("panda", ids[1]);
            Assert.AreEqual("cat", ids[5]);
            Assert.AreEqual("panda", ids[6]);
        }
    }
}