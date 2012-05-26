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

            Func<IEnumerable<Tuple<int, int>>, IEnumerable<Tuple<int, int>>> pScore = candidateList =>
            {
                var bestCandidate = (from t in candidateList
                                    orderby t.Item2 ascending
                                    select t.Item1).First();

                return candidateList.Select(candidate => new Tuple<int, int>(candidate.Item1, candidate.Item2 == bestCandidate ? 1 : 0));
            };

            // The election is done by who has the most votes - so we just sum up all the weights, and return them
            // the final one with "ordering" of some sort. The weight will be the larest is the winner.

            Func<IEnumerable<IEnumerable<Tuple<int, int>>>, IEnumerable<Tuple<int, int>>> eScore = votingRecord =>
                {
                    var candidateWeights = from voteScore in votingRecord
                                           from candidateWeight in voteScore
                                           group candidateWeight.Item2 by candidateWeight.Item1 into weightsByCandidate
                                           select Tuple.Create(weightsByCandidate.Key, weightsByCandidate.Sum());

                    return candidateWeights;
                };

            // Score the people.
            var peopleScored = from p in people
                               select pScore(p.FullRanking());

            // Now score the election
            var election = eScore(peopleScored);

            Console.WriteLine("Election Results:");
            foreach (var ranking in election.OrderBy(e => e.Item2))
            {
                Console.WriteLine("Candidate {0} had {1} votes.", ranking.Item1, ranking.Item2);
            }

        }
    }
}
