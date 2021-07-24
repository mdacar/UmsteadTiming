using System.Collections.Generic;
using System.Linq;

namespace UltimateTiming.DomainModel.PaceCalculation
{
    public class SplitProjector
    {

        public List<Split> ProjectSplits(Race race, Runner runner)
        {
            var output = new List<Split>();

            foreach (var cp in race.CheckPoints)
            {
                //If the runner doesn't have a split project one.
                if (!runner.Splits.Any(s => s.CheckPoint.Id == cp.Id))
                {
                    //Get the other splits at that timing location
                    var otherSplits = runner.Splits.Where(s => s.CheckPoint.TimingLocation.Id == cp.TimingLocation.Id).OrderBy(s => s.CheckPoint.TimingLocationSequence);
                    //Find the nearest previous timing location sequence and use that pace
                    for (int i = cp.TimingLocationSequence; i > 0; i--)
                    {
                        var referenceSplit = otherSplits.Where(s => s.CheckPoint.TimingLocationSequence == i).FirstOrDefault();

                        if (referenceSplit != null)
                        {
                            var splitTimeProjected = (long)(referenceSplit.OverallPace.TotalMilliseconds * (double)cp.TotalMiles);
                            output.Add(new Split()
                            {
                                CheckPoint = cp,
                                ElapsedMilliseconds = splitTimeProjected,
                                RaceId = race.Id,
                                RaceXRunnerId = runner.RaceXRunnerId,
                                Runner = runner
                            }
                            );
                        }

                    }

                }
            }

            return output;
        }

    }
}
