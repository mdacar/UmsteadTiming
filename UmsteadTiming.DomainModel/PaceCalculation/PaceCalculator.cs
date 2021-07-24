using System;
using System.Collections.Generic;
using System.Linq;

namespace UltimateTiming.DomainModel.PaceCalculation
{
    public class PaceCalculator : IPaceCalc
    {
        public void CalculatePace(Runner runner, Race race)
        {
            Split lastSplit = null;
            var orderedSplits = runner.Splits.OrderBy(s => s.ElapsedMilliseconds);

            foreach (var split in orderedSplits)
            {
                if (!split.CheckPoint.ShortName.ToLower().Contains("air") && split.CheckPoint.TotalMiles != 0)
                {
                    split.SplitPace = GetSplitPace(split, lastSplit);
                    split.SplitElapsedTime = GetSplitTime(split, lastSplit);
                    split.OverallPace = new TimeSpan(0, 0, 0, 0, Convert.ToInt32(split.ElapsedMilliseconds / split.TotalMiles));
                    lastSplit = split;
                }
            }
        }


        public IEnumerable<Split> CalculatePace(IEnumerable<Split> splits)
        {
            Split lastSplit = null;
            if (splits != null)
            {
                var orderedSplits = splits.OrderBy(s => s.ElapsedMilliseconds).ToList();

                foreach (var split in orderedSplits)
                {
                    if (split.CheckPoint != null)
                    {
                        if (!split.CheckPoint.ShortName.ToLower().Contains("air") && split.CheckPoint.TotalMiles != 0)
                        {
                            split.SplitPace = GetSplitPace(split, lastSplit);
                            split.SplitElapsedTime = GetSplitTime(split, lastSplit);
                            split.OverallPace = new TimeSpan(0, 0, 0, 0, Convert.ToInt32(split.ElapsedMilliseconds / split.TotalMiles));
                            lastSplit = split;
                        }
                    }

                }

                return orderedSplits;
            }
            else
            {
                return null;
            }

        }


        private TimeSpan GetSplitTime(Split split, Split lastSplit)
        {

            return new TimeSpan(0, 0, 0, 0, Convert.ToInt32(split.ElapsedMilliseconds - ((lastSplit != null) ? lastSplit.ElapsedMilliseconds : 0)));
        }

        private TimeSpan GetSplitPace(Split split, Split lastSplit)
        {
            long lastSplitTime = (lastSplit != null) ? lastSplit.ElapsedMilliseconds : 0;
            decimal lastSplitDistance = (lastSplit != null) ? lastSplit.TotalMiles : 0;

            decimal distance = split.CheckPoint.TotalMiles - lastSplitDistance;
            long time = split.ElapsedMilliseconds - lastSplitTime;

            return new TimeSpan(0, 0, 0, 0, Convert.ToInt32(time / distance));
        }
    }
}
