using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ElectionDriver
{
    /// <summary>
    /// Vary something about the People population and see what the trend is that comes
    /// out the other side.
    /// </summary>
    public class ElectionTrend
    {
        private Election _election;

        public ElectionTrend(Election e)
        {
            // TODO: Complete member initialization
            this._election = e;
        }

        /// <summary>
        /// Run an election ensemble for some number of points. Call the constraint generator
        /// each time to generate a new constraint for the election.
        /// </summary>
        /// <param name="constraintGenerator"></param>
        /// <param name="numberPerPoint"></param>
        /// <param name="points"></param>
        public Task<Election.ElectionEnsembleResults>[] RunTrend(Func<int, int, Tuple<double, Func<Person, bool>>> constraintGenerator,
            int numberPerPoint = 50, int points = 10)
        {
            var ec = from iP in Enumerable.Range(0, points)
                     let constraintInfo = constraintGenerator(iP, points)
                     select _election.ResetAndAdd(constraintInfo.Item1, constraintInfo.Item2).RunElectionEnsemble((uint)numberPerPoint);

            return ec.ToArray();
        }
    }

    internal static class ElectionTrendUtils
    {
        /// <summary>
        /// Helper class to make things more "functional". :-)
        /// </summary>
        /// <param name="e"></param>
        /// <param name="frac"></param>
        /// <param name="constraint"></param>
        /// <returns></returns>
        public static Election ResetAndAdd(this Election e, double frac, Func<Person, bool> constraint)
        {
            e.ClearPeopleConstrains();
            e.AddPeopleConstraint(frac, constraint);
            return e;
        }
    }
}
