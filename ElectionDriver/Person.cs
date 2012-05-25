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
        private int[] _candidateOrdering;

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

            _candidateOrdering = (from possible in ordering
                                 orderby possible.Prob
                                 select possible.Index).ToArray();
        }

        /// <summary>
        /// Get the # of candidates in this person.
        /// </summary>
        public int NumberOfCandidates
        {
            get { return _candidateOrdering.Length; }
        }
    }
}
