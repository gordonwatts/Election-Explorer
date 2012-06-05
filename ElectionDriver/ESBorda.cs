using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ElectionDriver
{
    /// <summary>
    /// Implement Borda's election scheme. Weight for each person, with the leader getting the most points,
    /// but points for everyone in the list.
    /// </summary>
    public class ESBorda : IElectionStep
    {
        /// <summary>
        /// Run the election itself.
        /// </summary>
        /// <param name="people"></param>
        /// <param name="previousResults"></param>
        /// <returns></returns>
        public CandiateRanking[] RunStep(Person[] people, CandiateRanking[][] previousResults)
        {
            var summedWeights = from p in people
                                from ranking in p.FullRanking()
                                group ranking.ranking by ranking.candidate into candidateRankings
                                select new CandiateRanking()
                                {
                                    candidate = candidateRankings.Key,
                                    ranking = candidateRankings.Sum()
                                };
            return summedWeights.ToArray();
        }
    }
}
