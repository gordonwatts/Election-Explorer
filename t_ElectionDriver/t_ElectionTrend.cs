using System;
using ElectionDriver;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Linq;
using System.Threading.Tasks;

namespace t_ElectionDriver
{
    [TestClass]
    public class t_ElectionTrend
    {
        [TestMethod]
        public void TestCTor()
        {
            var e = new Election();
            var et = new ElectionTrend(e);
        }

        [TestMethod]
        public async Task TestSimpleTrend()
        {
            // Simple, constant election
            var e = new Election() { NumberOfCandidates = 2, NumberOfPeople = 10 };
            var step = new ElectionDriver.Fakes.StubIElectionStep();
            step.RunStepPersonArrayCandiateRankingArrayArray = (people, prev) =>
            {
                var mypeople = people.Where(p => p.NumberOfCandidates == 2);
                return new CandiateRanking[] { 
                    new CandiateRanking(0, mypeople.Where(p => p.Ranking(0) == 1).Count()),
                    new CandiateRanking(1, mypeople.Where(p => p.Ranking(1) == 1).Count())
                };
            };
            e.AddStep(step);

            var et = new ElectionTrend(e);
            var r = et.RunTrend(
                (point, totPoint) => Tuple.Create<double, Func<Person, bool>>(0.1*point, p => p.Ranking(0) == 1),
                points: 10
                );

            Assert.AreEqual(10, r.Length, "# of ensemble results that came back");
            for (int i = 0; i < 10; i++)
            {
                var real = await r[i];
                var frac = 0.1 * i;

                Console.WriteLine("Election Point {0}: ", i);

                for (int icand = 0; icand < 2; icand++)
                {
                    Console.Write("  Candidate {0}: ", icand);
                    Console.WriteLine("r1={0}, r2={1}", real.candidateResults[icand].resultTimes[0],
                        real.candidateResults[icand].resultTimes[1]);

                    if (i < 5)
                    {
                        Assert.AreEqual(0, real.candidateResults[0].resultTimes[0], "Expected for iteration " + i + "!");
                    }
                    else
                    {
                        Assert.AreEqual(50, real.candidateResults[0].resultTimes[0], "Expected for iteration " + i + "!");
                    }
                }

            }
        }
    }
}
