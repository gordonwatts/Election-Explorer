using System;
using ElectionDriver;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Linq;

namespace t_ElectionDriver
{
    [TestClass]
    public class t_ESBorda
    {
        [TestMethod]
        public void Test1PersonElection()
        {
            var people = new Person[] {
                new Person(2, 1, 0)
            };

            var es = new ESBorda();
            var result = es.RunStep(people, null).OrderBy(g => g.ranking).ToArray();

            Assert.AreEqual(3, result.Length, "Result length");
            var ranking1 = result[0];
            var ranking2 = result[1];
            var ranking3 = result[2];
            Assert.AreEqual(0, ranking1.candidate, "candidate 1 index");
            Assert.AreEqual(1, ranking2.candidate, "Candidate 2 index");
            Assert.AreEqual(2, ranking3.candidate, "Candidate 3 index");
            Assert.AreEqual(0, ranking1.ranking, "candidate 1 ranking");
            Assert.AreEqual(1, ranking2.ranking, "Candidate 2 ranking");
            Assert.AreEqual(2, ranking3.ranking, "Candidate 3 ranking");
        }

        [TestMethod]
        public void Test2PersonElection()
        {
            var people = new Person[] {
                new Person(2, 1, 0), new Person(2, 1, 0)
            };

            var es = new ESBorda();
            var result = es.RunStep(people, null).OrderBy(g => g.ranking).ToArray();

            Assert.AreEqual(3, result.Length, "Result length");
            var ranking1 = result[0];
            var ranking2 = result[1];
            var ranking3 = result[2];
            Assert.AreEqual(0, ranking1.candidate, "candidate 1 index");
            Assert.AreEqual(1, ranking2.candidate, "Candidate 2 index");
            Assert.AreEqual(2, ranking3.candidate, "Candidate 3 index");
            Assert.AreEqual(0, ranking1.ranking, "candidate 1 ranking");
            Assert.AreEqual(2, ranking2.ranking, "Candidate 2 ranking");
            Assert.AreEqual(4, ranking3.ranking, "Candidate 3 ranking");
        }

        [TestMethod]
        public void Test3PersonElection()
        {
            var people = new Person[] {
                new Person(2, 1, 0), new Person(1, 0, 2), new Person(0, 1, 2)
            };

            var es = new ESBorda();
            var result = es.RunStep(people, null).OrderBy(g => g.ranking).ToArray();

            Assert.AreEqual(3, result.Length, "Result length");
            var ranking1 = result[0];

            var ranking2 = result[1];
            var ranking3 = result[2];
            Assert.AreEqual(1, ranking3.candidate, "candidate 1 index");
            Assert.AreEqual(4, ranking3.ranking, "candidate 1 ranking");
        }
    }
}
