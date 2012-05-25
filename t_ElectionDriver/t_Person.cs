using System;
using System.Linq;
using ElectionDriver;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace t_ElectionDriver
{
    [TestClass]
    public class t_Person
    {
        [TestMethod]
        public void TestCreation()
        {
            var p = new Person(5, new Random());
            Assert.AreEqual(5, p.NumberOfCandidates, "# of candidates");
        }

        [TestMethod]
        public void TestForFlat()
        {
            int nCandidates = 5;
            var people = from i in Enumerable.Range(0, 1000)
                         select new Person(nCandidates, new Random());

            var rankings = from p in people
                           from candidate in Enumerable.Range(0, nCandidates)
                           select 1;
        }
    }
}
