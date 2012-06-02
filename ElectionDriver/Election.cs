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
        /// Run an election and return the results. The results are sorted by weight.
        /// </summary>
        /// <returns>Ranking of candidates from winner on down</returns>
        public async Task<CandiateRanking[]> RunSingleElection()
        {
            // Generate the people
            var people = GeneratePeople().ToArray();

            return await RunSingleElectionInternal(people);
        }

        /// <summary>
        /// Run an election on a given set of people. Return the full candidate ordering
        /// when we are done.
        /// </summary>
        /// <param name="people"></param>
        /// <returns>Weights for everyone that got votes, ordered from winner on down</returns>
        private Task<CandiateRanking[]> RunSingleElectionInternal(Person[] people)
        {
            // Quick checks.
            if (_steps.Count == 0)
                throw new InvalidOperationException("Election has no steps defined");
            if (people == null || people.Length == 0)
                throw new ArgumentException("Election can't happen without people");

            return Task<CandiateRanking[]>.Factory.StartNew(() =>
                {
                    List<CandiateRanking[]> results = new List<CandiateRanking[]>();
                    int[] keepOnly = new int[0];
                    int oldKeepCount = 0;
                    var peopleThisRound = people;
                    foreach (var s in _steps)
                    {
                        // First, window out people if we need to.
                        if (keepOnly.Length != oldKeepCount)
                        {
                            var keepThem = keepOnly.ToArray();
                            peopleThisRound = peopleThisRound.Select(p => p.KeepCandidates(keepThem)).ToArray();
                            oldKeepCount = keepOnly.Length;
                        }

                        // Run the election
                        var stepResult = s.RunStep(peopleThisRound, results.ToArray());

                        // Some simple case results that will terminate our processing of the election.
                        if (stepResult == null)
                            throw new InvalidOperationException("Election step returned a null value!");
                        if (stepResult.Length == 0)
                            throw new ElectionFailureException("Election step returned zero candidates");
                        if (stepResult.Length == 1)
                            return stepResult;

                        // Save the results for use in future steps.
                        results.Add(stepResult);

                        // And only let through the candidates that made it.
                        keepOnly = stepResult.Select(c => c.candidate).ToArray();
                    }

                    return (from fr in results.Last()
                            orderby fr.ranking descending
                            select fr).ToArray();
                });
        }

        /// <summary>
        /// Run an election with a set of randomly generated poeple. Once they are run,
        /// remove each non-winning candidate in turn and count the number of times
        /// that the winner changes.
        /// </summary>
        /// <returns></returns>
        public async Task<int> RunElection()
        {
            // Generate the people.
            var people = GeneratePeople().ToArray();

            // Next, run the election
            var result = await RunSingleElectionInternal(people);

            // Now, loop over each candidate and remove them... unless they are the winner!
            var winner = result[0].candidate;
            int flips = 0;
            for (int i_cand = 0; i_cand < NumberOfCandidates; i_cand++)
            {
                if (i_cand != winner)
                {
                    var peopleWithOut = people.Select(p => p.RemoveCandidates(i_cand)).ToArray();
                    var resultWithOut = await RunSingleElectionInternal(peopleWithOut);
                    if (resultWithOut[0].candidate != winner)
                        flips++;
                }
            }

            return flips;
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

        public class ElectionResults
        {
            /// <summary>
            /// Number of times the winner (first place candidate) changed when we varied the
            /// people who were in the election.
            /// </summary>
            public int flips;

            /// <summary>
            /// The order of the candidates - the first one is the first candidate, etc. Gives the
            /// ranking of the candidates.
            /// </summary>
            public int[] candidateOrdering;
        }

        /// <summary>
        /// Repeatedly run the election, and count the number of elections that
        /// contain at least one flip.
        /// </summary>
        /// <param name="p"></param>
        /// <returns></returns>
        public async Task<ElectionResults> RunElectionEnsemble(uint numberOfElections)
        {
            if (numberOfElections == 0)
                throw new ArgumentException("Asked to run an ensemble of zero elections!");

            var allResults = from i_election in Enumerable.Range(0, (int)numberOfElections)
                             select RunElection();

            var flipsPerResult = await Task.WhenAll(allResults.ToArray());

            var r = new ElectionResults();
            r.flips = flipsPerResult.Select(c => c > 0 ? 1 : 0).Sum();
            return r;
        }
    }
}
