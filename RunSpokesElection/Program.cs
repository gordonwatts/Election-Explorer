using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ElectionDriver;

namespace RunSpokesElection
{
    class Program
    {
        /// <summary>
        /// Try to emulate some fairly complex election algorithms that I've seen around
        /// the web.
        /// </summary>
        /// <param name="args"></param>
        static void Main(string[] args)
        {
            // Basic config of the election

            int nPeople = 400;
            int nElections = 1000;
            int nCandidates = 4;

            // At each step each person votes for one person, presumably the one they like best.
            // So we reweight each vote to be 1 or 0, depending if it is their first candidate or not.

            Func<IEnumerable<CandiateRanking>, IEnumerable<CandiateRanking>> pScore = candidateList =>
            {
                var bestCandidate = (from t in candidateList
                                     orderby t.ranking ascending
                                     select t.candidate).First();

                return candidateList.Select(candidate => new CandiateRanking(candidate.candidate, candidate.candidate == bestCandidate ? 1 : 0));
            };

            // The election is done by who has the most votes - so we just sum up all the weights, and return them
            // the final one with "ordering" of some sort. The weight will be the larest is the winner.

            Func<IEnumerable<IEnumerable<CandiateRanking>>, IEnumerable<CandiateRanking>> eScore = votingRecord =>
            {
                var candidateWeights = from voteScore in votingRecord
                                       from candidateWeight in voteScore
                                       group candidateWeight.ranking by candidateWeight.candidate into weightsByCandidate
                                       select new CandiateRanking(weightsByCandidate.Key, weightsByCandidate.Sum());

                return candidateWeights;
            };

            // A bunch of people

            int flips = 0;

            for (int i = 0; i < nElections; i++)
            {
                Console.WriteLine();
                int localFlips = RunElection(nPeople, nCandidates, pScore, eScore);
                if (localFlips != 0)
                    flips += 1;
            }

            Console.WriteLine("Number of flips was {0} in {1} elections.", flips, nElections);

        }

        private static int RunElection(int nPeople, int nCandidates, Func<IEnumerable<CandiateRanking>, IEnumerable<CandiateRanking>> pScore, Func<IEnumerable<IEnumerable<CandiateRanking>>, IEnumerable<CandiateRanking>> eScore)
        {
            var r = new Random();
            var people = (from idx in Enumerable.Range(0, nPeople)
                          select new Person(nCandidates, r)).ToArray();

            // The election

            var winner = RunElection(people, pScore, eScore, printResult: true);

            // The result

            int flips = 0;
            for (int i = 0; i < nCandidates; i++)
            {
                if (i != winner)
                {
                    var newwinner = RunElection(people, pScore, eScore, new int[] { i });
                    if (newwinner != winner)
                        flips++;
                }
            }

            return flips;
        }

        /// <summary>
        /// Runs one election
        /// </summary>
        /// <param name="people"></param>
        /// <param name="pScore"></param>
        /// <param name="eScore"></param>
        /// <returns>Candidate who won.</returns>
        private static int RunElection(Person[] people, Func<IEnumerable<CandiateRanking>, IEnumerable<CandiateRanking>> pScore, Func<IEnumerable<IEnumerable<CandiateRanking>>, IEnumerable<CandiateRanking>> eScore, int[] candidatesToDrop = null, bool printResult = false)
        {
            // Score the people.
            var peopleScored = from p in people
                               select pScore(p.FullRanking(candidatesToDrop));

            // Now score the election
            var election = eScore(peopleScored);

            var firstRoundOrdering = (from er in election
                                      orderby er.ranking descending
                                      select er).ToArray();

            // Now, if we end up with the #1 candidate getting more than 2/3 the vote, then we are finished. Otherwise
            // we are going to have to go into run off.

            var winner = firstRoundOrdering.First();
            var outrightWinnerThreshold = 0.5 * (double)people.Length;
            if (((double)winner.ranking) <= outrightWinnerThreshold)
            {
                var peopleToDrop = new List<int>();
                if (candidatesToDrop != null)
                    peopleToDrop.AddRange(candidatesToDrop);
                foreach (var looser in firstRoundOrdering.Skip(2))
	            {
		             peopleToDrop.Add(looser.candidate);
	            }
                var ptd = peopleToDrop.ToArray();
                var peopleRescored = from p in people
                                    select pScore(p.FullRanking(ptd));

                // Now score the election
                var runoffElection = eScore(peopleRescored);

                var runoffElectionResults = (from er in runoffElection
                                            orderby er.ranking descending
                                            select er).ToArray();
                winner = runoffElectionResults.First();
            }

            var bld = new StringBuilder();
            bld.AppendFormat("Winner of the election is candidate {0} with {1} votes", winner.candidate, winner.ranking);

            if (printResult)
            {
                if (candidatesToDrop != null)
                {
                    bld.Append("(dropped candidates ");
                    bool isfirst = true;
                    foreach (var c in candidatesToDrop)
                    {
                        if (!isfirst)
                            bld.Append(", ");
                        isfirst = false;
                        bld.Append(c);
                    }
                    bld.Append(")");
                }
                bld.Append(".");

                Console.WriteLine(bld.ToString());
            }

            return winner.candidate;
        }
    }
}
