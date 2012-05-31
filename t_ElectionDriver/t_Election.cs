﻿using System;
using ElectionDriver;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.QualityTools.Testing.Fakes;
using System.Linq;

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

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void TestSimpleStepNullFail()
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
            var result = e.RunSingleElection();
        }

        [TestMethod]
        [ExpectedException(typeof(ElectionFailureException))]
        public void TestSimpleStepRuturnNoCandidates()
        {
            var step = new ElectionDriver.Fakes.StubIElectionStep();
            CandiateRanking[] rankings = new CandiateRanking[0];
            step.RunStepPersonArrayCandiateRankingArrayArray = (people, prev) =>
            {
                return rankings;
            };

            var e = new Election();
            e.AddStep(step);
            var result = e.RunSingleElection();
        }

        [TestMethod]
        public void TestSimpleReturn()
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
            var result = e.RunSingleElection();

            Assert.AreEqual(350, numPeople, "# of people");
            Assert.AreEqual(15, numCandidates, "# of candidates");

            Assert.AreEqual(r, result, "Candidate ranking that came back isn't right");

        }

        [TestMethod]
        public void TestTwoStepElectionWithFirstAWinner()
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
            var result = e.RunSingleElection();

            Assert.AreEqual(ranking1, result, "Candidate ranking should be what came out of step 1");
        }

        [TestMethod]
        public void TestTwoStepElectionSimple()
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
            var result = e.RunSingleElection();

            Assert.AreEqual(ranking2, result, "Candidate ranking should be what came out of step 1");
        }

        [TestMethod]
        public void TestElectionWindowing()
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
            var result = e.RunSingleElection();

            Assert.AreEqual(ranking3, result, "Candidate ranking should be what came out of step 1");
        }

        [TestMethod]
        public void RunSimpleElectionSetWithFlip()
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

            var flips = e.RunElection();
            Assert.AreEqual(1, flips, "Expected # of flips");
        }

        [TestMethod]
        public void RunSimpleElectionSetWithNoFlip()
        {
            var e = new Election() { NumberOfCandidates = 2, NumberOfPeople = 2 };

            var step1 = new ElectionDriver.Fakes.StubIElectionStep();
            step1.RunStepPersonArrayCandiateRankingArrayArray = (people, prev) =>
            {
                return new CandiateRanking[] { new CandiateRanking(0, 1) };
            };
            e.AddStep(step1);

            var flips = e.RunElection();
            Assert.AreEqual(0, flips, "Expected # of flips");
        }
    }
}
