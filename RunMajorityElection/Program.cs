using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ElectionDriver;

namespace RunMajorityElection
{
    class Program
    {
        /// <summary>
        /// Run a very simple majority election
        /// </summary>
        /// <param name="args"></param>
        static void Main(string[] args)
        {
            uint nElections = 4000;
            uint nCandidates = 4;
            Console.WriteLine("Running the Majority election with {0} candidates {1} times.", nCandidates, nElections);

            var e = new Election()
            {
                NumberOfCandidates = (int) nCandidates,
                NumberOfPeople = 4000
            };
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
                (point, numPoints) => Tuple.Create<double, Func<Person, bool>>(0.1*point, p => p.Ranking(0) == nCandidates-1),
                points: 10,
                numberPerPoint: (int) nElections
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
