using System;
using System.Collections.Generic;
using System.Linq;


namespace UltimateTiming.DomainModel.Sorting
{
    public class TimeEntrySorter : ITimeEntrySorter
    {

        private IList<TimingLocation> _timingLocations;
        private IList<CheckPoint> _checkpoints;



        public TimeEntrySorter(IList<TimingLocation> timingLocations, IList<CheckPoint> checkpoints)
        {
            _timingLocations = timingLocations;
            _checkpoints = checkpoints;

        }



        public TimeEntrySortResponse Sort(Runner runner)
        {
            var response = SortRunnerTimeEntries(runner);

            SetHasMissingSplits(runner, response.Splits);

            return response;
        }

        private void SetHasMissingSplits(Runner runner, IList<Split> splits)
        {
            if (splits.Any())
            {
                var lastSplit = splits.Where(s => s.ElapsedMilliseconds == splits.Max(s2 => s2.ElapsedMilliseconds)).FirstOrDefault();
                var totalCheckpointSequence = _checkpoints.Where(cp => !cp.LongName.Contains("Airport") && cp.Sequence <= lastSplit.CheckPoint.Sequence).Sum(cp => cp.Sequence);
                var runnerCheckpointSequence = splits.Where(s => !s.CheckPoint.LongName.Contains("Airport")).Sum(s => s.CheckPoint.Sequence);
                runner.HasMissingSplits = totalCheckpointSequence != runnerCheckpointSequence;
            }
            else
            {
                runner.HasMissingSplits = false;
            }
        }


        #region Sorting Related
        private TimeEntrySortResponse SortRunnerTimeEntries(Runner runner)
        {
            var response = new TimeEntrySortResponse();
            //var timeEntries = RemoveDuplicates(runner.TimeEntries);
            //Only get the primary tags unless the primary tag was missed
            var orderedEntries = runner.TimeEntries
                .OrderBy(t => t.ElapsedTime).ThenBy(t => t.Source.Id);

            CheckPoint lastCheckPoint = null;
            CheckPoint currentCheckPoint = null;

            foreach (var entry in orderedEntries)
            {

                //Check for duplicates
                if (response.Splits.Any(s => s.TimeEntry.ReaderTimestamp == entry.ReaderTimestamp))
                {
                    entry.Status = TimeEntryStatus.Invalid;
                    entry.StatusReason = "Duplicate";
                    continue;
                }
                //System.Diagnostics.Debug.WriteLine($"{entry.Reader.ReaderName} - {entry.ReaderTimestamp} : {entry.AbsoluteTime}");
                //If I have more entries than checkpoints mark the entry as invalid.  Most likely people milling around the finish.
                if (IsEntryPastFinish(lastCheckPoint, entry))
                {
                    entry.Status = TimeEntryStatus.Invalid;
                    entry.StatusReason = "Time entry past finish";
                    continue;
                }
                else
                {
                    if (IsSecondary(entry, runner))
                    {
                        entry.Status = TimeEntryStatus.Secondary;
                        entry.StatusReason = "Secondary";
                        continue;
                    }
                    else
                    {
                        //Check to see if the entry is too close to another entry at the same checkpoint i.e. read twice
                        if (IsTooClose(entry, runner))
                        {
                            continue;
                        }
                        else
                        {
                            if (entry.Status == TimeEntryStatus.Valid || entry.Status == TimeEntryStatus.ModifiedValid || entry.Status == TimeEntryStatus.Unknown)
                            {

                                currentCheckPoint = GetCurrentCheckPoint(entry, lastCheckPoint);
                                if (currentCheckPoint == null)
                                {
                                    response.Errors.Add($"Entry for {entry.RaceXRunnerId} had an invalid checkpoint");
                                    entry.Status = TimeEntryStatus.Invalid;
                                    entry.StatusReason = "Unknown checkpoint";
                                }
                                else
                                {
                                    lastCheckPoint = currentCheckPoint;

                                    response.Splits.Add(CreateSplit(currentCheckPoint, entry, runner));
                                }
                            }
                            else
                            {
                                if (entry.Status != TimeEntryStatus.Invalid)
                                    response.Errors.Add($"Entry for {entry.RaceXRunnerId} had an invalid status of {entry.Status}");
                            }
                        }
                    }
                }
            }
            return response;
        }




        private CheckPoint GetCurrentCheckPoint(TimeEntry entry, CheckPoint lastCheckPoint)
        {
            var timingLocation = _timingLocations.Where(t => string.Compare(t.Code, entry.Reader.ReaderName, true) == 0).FirstOrDefault();
            var checkPointsForTimingLocation = _checkpoints.Where(c => c.TimingLocation.Code == timingLocation.Code).OrderBy(c => c.Sequence);
            //No previous checkpoints have been hit so this must be the start
            if (lastCheckPoint == null)
            {
                return checkPointsForTimingLocation.First();
            }
            else
            {
                return checkPointsForTimingLocation.Where(c => c.Sequence > lastCheckPoint.Sequence).FirstOrDefault();
            }
        }


        private bool IsTooClose(TimeEntry entry, Runner runner)
        {
            //If it's not marked as invalid or secondary above see if there's another time entry from the same location in the last configured timespan

            var entriesSameLocation = runner.TimeEntries.Where(t => string.Compare(t.Reader.ReaderName, entry.Reader.ReaderName, true) == 0
                && t.ElapsedTime < entry.ElapsedTime
                && entry.ElapsedTime != t.ElapsedTime
                && (t.Status != TimeEntryStatus.Invalid && t.Status != TimeEntryStatus.Secondary && t.Status != TimeEntryStatus.Valid));

            var duplicateEntry = entriesSameLocation.Where(t => (entry.ElapsedTime - t.ElapsedTime) < 60 * 60000).FirstOrDefault();
            if (duplicateEntry != null)
            {
                entry.Status = TimeEntryStatus.Invalid;
                entry.StatusReason = string.Format("Too close to previous entry: {0} minutes",
                        Math.Round(TimeSpan.FromMilliseconds(Math.Abs(duplicateEntry.ElapsedTime - entry.ElapsedTime)).TotalMinutes));
                return true;
            }

            return false;
        }

        private bool IsSecondary(TimeEntry entry, Runner runner)
        {
            if (entry.Source.Description != "RFID Reader")
            {
                //If the time entry is from something other than an RFID reader and I have an RFID reader entry within 5 minutes mark it as secondary and therefore not relevant.
                if (runner.TimeEntries.Where(te => te.Source.Description == "RFID Reader" && te.Reader.Id == entry.Reader.Id).Any(t => Math.Abs(t.ElapsedTime - entry.ElapsedTime) < 1800000))
                {
                    return true;
                }
            }

            return false;
        }

        private bool IsEntryPastFinish(CheckPoint lastCheckpoint, TimeEntry entry)
        {
            //TODO: Figure out how to make this work if the last checkpoint isn't greater than 100 miles.  I.E. Missing the final HQ split and getting to the airport a 9th time.
            return lastCheckpoint != null && lastCheckpoint.TotalMiles >= 100;
        }


        private Split CreateSplit(CheckPoint currentCheckPoint, TimeEntry entry, Runner runner)
        {
            var split = new Split()
            {
                CheckPoint = currentCheckPoint,
                ElapsedMilliseconds = entry.ElapsedTime,
                RaceXRunnerId = entry.RaceXRunnerId,
                Runner = runner,
                SplitTime = entry.AbsoluteTime,
                TimeEntry = entry
            };
            entry.Split = split;
            return split;
        }

        #endregion

    }
}
