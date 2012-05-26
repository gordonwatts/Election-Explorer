using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ElectionDriver;

namespace RunMajorityElection
{
    class Program
    {
        /// <summary>
        /// Run a very simple majority election
        /// </summary>
        /// <param name="args"></param>
        static void Main(string[] args)
        {
            int nPeople = 2000;
            int nCandidates = 3;

            // A bunch of people

            var r = new Random();
            var people = (from idx in Enumerable.Range(0, nPeople)
                         select new Person(nCandidates, r)).ToArray();

            // The scoring system for simple majority is just who is out front.
            // The score function just returns a list of numbers for each person.

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

            // Score the people.
            var peopleScored = from p in people
                               select pScore(p.FullRanking());

            // Now score the election
            var election = eScore(peopleScored);

            var winner = (from er in election
                          orderby er.ranking descending
                          select er).First();
            Console.WriteLine("Winner of the election is candidate {0} with {1} votes.", winner.candidate, winner.ranking);

        }
    }
}
