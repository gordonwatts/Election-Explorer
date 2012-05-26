using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ElectionDriver
{
    /// <summary>
    /// Very simple candidate ranking - this is exactly like a Tuple, so that
    /// Item1 and Item2 have names.
    /// </summary>
    public struct CandiateRanking
    {
        /// <summary>
        /// Create a candidate ranking
        /// </summary>
        /// <param name="c"></param>
        /// <param name="r"></param>
        public CandiateRanking(int c, int r)
        {
            candidate = c;
            ranking = r;
        }

        /// <summary>
        /// The candidate that is being ranked.
        /// </summary>
        public int candidate;

        /// <summary>
        /// The ranking of the candidate.
        /// </summary>
        public int ranking;
    }
}
