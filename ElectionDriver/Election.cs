﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ElectionDriver
{
    /// <summary>
    /// Told how many people in an election and a way to run it, run the election, and
    /// analyze it by dropping each of the other candidates in turn.
    /// </summary>
    public class Election
    {
        public Election()
        {
            NumberOfPeople = 400;
            NumberOfCandidates = 1000;
        }

        /// <summary>
        /// Get/Set the number of poeple that are going to vote.
        /// </summary>
        public int NumberOfPeople { get; set; }

        /// <summary>
        /// Get/Set the number of candidates that are going to run.
        /// </summary>
        public int NumberOfCandidates { get; set; }

        /// <summary>
        /// Run an election and return the results. The results are sorted by weight.
        /// </summary>
        /// <returns>Ranking of candidates from winner on down</returns>
        public async Task<CandiateRanking[]> RunSingleElection()
        {
            // Generate the people
            var people = GeneratePeople().ToArray();

            return await RunSingleElectionInternal(people);
        }

        /// <summary>
        /// Run an election on a given set of people. Return the full candidate ordering
        /// when we are done.
        /// </summary>
        /// <param name="people"></param>
        /// <returns>Weights for everyone that got votes, ordered from winner on down</returns>
        private Task<CandiateRanking[]> RunSingleElectionInternal(Person[] people)
        {
            // Quick checks.
            if (_steps.Count == 0)
                throw new InvalidOperationException("Election has no steps defined");
            if (people == null || people.Length == 0)
                throw new ArgumentException("Election can't happen without people");

            return Task<CandiateRanking[]>.Factory.StartNew(() =>
                {
                    List<CandiateRanking[]> results = new List<CandiateRanking[]>();
                    int[] keepOnly = new int[0];
                    int oldKeepCount = 0;
                    var peopleThisRound = people;
                    foreach (var s in _steps)
                    {
                        // First, window out people if we need to.
                        if (keepOnly.Length != oldKeepCount)
                        {
                            var keepThem = keepOnly.ToArray();
                            peopleThisRound = peopleThisRound.Select(p => p.KeepCandidates(keepThem)).ToArray();
                            oldKeepCount = keepOnly.Length;
                        }

                        // Run the election
                        var stepResult = s.RunStep(peopleThisRound, results.ToArray());

                        // Some simple case results that will terminate our processing of the election.
                        if (stepResult == null)
                            throw new InvalidOperationException("Election step returned a null value!");
                        if (stepResult.Length == 0)
                            throw new ElectionFailureException("Election step returned zero candidates");
                        if (stepResult.Length == 1)
                            return stepResult;

                        // Save the results for use in future steps.
                        results.Add(stepResult);

                        // And only let through the candidates that made it.
                        keepOnly = stepResult.Select(c => c.candidate).ToArray();
                    }

                    return (from fr in results.Last()
                            orderby fr.ranking descending
                            select fr).ToArray();
                });
        }

        /// <summary>
        /// Run an election with a set of randomly generated poeple. Once they are run,
        /// remove each non-winning candidate in turn and count the number of times
        /// that the winner changes.
        /// </summary>
        /// <returns></returns>
        public async Task<ElectionResults> RunElection()
        {
            // Generate the people.
            var people = GeneratePeople().ToArray();

            // Next, run the election
            var result = await RunSingleElectionInternal(people);

            // Now, loop over each candidate and remove them... unless they are the winner!
            var winner = result[0].candidate;
            int flips = 0;
            for (int i_cand = 0; i_cand < NumberOfCandidates; i_cand++)
            {
                if (i_cand != winner)
                {
                    var peopleWithOut = people.Select(p => p.RemoveCandidates(i_cand)).ToArray();
                    var resultWithOut = await RunSingleElectionInternal(peopleWithOut);
                    if (resultWithOut[0].candidate != winner)
                        flips++;
                }
            }

            // Build the result

            var r = new ElectionResults();
            r.flips = flips;
            r.candidateOrdering = (from rs in result
                                   orderby rs.ranking descending
                                   select rs.candidate).ToArray();

            return r;
        }

        /// <summary>
        /// Generate the people we want.
        /// </summary>
        /// <returns></returns>
        private IEnumerable<Person> GeneratePeople()
        {
            var r = new Random();
            bool satisfiedAllConstraints = _constraints.Count == 0;
            int constraintIndex = 0;
            int counter = 0;
            int thisConstraintSatisifedEvents = 0;

            var constraints = (from c in _constraints
                               orderby c.fraction ascending
                               select c).ToArray();

            List<Person> alreadyGood = new List<Person>();

            while (counter < NumberOfPeople)
            {
                var p = new Person(NumberOfCandidates, r);
                if (!satisfiedAllConstraints)
                {
                        if (constraints.Take(constraintIndex).Where(c => c.constraint(p)).Any())
                            continue;
                        if (!constraints[constraintIndex].constraint(p))
                            continue;
                        thisConstraintSatisifedEvents++;
                        if (thisConstraintSatisifedEvents >= (constraints[constraintIndex].fraction * NumberOfPeople))
                        {
                            constraintIndex++;
                            if (constraintIndex < constraints.Length)
                            {
                                thisConstraintSatisifedEvents = alreadyGood.Where(lp => constraints[constraintIndex].constraint(lp)).Count();
                            }
                            else
                            {
                                satisfiedAllConstraints = true;
                            }
                        }
                }
                else
                {
                    if (constraints.Where(c => c.constraint(p)).Any())
                        continue;
                }

                alreadyGood.Add(p);
                yield return p;
                counter++;
            }

            if (!satisfiedAllConstraints)
            {
                throw new InvalidOperationException("People constraints may be incompatible - unable to satisfy them this time around!");
            }
        }

        /// <summary>
        /// Keep track of the constraints we will put on the people we generate.
        /// </summary>
        private struct PeopleConstraint
        {
            public double fraction;
            public Func<Person, bool> constraint;
        }

        private List<PeopleConstraint> _constraints = new List<PeopleConstraint>();

        /// <summary>
        /// Make sure exactly fraction of events satisfy the constraint. This remains true
        /// even if multiple guys are added as constraints! So be ware! :-)
        /// </summary>
        /// <param name="fraction"></param>
        /// <param name="peopleChecK"></param>
        public void AddPeopleConstraint(double fraction, Func<Person, bool> peopleConstraint)
        {
            _constraints.Add(new PeopleConstraint() { fraction = fraction, constraint = peopleConstraint });
        }

        /// <summary>
        /// Clear all people constraints
        /// </summary>
        public void ClearPeopleConstrains()
        {
            _constraints.Clear();
        }

        /// <summary>
        /// The list of steps that we go through for an election.
        /// </summary>
        List<IElectionStep> _steps = new List<IElectionStep>();

        /// <summary>
        /// Add a step to the election steps.
        /// </summary>
        /// <param name="step"></param>
        public void AddStep(IElectionStep step)
        {
            _steps.Add(step);
        }

        public class ElectionResults
        {
            /// <summary>
            /// Number of times the winner (first place candidate) changed when we varied the
            /// people who were in the election.
            /// </summary>
            public int flips;

            /// <summary>
            /// The order of the candidates - the first one is the first candidate, etc. Gives the
            /// ranking of the candidates.
            /// </summary>
            public int[] candidateOrdering;
        }

        public class ElectionEnsembleResults
        {
            /// <summary>
            /// The number of times an election winner changed when a person was removed from
            /// a population/electoin run.
            /// </summary>
            public int flips;

            /// <summary>
            /// Hold the ranking frequency info from the straight-up elections.
            /// </summary>
            public struct CandidateResults
            {
                /// <summary>
                /// The times that this candidate got first (index 0), second (index 1), etc.
                /// from the full on election
                /// </summary>
                public int[] resultTimes;
            }

            /// <summary>
            /// The times in each guy for candidate 0, 1, 2, etc.
            /// </summary>
            public CandidateResults[] candidateResults;
        }

        /// <summary>
        /// Repeatedly run the election, and count the number of elections that
        /// contain at least one flip.
        /// </summary>
        /// <param name="p"></param>
        /// <returns></returns>
        public async Task<ElectionEnsembleResults> RunElectionEnsemble(uint numberOfElections)
        {
            if (numberOfElections == 0)
                throw new ArgumentException("Asked to run an ensemble of zero elections!");

            var allResults = from i_election in Enumerable.Range(0, (int)numberOfElections)
                             select RunElection();

            var flipsPerResult = await Task.WhenAll(allResults.ToArray());

            // Do the simple part of the results - the number of flips.
            var r = new ElectionEnsembleResults();
            r.flips = flipsPerResult.Select(c => c.flips > 0 ? 1 : 0).Sum();

            // Now, get back the election ordering results
            var rankingGroups = from f in flipsPerResult
                          from rOrder in Enumerable.Range(0, f.candidateOrdering.Length)
                          group rOrder by f.candidateOrdering[rOrder];

            var rankingPerCandidate = from candidate in rankingGroups
                                      select new
                                      {
                                          Candidate = candidate.Key,
                                          Ranking = from candRank in candidate
                                                           group  candRank by candRank into candidateListing
                                                           select new
                                                           {
                                                               Rank = candidateListing.Key,
                                                               Count = candidateListing.Count()
                                                           }
                                      };

            var rankingPerCandidateDict = rankingPerCandidate.ToDictionary(k => k.Candidate, v => v.Ranking.ToDictionary(vk => vk.Rank, vk => vk.Count));
            r.candidateResults = new ElectionEnsembleResults.CandidateResults[NumberOfCandidates];
            for (int iCand = 0; iCand < NumberOfCandidates; iCand++)
            {
                r.candidateResults[iCand].resultTimes = new int[NumberOfCandidates];
                if (rankingPerCandidateDict.ContainsKey(iCand)) {
                    for (int iRank = 0; iRank < NumberOfCandidates; iRank++) 
                    {
                        if (rankingPerCandidateDict[iCand].ContainsKey(iRank))
                            r.candidateResults[iCand].resultTimes[iRank] = rankingPerCandidateDict[iCand][iRank];
                    }
                }
            }
            return r;
        }
    }
}
