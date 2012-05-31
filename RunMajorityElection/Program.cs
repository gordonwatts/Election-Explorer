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
            uint nElections = 50;

            var e = new Election()
            {
                NumberOfCandidates = 3,
                NumberOfPeople = 4000
            };
            e.AddStep(new ESOnlyBestCounts());

            var flips = e.RunElectionEnsemble(nElections).Result;

            Console.WriteLine("Saw {0} flips in {1} elections.", flips, nElections);
        }

    }
}
