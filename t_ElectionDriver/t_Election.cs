using System;
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

        /// <summary>
        /// Simple election tester.
        /// </summary>
        class ESTester : IElectionStep
        {
            public ESTester()
            {
                Result = null;
            }

            public int NumberPeople { get; set; }
            public CandiateRanking[] Result { get; set; }
            public CandiateRanking[] RunStep(Person[] people)
            {
                NumberPeople = people.Length;
                return Result;
            }
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void TestSimpleStepNullFail()
        {
            var step = new ElectionDriver.Fakes.StubIElectionStep();
            step.RunStepPersonArray = people =>
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
        public void TestSimpleStepRuturnNoCandidates()
        {
            var step = new ElectionDriver.Fakes.StubIElectionStep();
            CandiateRanking[] rankings = new CandiateRanking[0];
            step.RunStepPersonArray = people =>
            {
                return rankings;
            };

            var e = new Election();
            e.NumberOfCandidates = 15;
            e.NumberOfPeople = 350;
            e.AddStep(step);
            var result = e.RunSingleElection();
            Assert.IsNotNull(result, "Result return should not be null");
            Assert.AreEqual(0, result.Length, "# of candidates that came back");
        }

        [TestMethod]
        public void TestSimpleReturn()
        {
            var step = new ElectionDriver.Fakes.StubIElectionStep();
            int numPeople = 0;
            int numCandidates = 0;
            CandiateRanking[] r = new CandiateRanking[] { new CandiateRanking(0, 1) };
            step.RunStepPersonArray = people => {
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
            step1.RunStepPersonArray = people => ranking1;

            var step2 = new ElectionDriver.Fakes.StubIElectionStep();
            CandiateRanking[] ranking2 = new CandiateRanking[] { new CandiateRanking(0, 1), new CandiateRanking(1, 1) };
            step2.RunStepPersonArray = people => ranking2;

            var e = new Election();
            e.AddStep(step1);
            e.AddStep(step2);
            var result = e.RunSingleElection();

            Assert.AreEqual(ranking1, result, "Candidate ranking should be what came out of step 1");
        }
    }
}
