using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ElectionDriver
{
    /// <summary>
    /// Told how many people in an election and a way to run it, run the election, and
    /// analyze it by dropping each of the other candidates in turn.
    /// </summary>
    public class Election
    {
        public Election()
        {
            NumberOfPeople = 400;
            NumberOfCandidates = 1000;
        }

        /// <summary>
        /// Get/Set the number of poeple that are going to vote.
        /// </summary>
        public int NumberOfPeople { get; set; }

        /// <summary>
        /// Get/Set the number of candidates that are going to run.
        /// </summary>
        public int NumberOfCandidates { get; set; }

        /// <summary>
        /// Run an election and return the results.
        /// </summary>
        public CandiateRanking[] RunSingleElection()
        {
            if (_steps.Count == 0)
                throw new InvalidOperationException("Election has no steps defined");

            // Generate the people
            var people = GeneratePeople().ToArray();

            List<CandiateRanking[]> results = new List<CandiateRanking[]>();
            foreach (var s in _steps)
            {
                var stepResult = s.RunStep(people, results.ToArray());
                if (stepResult == null)
                    throw new InvalidOperationException("Election step returned a null value!");
                if (stepResult.Length == 1)
                    return stepResult;

                results.Add(stepResult);
            }

            return results.Last();
        }

        /// <summary>
        /// Generate the people we want.
        /// </summary>
        /// <returns></returns>
        private IEnumerable<Person> GeneratePeople()
        {
            var r = new Random();
            return from i in Enumerable.Range(0, NumberOfPeople)
                   select new Person(NumberOfCandidates, r);
        }

        /// <summary>
        /// The list of steps that we go through for an election.
        /// </summary>
        List<IElectionStep> _steps = new List<IElectionStep>();

        /// <summary>
        /// Add a step to the election steps.
        /// </summary>
        /// <param name="step"></param>
        public void AddStep(IElectionStep step)
        {
            _steps.Add(step);
        }
    }
}
