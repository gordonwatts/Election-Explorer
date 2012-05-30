using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ElectionDriver
{
    /// <summary>
    /// A step in an election. This is sub-classed.
    /// </summary>
    public interface IElectionStep
    {
        /// <summary>
        /// Return the rankings for the people by running this step.
        /// </summary>
        /// <param name="people">Everyone that is voting in this election step</param>
        /// <param name="previousResults">All the previous result from previous steps in the election, in order of the steps</param>
        /// <returns>Ranking from this step. If only one candidate is returned then the election is considered "done"</returns>
        /// <remarks>
        /// If this is the first step, then previousResults will be a zero length array.
        /// Otherwise [0] is the first step in the election, 1 is the next step, etc.
        /// </remarks>
        CandiateRanking[] RunStep(Person[] people, CandiateRanking[][] previousResults);
    }
}
