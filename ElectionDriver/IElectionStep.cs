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
        /// <param name="people"></param>
        /// <returns></returns>
        CandiateRanking[] RunStep(Person[] people);
    }
}
