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
            var e = new Election()
            {
                NumberOfPeople = 400,
                NumberOfCandidates = 4
            };
            uint nElections = 1000;

            // Election:
            // 1. Everyone has a single vote
            // 2. If someone is > 50%, then that person is the winner.
            // 3. Otherwise the top two are kept, and we have a run-off with everyone has single vote
            e.AddStep(new ESOnlyBestCounts());
            e.AddStep(new ESKeepThoseBetterThan(0.50) { DoNothingIfNoOnePasses = true });
            e.AddStep(new ESKeepBestN(2));
            e.AddStep(new ESOnlyBestCounts());

            var flips = e.RunElectionEnsemble(nElections);
            Console.WriteLine("Saw {0} flips in {1} elections.", flips, nElections);

        }

    }
}
