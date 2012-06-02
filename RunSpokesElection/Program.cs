using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ElectionDriver;

namespace RunSpokesElection
{
    class Program
    {
        /// <summary>
        /// Try to emulate some fairly complex election algorithms that I've seen around
        /// the web.
        /// </summary>
        /// <param name="args"></param>
        static void Main(string[] args)
        {
            uint nElections = 1000;
            uint nCandidates = 4;
            var e = new Election()
            {
                NumberOfPeople = 400,
                NumberOfCandidates = (int) nCandidates
            };

            // Election:
            // 1. Everyone has a single vote
            // 2. If someone is > 50%, then that person is the winner.
            // 3. Otherwise the top two are kept, and we have a run-off with everyone has single vote
            e.AddStep(new ESOnlyBestCounts());
            e.AddStep(new ESKeepThoseBetterThan(0.50) { DoNothingIfNoOnePasses = true });
            e.AddStep(new ESKeepBestN(2));
            e.AddStep(new ESOnlyBestCounts());

            var flips = e.RunElectionEnsemble(nElections).Result;
            Console.WriteLine("Saw {0} flips in {1} elections.", flips.flips, nElections);
            for (int icand = 0; icand < flips.candidateResults.Length; icand++)
            {
                Console.Write("Candidate {0}: ", icand);
                for (int irank = 0; irank < flips.candidateResults.Length; irank++)
                {
                    Console.Write("{0}={1}, ", irank, flips.candidateResults[icand].resultTimes[irank]);
                }
                Console.WriteLine();
            }

            // Do a trend as a function of candidate 0...

            var eTrend = new ElectionTrend(e);
            var results = eTrend.RunTrend(
                (point, numPoints) => Tuple.Create<double, Func<Person, bool>>(0.02 * point, p => p.Ranking(0) == nCandidates - 1),
                points: 15
                );

            for (int i = 0; i < results.Length; i++)
            {
                var r = results[i].Result;
                Console.WriteLine("Election with candidate 0 having {0}% of the vote ({1} flips):", 10.0 * i, r.flips);
                for (int icand = 0; icand < flips.candidateResults.Length; icand++)
                {
                    Console.Write("  Candidate {0}: ", icand);
                    for (int irank = 0; irank < r.candidateResults.Length; irank++)
                    {
                        Console.Write("{0}={1}, ", irank, r.candidateResults[icand].resultTimes[irank]);
                    }
                    Console.WriteLine();
                }
            }
        }

    }
}
