using System;
using ElectionDriver;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace t_ElectionDriver
{
    [TestClass]
    public class t_ESKeepThoseBetterThan
    {
        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void TestNotFirstStep()
        {
            var people = new Person[] { new Person(0, 1, 2) };
            var old = new CandiateRanking[][] { };

            var s = new ESKeepThoseBetterThan(0.5);
            s.RunStep(people, old);
        }

        [TestMethod]
        public void TestSimpleRun()
        {
            var people = new Person[] {
                new Person(0, 1, 2),
                new Person(0, 1, 2),
                new Person(0, 1, 2),
                new Person(0, 1, 2),
            };
            var old = new CandiateRanking[][] { new CandiateRanking[] { new CandiateRanking(0, 3), new CandiateRanking(1, 1) } };

            var s = new ESKeepThoseBetterThan(0.7);
            var r = s.RunStep(people, old);
            Assert.AreEqual(1, r.Length, "# of candidates ranked");
            Assert.AreEqual(0, r[0].candidate, "Incorrect candidate came back");
        }

        [TestMethod]
        public void TestSimpleRunNoOneDefault()
        {
            var people = new Person[] {
                new Person(0, 1, 2),
                new Person(0, 1, 2),
                new Person(0, 1, 2),
                new Person(0, 1, 2),
            };
            var old = new CandiateRanking[][] { new CandiateRanking[] { new CandiateRanking(0, 2), new CandiateRanking(1, 2) } };

            var s = new ESKeepThoseBetterThan(0.7);
            var r = s.RunStep(people, old);
            Assert.AreEqual(2, r.Length, "# of candidates ranked");
            Assert.AreEqual(0, r[0].candidate, "Incorrect candidate came back");
            Assert.AreEqual(1, r[1].candidate, "Incorrect candidate came back");
        }

        [TestMethod]
        public void TestSimpleRunNoOneFailWhen()
        {
            var people = new Person[] {
                new Person(0, 1, 2),
                new Person(0, 1, 2),
                new Person(0, 1, 2),
                new Person(0, 1, 2),
            };
            var old = new CandiateRanking[][] { new CandiateRanking[] { new CandiateRanking(0, 2), new CandiateRanking(1, 2) } };

            var s = new ESKeepThoseBetterThan(0.7) { DoNothingIfNoOnePasses=false };
            var r = s.RunStep(people, old);
            Assert.AreEqual(0, r.Length, "# of candidates ranked");
        }
    }
}
