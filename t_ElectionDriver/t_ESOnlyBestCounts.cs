using System;
using ElectionDriver;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Linq;

namespace t_ElectionDriver
{
    [TestClass]
    public class t_ESOnlyBestCounts
    {
        [TestMethod]
        public void TestSimpleElection()
        {
            var p1 = new Person(0, 1, 2);
            var p2 = new Person(0, 2, 1);

            var r = RunStep(p1, p2);

            Assert.AreEqual(1, r.Length, "# of rankings that came back");
            Assert.AreEqual(2, r.Where(c => c.candidate == 0).First().ranking, "# of votes for candidate 0");
        }

        [TestMethod]
        public void TestSimpleElection2()
        {
            var p1 = new Person(0, 1, 2);
            var p2 = new Person(0, 2, 1);
            var p3 = new Person(1, 0, 2);

            var r = RunStep(p1, p2, p3);

            Assert.AreEqual(2, r.Length, "# of rankings that came back");
            Assert.AreEqual(2, r.Where(c => c.candidate == 0).First().ranking, "# of votes for candidate 0");
            Assert.AreEqual(1, r.Where(c => c.candidate == 1).First().ranking, "# of votes for candidate 1");
        }

        /// <summary>
        /// Helper to run the step
        /// </summary>
        /// <param name="people"></param>
        /// <returns></returns>
        private CandiateRanking[] RunStep(params Person[] people)
        {
            var s = new ESOnlyBestCounts();
            return s.RunStep(people);
        }
    }
}
