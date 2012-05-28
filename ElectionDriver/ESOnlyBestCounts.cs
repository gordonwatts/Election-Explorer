using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ElectionDriver
{
    /// <summary>
    /// Run an election where you rank by only the person's top choice.
    /// </summary>
    public class ESOnlyBestCounts : IElectionStep
    {
        /// <summary>
        /// Calculate weights for all candidates by the #1 choice out there.
        /// </summary>
        /// <param name="people"></param>
        /// <returns></returns>
        public CandiateRanking[] RunStep(Person[] people)
        {
            // Weights only work for the best candidate.
            var bestvote = from p in people
                             let ranking = p.FullRanking()
                             let bestRanking = from r in ranking
                                               orderby r.ranking descending
                                               select r
                             select bestRanking.First().candidate;

            // Group them by their best vote and tally them up!
            return (from bc in bestvote
                   group bc by bc into candidatecounts
                   select new CandiateRanking (candidatecounts.Key, candidatecounts.Count())).ToArray();
        }
    }
}
