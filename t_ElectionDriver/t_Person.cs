using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using ElectionDriver;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace t_ElectionDriver
{
    [TestClass]
    public class t_Person
    {
        [TestMethod]
        public void TestCreation()
        {
            var p = new Person(5, new Random());
            Assert.AreEqual(5, p.NumberOfCandidates, "# of candidates");
        }

        [TestMethod]
        public void TestRankingOK()
        {
            int count = 10;
            var p = new Person(count, new Random());
            var seen = new SortedSet<int>();
            foreach (var candidate in Enumerable.Range(0, 10))
            {
                var r = p.Ranking(candidate);
                Assert.IsFalse(seen.Contains(r), "Duplicate ranking");
                seen.Add(r);
                Console.WriteLine("Candiate {0} has rank {1}.", candidate, r);
            }
            Assert.AreEqual(count, seen.Count, "Size of the count");
        }

        [TestMethod]
        [ExpectedException(typeof(KeyNotFoundException))]
        public void TestRankingOutOfBounds()
        {
            int count = 10;
            var p = new Person(count, new Random());
            var r = p.Ranking(count + 1);
        }

        [TestMethod]
        public void TestFullRanking()
        {
            var p = new Person(3, new Random());
            var fr = p.FullRanking().ToArray();
            CheckContiguous(fr.Select(c => c.ranking).ToArray(), 3);
            CheckContiguous(fr.Select(c => c.candidate).ToArray(), 3);
        }

        [TestMethod]
        public void TestFullRankingWithOneGone()
        {
            var p = new Person(3, new Random());
            var fr = p.FullRanking(new int[] { 1 }).ToArray();
            Assert.AreEqual(2, fr.Length, "Incorrect number came back");
            CheckContiguous(fr.Select(c => c.ranking).ToArray(), 2);
            var s = new SortedSet<int>(fr.Select(c => c.candidate));
            Assert.IsFalse(s.Contains(1), "candidate 1 should not be in there");

            var fullOrdering = (from c in p.FullRanking()
                               orderby c.ranking ascending
                               select c).ToArray();
            var partialOrdering = (from c in fr
                                   orderby c.ranking ascending
                                   select c).ToArray();

            int i_p = 0;
            for (int i_f = 0; i_f < fullOrdering.Length; i_f++)
            {
                if (fullOrdering[i_f].candidate == partialOrdering[i_p].candidate)
                {
                    i_p++;
                }
            }
            Assert.AreEqual(partialOrdering.Length, i_p, "Partial list not ordered correctly");
        }

        [TestMethod]
        public void TestFullRankingWithTwoGone()
        {
            var p = new Person(3, new Random());
            var fr = p.FullRanking(new int[] { 1, 0 }).ToArray();
            Assert.AreEqual(1, fr.Length, "Incorrect number came back");
            CheckContiguous(fr.Select(c => c.ranking).ToArray(), 1);
            var s = new SortedSet<int>(fr.Select(c => c.candidate));
            Assert.IsFalse(s.Contains(1), "candidate 1 should not be in there");
            Assert.IsFalse(s.Contains(3), "candidate 3 should not be in there");
        }

        /// <summary>
        /// Make sure this guy is contiguous.
        /// </summary>
        /// <param name="rankings"></param>
        private static void CheckContiguous(int[] rankings, int properSize)
        {
            Assert.AreEqual(properSize, rankings.Length, "Array lenght incorrect");
            var s = new SortedSet<int>(rankings);
            Assert.AreEqual(properSize, s.Count, "There were some non-unique numbers in there!");
            foreach (var i in Enumerable.Range(0, properSize))
            {
                Assert.IsTrue(s.Contains(i), string.Format("Missing element {0}.", i));
            }
        }

        [TestMethod]
        public void TestForFlat()
        {
            int nCandidates = 5;
            int nTimes = 1000;
            var rgn = new Random();
            var people = from i in Enumerable.Range(0, nTimes)
                         select new Person(nCandidates, rgn);

            // First, turn each person into a giant list of each of their rankings
            // for all candidates. We consider all equal weight, so no need to track
            // who voted for who.

            var rankListing = from p in people
                                from candidate in Enumerable.Range(0, nCandidates)
                                select new 
                                {
                                    Candidate = candidate,
                                    Ranking = p.Ranking(candidate)
                                };

            // Count the number of times each person was ranking at each slot
            // in the above list, and count the number of times.

            var rankingPerCandidate = (from r in rankListing
                                      group r by r.Candidate into rankingsByCandidate
                                      select new
                                      {
                                          Candidate = rankingsByCandidate.Key,
                                          RankingCountList = (from vote in rankingsByCandidate
                                                              group vote by vote.Ranking into candidateRanking
                                                              select new
                                                              {
                                                                  Rank = candidateRanking.Key,
                                                                  Count = candidateRanking.Count()
                                                              }).ToDictionary(k => k.Rank, k => k.Count)
                                      }).ToArray();

            // Dump out so we can see in the test output

            var avg = (double)nTimes / (double)nCandidates;
            foreach (var c in rankingPerCandidate)
            {
                Console.WriteLine("Candidate {0}", c.Candidate);
                Func<double, double> fsdt = (double a) => Math.Sqrt(Math.Pow(avg - a, 2));
                foreach (var r in c.RankingCountList.Keys.OrderBy(k => k))
                {
                    Console.WriteLine("Rank {0} had {1} votes, with stdev {2}.",
                        r,
                        c.RankingCountList[r],
                        fsdt(c.RankingCountList[r]));
                }
            }

            // Make sure the sums are ok, and other things.
            // Statistically, other things can happen... so there isn't much else we 
            // can do for testing here, I'm afraid.

            Assert.IsTrue(rankingPerCandidate.Select(k => k.RankingCountList.Sum(d => d.Value)).All(t => t == nTimes), "Some votes didn't total up correctly");

            // Make sure nothing is more than 3 sigma out... shouldn't happen often, obviously.

            var sigma = Math.Sqrt(avg);
            Console.WriteLine("Average is {0} and expected sigma is {1}.", avg, sigma);
            Assert.IsTrue(rankingPerCandidate.SelectMany(k => k.RankingCountList.Select(p => p.Value)).Select(cnt => Math.Abs(avg - cnt) / sigma).All(dev => dev < 3.0), "std deviation is not in bounds");
        }
    }
}
