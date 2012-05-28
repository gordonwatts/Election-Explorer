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
        public void TestExplicitCreation1()
        {
            var p = new Person(0);
            Assert.AreEqual(1, p.FullRanking().Count(), "single candidate, not ranked");
            Assert.AreEqual(0, p.FullRanking().First().candidate, "first candidate");
            Assert.AreEqual(0, p.FullRanking().First().ranking, "first candidate ranking");
            Assert.AreEqual(0, p.Ranking(0), "first candidate direct ranking");
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void TestExplicitCreate1Bad()
        {
            var p = new Person(0);
            p.Ranking(1);
        }

        [TestMethod]
        public void TestExplicitCreation3()
        {
            var p = new Person(2, 0, 1);
            Assert.AreEqual(1, p.Ranking(0), "Rank of 0");
            Assert.AreEqual(0, p.Ranking(1), "Rank of 1");
            Assert.AreEqual(2, p.Ranking(2), "Rank of 2");
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void TestExplicitCreation3BadMissing()
        {
            var p = new Person(2, 0);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void TestExplicitCreation3BadDuplicate()
        {
            var p = new Person(2, 0, 2);
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
        [ExpectedException(typeof(ArgumentException))]
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
            TestFullRankingWithOneGoneParameterized(new Person(0, 1, 2));
            TestFullRankingWithOneGoneParameterized(new Person(0, 2, 1));
            TestFullRankingWithOneGoneParameterized(new Person(1, 0, 2));
            TestFullRankingWithOneGoneParameterized(new Person(1, 2, 0));
            TestFullRankingWithOneGoneParameterized(new Person(2, 0, 1));
            TestFullRankingWithOneGoneParameterized(new Person(2, 1, 0));
        }

        private static void TestFullRankingWithOneGoneParameterized(Person p)
        {
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
                if (fullOrdering[i_f].candidate != 1)
                {
                    if (fullOrdering[i_f].candidate == partialOrdering[i_p].candidate)
                    {
                        i_p++;
                    }
                }
            }
            Assert.AreEqual(partialOrdering.Length, i_p, "Partial list not ordered correctly");
        }

        [TestMethod]
        public void TestFullRankingWithOneGone2()
        {
            var p = new Person(2, 1, 0);
            Assert.AreEqual(0, p.Ranking(0), "Rank for 0");
            Assert.AreEqual(2, p.Ranking(2), "Rank for 0");
            var ranking = p.FullRanking(1).ToArray();
            Assert.AreEqual(2, ranking.Length, "# of ranked outputs");

            var c0 = ranking.Where(c => c.candidate == 0).First().ranking;
            var c2 = ranking.Where(c => c.candidate == 2).First().ranking;

            Assert.AreEqual(0, c0, "Ranking for candidate 0");
            Assert.AreEqual(1, c2, "Ranking for candidate 2");
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

        [TestMethod]
        public void TestRemoveCandidates1()
        {
            var p = new Person(2, 1, 0);
            var p1 = p.RemoveCandidates(1);
            Assert.AreEqual(0, p1.Ranking(0), "Ranking of zero");
            Assert.AreEqual(1, p1.Ranking(2), "Ranking of candidate #2");
            Assert.AreEqual(2, p1.FullRanking().Count(), "# of candidates around now");
            Assert.AreEqual(2, p1.NumberOfCandidates, "# of candidates stored");
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void TestRemoveCandidates1RemovedCandidateReference()
        {
            var p = new Person(2, 1, 0);
            var p1 = p.RemoveCandidates(1);
            p1.Ranking(1);
        }

        [TestMethod]
        public void TestRemoveCandidates2()
        {
            var p = new Person(2, 1, 0);
            var p1 = p.RemoveCandidates(1, 0);
            Assert.AreEqual(0, p1.Ranking(2), "Ranking of candidate #2");
            Assert.AreEqual(1, p1.FullRanking().Count(), "# of candidates around now");
        }

        [TestMethod]
        public void TestRemoveCandidates1Twice()
        {
            var p = new Person(2, 1, 0);
            var p1 = p.RemoveCandidates(1);
            var p2 = p.RemoveCandidates(1);
            Assert.AreEqual(2, p2.NumberOfCandidates, "# of candidates after 1 removed twice");
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
