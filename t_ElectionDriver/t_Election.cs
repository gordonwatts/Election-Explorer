using System;
using ElectionDriver;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace t_ElectionDriver
{
    [TestClass]
    public class t_Election
    {
        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void TestBlankRun()
        {
            var e = new Election();
            e.RunSingleElection();
        }

        [TestMethod]
        public void TestSimpleRun()
        {
            var e = new Election();
            e.AddStep(new ESOnlyBestCounts());
            var r = e.RunSingleElection();
        }
    }
}
