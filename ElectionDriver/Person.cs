using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ElectionDriver
{
    /// <summary>
    /// Holds onto voting preferences for a single person
    /// </summary>
    public class Person
    {
        /// <summary>
        /// Keep track of the candidate ordering. Position i contains the ranking for candidate i.
        /// </summary>
        private IDictionary<int, int> _candidateOrdering;

        /// <summary>
        /// Create Person and stash the random voting preferences for the number
        /// of given candidates.
        /// </summary>
        /// <param name="candidates"></param>
        public Person(int candidates, Random generator)
        {
            var ordering = from i in Enumerable.Range(0, candidates)
                           select new
                           {
                               Index = i,
                               Prob = generator.Next()
                           };

            var ordr = from possible in ordering
                                 orderby possible.Prob
                                 select possible.Index;

            int index = 0;
            _candidateOrdering = new Dictionary<int, int>();
            foreach (var co in ordr)
            {
                _candidateOrdering[index] = co;
                index++;
            }
        }

        /// <summary>
        /// Get the # of candidates in this person.
        /// </summary>
        public int NumberOfCandidates
        {
            get { return _candidateOrdering.Count; }
        }

        /// <summary>
        /// Returns the ranking of a particular candidate
        /// </summary>
        /// <param name="candidate"></param>
        /// <returns></returns>
        public int Ranking(int candidate)
        {
            return _candidateOrdering[candidate];
        }

        /// <summary>
        /// Returns a full ranking by the person of the candidate list.
        /// Their number one choice has the largest weight. The weights are
        /// integer, and they are no gaps.
        /// </summary>
        /// <returns></returns>
        public IEnumerable<CandiateRanking> FullRanking(int[] candidatesToHoldBack = null)
        {
            SortedSet<int> holdBack;
            if (candidatesToHoldBack != null)
            {
                holdBack = new SortedSet<int>(candidatesToHoldBack);
            } else {
                holdBack = new SortedSet<int>();
            }

            var list = (from i in Enumerable.Range(0, _candidateOrdering.Count)
                   where !holdBack.Contains(i)
                   orderby _candidateOrdering[i] ascending
                   select new CandiateRanking(i, _candidateOrdering[i])).ToArray();

            for (int i = 0; i < list.Length; i++)
            {
                list[i].ranking = i;
            }

            return list;

        }
    }
}
