﻿using System;
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
    }
}
