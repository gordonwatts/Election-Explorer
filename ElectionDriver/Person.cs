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
        /// _candidateOrdering[0] contains the ranking for candidate 0, 1 for candidate 1, etc.
        /// The lowest number indicates the lowest rnaking. So if _candidateOrdering[0] is 0, then
        /// that is the last favorite candidate of this person.
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

            var ordr = from possible in ordering
                                 orderby possible.Prob descending
                                 select possible.Index;

            int index = 0;
            _candidateOrdering = new int[candidates];
            foreach (var co in ordr)
            {
                _candidateOrdering[co] = index;
                index++;
            }
        }

        /// <summary>
        /// Create a person, with the list of candidates in order from most favorite to least.
        /// </summary>
        /// <param name="candidatesInOrder">List of candidates, from most favorite to least</param>
        /// <remarks>Every single candidate must be specified! This is mostly used for testing.</remarks>
        public Person(params int[] candidatesInOrder)
        {
            if (candidatesInOrder == null)
                throw new ArgumentNullException("Can't have a person who knows about zero candidates");

            _candidateOrdering = new int[candidatesInOrder.Length];
            var hit = new bool[candidatesInOrder.Length];

            int ranking = candidatesInOrder.Length-1;
            foreach (var c in candidatesInOrder)
            {
                if (c < 0 || c >= candidatesInOrder.Length)
                    throw new ArgumentException(string.Format("Candidate '{0}' is longer than the implied length", c));
                if (hit[c])
                    throw new ArgumentException(string.Format("Candidate '{0}' was specified twice in the ordering to the Person ctor.", c));
                hit[c] = true;
                _candidateOrdering[c] = ranking;
                ranking--;
            }
        }

        /// <summary>
        /// Get the # of candidates in this person.
        /// </summary>
        public int NumberOfCandidates
        {
            get { return _candidateOrdering.Length; }
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
        /// Returns a new person, with the candidates given removed.
        /// </summary>
        /// <param name="candidates"></param>
        /// <returns></returns>
        public Person RemoveCandidates(params int[] candidates)
        {
            if (candidates == null || candidates.Length == 0)
                return this;
            return this;
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

            var list = (from i in Enumerable.Range(0, _candidateOrdering.Length)
                   where !holdBack.Contains(i)
                   orderby _candidateOrdering[i] descending
                   select new CandiateRanking(i, _candidateOrdering[i])).ToArray();

            for (int i = 0; i < list.Length; i++)
            {
                list[i].ranking = i;
            }

            return list;

        }
    }
}
