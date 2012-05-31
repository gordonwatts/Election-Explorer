using System;
using ElectionDriver;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace t_ElectionDriver
{
    /// <summary>
    /// Test the keep best of guy
    /// </summary>
    [TestClass]
    public class t_ESKeepBestN
    {
        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void TestKeepZero()
        {
            var es = new ESKeepBestN(0);
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void TestKeepFirstInChain()
        {
            var es = new ESKeepBestN(10);
            es.RunStep(new Person[] { new Person(0, 1, 2, 3) }, null);
        }

        [TestMethod]
        public void TestKeep2of5()
        {
            var cr = new CandiateRanking[][] { new CandiateRanking[] {
                new CandiateRanking(0, 10),
                new CandiateRanking(1, 9),
                new CandiateRanking(2, 8),
                new CandiateRanking(3, 7),
                new CandiateRanking(4, 6)
            }};
            var es = new ESKeepBestN(2);
            var r = es.RunStep(new Person[] { new Person(0, 1, 2, 3) }, cr);
            Assert.AreEqual(2, r.Length, "# of kept candidates");
            Assert.AreEqual(0, r[0].candidate, "First candidate");
            Assert.AreEqual(1, r[1].candidate, "Second candidate");
        }

        [TestMethod]
        public void TestKeep2of5OutOfOrder()
        {
            var cr = new CandiateRanking[][] { new CandiateRanking[] {
                new CandiateRanking(0, 6),
                new CandiateRanking(1, 7),
                new CandiateRanking(2, 8),
                new CandiateRanking(3, 9),
                new CandiateRanking(4, 10)
            }};
            var es = new ESKeepBestN(2);
            var r = es.RunStep(new Person[] { new Person(0, 1, 2, 3) }, cr);
            Assert.AreEqual(2, r.Length, "# of kept candidates");
            Assert.AreEqual(4, r[0].candidate, "First candidate");
            Assert.AreEqual(3, r[1].candidate, "Second candidate");
        }

        [TestMethod]
        public void TestKeep10of5()
        {
            var cr = new CandiateRanking[][] { new CandiateRanking[] {
                new CandiateRanking(0, 10),
                new CandiateRanking(1, 9),
                new CandiateRanking(2, 8),
                new CandiateRanking(3, 7),
                new CandiateRanking(4, 6)
            }};
            var es = new ESKeepBestN(10);
            var r = es.RunStep(new Person[] { new Person(0, 1, 2, 3) }, cr);
            Assert.AreEqual(5, r.Length, "# of kept candidates");
        }
    }
}
