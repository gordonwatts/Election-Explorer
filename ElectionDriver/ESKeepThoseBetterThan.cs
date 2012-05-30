using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ElectionDriver
{
    /// <summary>
    /// A filtering election step - keep only those candidates that have
    /// more than some fraction of all the votes.
    /// </summary>
    public class ESKeepThoseBetterThan : IElectionStep
    {
        /// <summary>
        /// Create an election step that will keep only the candidates who have a minFrac
        /// of the votes.
        /// </summary>
        /// <param name="minFrac"></param>
        public ESKeepThoseBetterThan(double minFrac)
        {
            MinimumFraction = minFrac;
            DoNothingIfNoOnePasses = true;
        }

        /// <summary>
        /// Window out the list of candidates.
        /// </summary>
        /// <param name="people"></param>
        /// <param name="previousResults"></param>
        /// <returns></returns>
        public CandiateRanking[] RunStep(Person[] people, CandiateRanking[][] previousResults)
        {
            if (previousResults == null || previousResults.Length == 0)
                throw new ArgumentException("Cant run the elections step that keeps the best guys if we don't have the result of an election!");

            var lastStepResults = previousResults[previousResults.Length-1];
            var passed = (from c in lastStepResults
                          where ((double)c.ranking) / ((double)people.Length) > MinimumFraction
                          select c).ToArray();

            if (passed.Length == 0)
            {
                if (DoNothingIfNoOnePasses)
                    return lastStepResults;
                return new CandiateRanking[] { };
            }

            return passed;
        }

        /// <summary>
        /// Get Set the minimum fraction to keep.
        /// </summary>
        public double MinimumFraction { get; set; }

        /// <summary>
        /// If set to true (default) then the candidate list is returned if there is no one above
        /// the fraction.
        /// </summary>
        public bool DoNothingIfNoOnePasses { get; set; }
    }
}
