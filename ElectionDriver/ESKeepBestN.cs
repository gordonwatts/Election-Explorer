using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ElectionDriver
{
    /// <summary>
    /// Election step to keep the top n candidates. If there are less, keep them all.
    /// </summary>
    public class ESKeepBestN : IElectionStep
    {
        /// <summary>
        /// Init with the number of candidates to keep
        /// </summary>
        /// <param name="candidatesToKeep"></param>
        public ESKeepBestN(uint candidatesToKeep)
        {
            if (candidatesToKeep == 0)
                throw new ArgumentException("Can't create a Keep Best N election step where we keep zero candidates!");
            CandidatesToKeep = candidatesToKeep;
        }

        /// <summary>
        /// Run the election
        /// </summary>
        /// <param name="people"></param>
        /// <param name="previousResults"></param>
        /// <returns></returns>
        public CandiateRanking[] RunStep(Person[] people, CandiateRanking[][] previousResults)
        {
            if (previousResults == null || previousResults.Length == 0)
                throw new InvalidOperationException("Can't run a Keep Best N candidates election step as first step in an election!");

            return (from r in previousResults.Last()
                    orderby r.ranking descending
                    select r).Take((int)CandidatesToKeep).ToArray();
        }

        /// <summary>
        /// Get the # of candidates to keep
        /// </summary>
        public uint CandidatesToKeep { get; private set; }
    }
}
