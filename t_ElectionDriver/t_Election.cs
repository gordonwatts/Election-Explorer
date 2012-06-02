using System;
using ElectionDriver;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.QualityTools.Testing.Fakes;
using System.Linq;
using System.Threading.Tasks;

namespace t_ElectionDriver
{
    [TestClass]
    public class t_Election
    {
        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public async Task TestBlankRun()
        {
            var e = new Election();
            await e.RunSingleElection();
        }

        [TestMethod]
        public void TestSimpleRun()
        {
            var e = new Election();
            e.AddStep(new ESOnlyBestCounts());
            var r = e.RunSingleElection();
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public async Task TestSimpleStepNullFail()
        {
            var step = new ElectionDriver.Fakes.StubIElectionStep();
            step.RunStepPersonArrayCandiateRankingArrayArray = (people, prev) =>
            {
                return null;
            };

            var e = new Election();
            e.NumberOfCandidates = 15;
            e.NumberOfPeople = 350;
            e.AddStep(step);
            await e.RunSingleElection();
        }

        [TestMethod]
        [ExpectedException(typeof(ElectionFailureException))]
        public async Task TestSimpleStepRuturnNoCandidates()
        {
            var step = new ElectionDriver.Fakes.StubIElectionStep();
            CandiateRanking[] rankings = new CandiateRanking[0];
            step.RunStepPersonArrayCandiateRankingArrayArray = (people, prev) =>
            {
                return rankings;
            };

            var e = new Election();
            e.AddStep(step);
            var result = await e.RunSingleElection();
        }

        [TestMethod]
        public async void TestSimpleReturn()
        {
            var step = new ElectionDriver.Fakes.StubIElectionStep();
            int numPeople = 0;
            int numCandidates = 0;
            CandiateRanking[] r = new CandiateRanking[] { new CandiateRanking(0, 1) };
            step.RunStepPersonArrayCandiateRankingArrayArray = (people, prev) =>
            {
                numPeople = people.Length;
                numCandidates = people[0].FullRanking().Count();
                return r;
            };

            var e = new Election();
            e.NumberOfCandidates = 15;
            e.NumberOfPeople = 350;
            e.AddStep(step);
            var result = await e.RunSingleElection();

            Assert.AreEqual(350, numPeople, "# of people");
            Assert.AreEqual(15, numCandidates, "# of candidates");

            Assert.AreEqual(r, result, "Candidate ranking that came back isn't right");
        }

        [TestMethod]
        public async void TestElectionReturnOrder()
        {
            var step = new ElectionDriver.Fakes.StubIElectionStep();
            CandiateRanking[] r = new CandiateRanking[] { new CandiateRanking(0, 1), new CandiateRanking(1, 10) };
            step.RunStepPersonArrayCandiateRankingArrayArray = (people, prev) =>
            {
                return r;
            };

            var e = new Election();
            e.NumberOfCandidates = 15;
            e.NumberOfPeople = 350;
            e.AddStep(step);
            var result = await e.RunSingleElection();

            Assert.AreEqual(1, result[0].candidate, "Winner was not listed first");
        }

        [TestMethod]
        public async void TestElectionReturnOrder1()
        {
            var step = new ElectionDriver.Fakes.StubIElectionStep();
            CandiateRanking[] r = new CandiateRanking[] { new CandiateRanking(0, 20), new CandiateRanking(1, 10) };
            step.RunStepPersonArrayCandiateRankingArrayArray = (people, prev) =>
            {
                return r;
            };

            var e = new Election();
            e.NumberOfCandidates = 15;
            e.NumberOfPeople = 350;
            e.AddStep(step);
            var result = await e.RunSingleElection();

            Assert.AreEqual(0, result[0].candidate, "Winner was not listed first");
        }
        [TestMethod]
        public async void TestTwoStepElectionWithFirstAWinner()
        {
            var step1 = new ElectionDriver.Fakes.StubIElectionStep();
            CandiateRanking[] ranking1 = new CandiateRanking[] { new CandiateRanking(0, 1) };
            step1.RunStepPersonArrayCandiateRankingArrayArray = (people, prev) => ranking1;

            var step2 = new ElectionDriver.Fakes.StubIElectionStep();
            CandiateRanking[] ranking2 = new CandiateRanking[] { new CandiateRanking(0, 1), new CandiateRanking(1, 1) };
            step2.RunStepPersonArrayCandiateRankingArrayArray = (people, prev) => ranking2;

            var e = new Election();
            e.AddStep(step1);
            e.AddStep(step2);
            var result = await e.RunSingleElection();

            Assert.AreEqual(ranking1, result, "Candidate ranking should be what came out of step 1");
        }

        [TestMethod]
        public async void TestTwoStepElectionSimple()
        {
            var step1 = new ElectionDriver.Fakes.StubIElectionStep();
            CandiateRanking[] ranking1 = new CandiateRanking[] { new CandiateRanking(0, 1), new CandiateRanking(1, 2) };
            step1.RunStepPersonArrayCandiateRankingArrayArray = (people, prev) => ranking1;

            var step2 = new ElectionDriver.Fakes.StubIElectionStep();
            CandiateRanking[] ranking2 = new CandiateRanking[] { new CandiateRanking(1, 1) };
            step2.RunStepPersonArrayCandiateRankingArrayArray = (people, prev) => ranking2;

            var e = new Election();
            e.AddStep(step1);
            e.AddStep(step2);
            var result = await e.RunSingleElection();

            Assert.AreEqual(ranking2, result, "Candidate ranking should be what came out of step 1");
        }

        [TestMethod]
        public async void TestElectionWindowing()
        {
            var step1 = new ElectionDriver.Fakes.StubIElectionStep();
            CandiateRanking[] ranking1 = new CandiateRanking[] { new CandiateRanking(0, 1), new CandiateRanking(1, 2), new CandiateRanking(2, 3) };
            step1.RunStepPersonArrayCandiateRankingArrayArray = (people, prev) =>
                {
                    Assert.IsTrue(people.All(p => p.FullRanking().Count() == 4), "Not always three candidates");
                    return ranking1;
                };

            var step2 = new ElectionDriver.Fakes.StubIElectionStep();
            CandiateRanking[] ranking2 = new CandiateRanking[] { new CandiateRanking(1, 1), new CandiateRanking(0, 1) };
            step2.RunStepPersonArrayCandiateRankingArrayArray = (people, prev) => 
                {
                    Assert.IsTrue(people.All(p => p.FullRanking().Count() == 3), "Not always two candidates");
                    return ranking2;
                };

            var step3 = new ElectionDriver.Fakes.StubIElectionStep();
            CandiateRanking[] ranking3 = new CandiateRanking[] { new CandiateRanking(1, 1)};
            step3.RunStepPersonArrayCandiateRankingArrayArray = (people, prev) =>
            {
                Assert.IsTrue(people.All(p => p.FullRanking().Count() == 2), "Not always two candidates");
                return ranking3;
            };

            var e = new Election();
            e.NumberOfCandidates = 4;
            e.AddStep(step1);
            e.AddStep(step2);
            e.AddStep(step3);
            var result = await e.RunSingleElection();

            Assert.AreEqual(ranking3, result, "Candidate ranking should be what came out of step 1");
        }

        [TestMethod]
        public async void RunSimpleElectionSetWithFlip()
        {
            var e = new Election() { NumberOfCandidates = 2, NumberOfPeople = 2 };

            var step1 = new ElectionDriver.Fakes.StubIElectionStep();
            bool firstcall = true;
            step1.RunStepPersonArrayCandiateRankingArrayArray = (people, prev) =>
                {
                    if (firstcall)
                    {
                        firstcall = false;
                        return new CandiateRanking[] { new CandiateRanking(0, 1) };
                    }
                    else
                    {
                        var p1 = people.First();
                        Assert.AreEqual(1, p1.NumberOfCandidates, "# of candidates on second sub-election");
                        Assert.AreEqual(0, p1.FullRanking().First().candidate, "Kept candidate");
                        return new CandiateRanking[] { new CandiateRanking(1, 1) };
                    }
                };
            e.AddStep(step1);

            var flips = await e.RunElection();
            Assert.AreEqual(1, flips.flips, "Expected # of flips");
            Assert.AreEqual(1, flips.candidateOrdering.Length, "# of rnaking candidates");
            Assert.AreEqual(0, flips.candidateOrdering[0], "Candidate order 0");
        }

        [TestMethod]
        public async Task RunElection20Times()
        {
            var e = new Election() { NumberOfCandidates = 2, NumberOfPeople = 2 };

            var step1 = new ElectionDriver.Fakes.StubIElectionStep();
            int counterTimesCalledWith2 = 0;
            int counterTimesCalledWith1 = 0;
            step1.RunStepPersonArrayCandiateRankingArrayArray = (people, prev) =>
            {
                if (people[0].NumberOfCandidates == 1) {
                    counterTimesCalledWith1++;
                    return new CandiateRanking[] { new CandiateRanking(1, 1) };
                }
                if (people[0].NumberOfCandidates == 2) {
                    counterTimesCalledWith2++;
                    return new CandiateRanking[] { new CandiateRanking(0, 1) };
                }
                Assert.Fail("How do we deal with more than 2 candidates!?");
                return null;
            };
            e.AddStep(step1);

            var result = await e.RunElectionEnsemble(20);
            Assert.AreEqual(20, result.flips, "Expected # of flips");
            Assert.AreEqual(20, counterTimesCalledWith2, "Times called with 2");
            Assert.AreEqual(20, counterTimesCalledWith1, "Times called with 1");
            Assert.AreEqual(2, result.candidateResults.Length, "# of different winners");
            Assert.AreEqual(20, result.candidateResults[0].resultTimes[0], "# of times candidate zero won");
            Assert.AreEqual(0, result.candidateResults[0].resultTimes[1], "# of times candidate zero was second");
            Assert.AreEqual(0, result.candidateResults[1].resultTimes[0], "# of times candidate one was first");
            Assert.AreEqual(0, result.candidateResults[1].resultTimes[1], "# of times candidate one was second");
        }

        [TestMethod]
        public async Task RunElection20TimesWithDifferentResults()
        {
            var e = new Election() { NumberOfCandidates = 3, NumberOfPeople = 20 };

            var step1 = new ElectionDriver.Fakes.StubIElectionStep();
            step1.RunStepPersonArrayCandiateRankingArrayArray = (people, prev) =>
            {
                return new CandiateRanking[] { new CandiateRanking(0, 10), new CandiateRanking(1, 15) };
            };
            e.AddStep(step1);

            var result = await e.RunElectionEnsemble(20);
            Assert.AreEqual(0, result.flips, "Expected # of flips");
            Assert.AreEqual(3, result.candidateResults.Length, "# of different winners");
            Assert.AreEqual(0, result.candidateResults[0].resultTimes[0], "# of times candidate zero won");
            Assert.AreEqual(20, result.candidateResults[0].resultTimes[1], "# of times candidate zero was second");
            Assert.AreEqual(0, result.candidateResults[0].resultTimes[2], "# of times candidate zero was second");
            Assert.AreEqual(20, result.candidateResults[1].resultTimes[0], "# of times candidate one was first");
            Assert.AreEqual(0, result.candidateResults[1].resultTimes[1], "# of times candidate one was second");
            Assert.AreEqual(0, result.candidateResults[1].resultTimes[2], "# of times candidate one was second");
            Assert.AreEqual(0, result.candidateResults[2].resultTimes[0], "# of times candidate one was first");
            Assert.AreEqual(0, result.candidateResults[2].resultTimes[1], "# of times candidate one was second");
            Assert.AreEqual(0, result.candidateResults[2].resultTimes[2], "# of times candidate one was second");
        }

        [TestMethod]
        public async void RunSimpleElectionSetWithNoFlip()
        {
            var e = new Election() { NumberOfCandidates = 2, NumberOfPeople = 2 };

            var step1 = new ElectionDriver.Fakes.StubIElectionStep();
            step1.RunStepPersonArrayCandiateRankingArrayArray = (people, prev) =>
            {
                return new CandiateRanking[] { new CandiateRanking(0, 1) };
            };
            e.AddStep(step1);

            var flips = await e.RunElection();
            Assert.AreEqual(0, flips.flips, "Expected # of flips");
        }
    }
}
