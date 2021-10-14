using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TimeEntryProcessor.Processor.Models;

namespace TimeEntryProcessor.Processor.Services
{
    public class TimeEntrySorter
    {
        private IList<TimingLocation> _timingLocations;
        private IList<Checkpoint> _checkpoints;
        private readonly DateTime _raceStart;

        public TimeEntrySorter(IList<TimingLocation> timingLocations, IList<Checkpoint> checkpoints, DateTime raceStart)
        {
            _timingLocations = timingLocations;
            _checkpoints = checkpoints;
            _raceStart = raceStart;

        }



        public TimeEntrySortResponse Sort(Runner runner)
        {
            var response = SortRunnerTimeEntries(runner);

            return response;
        }



        #region Sorting Related
        private TimeEntrySortResponse SortRunnerTimeEntries(Runner runner)
        {
            var response = new TimeEntrySortResponse();
            //var timeEntries = RemoveDuplicates(runner.TimeEntries);
            //Only get the primary tags unless the primary tag was missed
            var orderedEntries = runner.TimeEntries
                .OrderBy(t => t.ElapsedTime).ThenBy(t => t.TimeEntrySource);

            Checkpoint lastCheckPoint = null;
            Checkpoint currentCheckPoint = null;

            FindDuplicates(ref orderedEntries);
            FindEntriesTooClose(ref orderedEntries);

            foreach (var entry in orderedEntries)
            {
                
            //    //System.Diagnostics.Debug.WriteLine($"{entry.Reader.ReaderName} - {entry.ReaderTimestamp} : {entry.AbsoluteTime}");
            //    //If I have more entries than checkpoints mark the entry as invalid.  Most likely people milling around the finish.
            //    if (IsEntryPastFinish(lastCheckPoint, entry))
            //    {
            //        entry.Status = TimeEntryStatus.Invalid;
            //        entry.StatusReason = "Time entry past finish";
            //        continue;
            //    }
            //    else
            //    {

            //        //Check to see if the entry is too close to another entry at the same checkpoint i.e. read twice
            //        if (IsTooClose(entry, runner))
            //        {
            //            continue;
            //        }
            //        else
            //        {
            //            if (entry.Status == TimeEntryStatus.Valid || entry.Status == TimeEntryStatus.ModifiedValid || entry.Status == TimeEntryStatus.Unknown)
            //            {

            //                currentCheckPoint = GetCurrentCheckPoint(entry, lastCheckPoint);
            //                if (currentCheckPoint == null)
            //                {
            //                    response.Errors.Add($"Entry for {entry.RaceXRunnerId} had an invalid checkpoint");
            //                    entry.Status = TimeEntryStatus.Invalid;
            //                    entry.StatusReason = "Unknown checkpoint";
            //                }
            //                else
            //                {
            //                    lastCheckPoint = currentCheckPoint;

            //                    response.Splits.Add(CreateSplit(currentCheckPoint, entry, runner));
            //                }
            //            }
            //            else
            //            {
            //                if (entry.Status != TimeEntryStatus.Invalid)
            //                    response.Errors.Add($"Entry for {entry.RaceXRunnerId} had an invalid status of {entry.Status}");
            //            }
            //        }

            //    }
            }
            return response;
        }

        private void FindEntriesTooClose(ref IOrderedEnumerable<TimeEntry> orderedEntries)
        {
            var previousTimestamps = new Dictionary<int, long>();
            foreach (var timeEntry in orderedEntries)
            {
                //Only check for them being too close if we haven't already marked the entry as invalid
                if (timeEntry.TimeEntryStatusID != (int)TimeEntryStatus.Invalid)
                {
                    //Look in the dictionary for the RFID reader ID, if we have it check to see if the value of the time entry
                    //is less than 1 hour from the previous entry at that RFID reader
                    if (previousTimestamps.ContainsKey(timeEntry.RFIDReaderID))
                    {
                        if (TimeSpan.FromMilliseconds(timeEntry.ReaderTimestamp / 1000 - previousTimestamps[timeEntry.RFIDReaderID] / 1000).TotalHours < 1)
                        {
                            timeEntry.StatusReason = "Too close to previous entry";
                            timeEntry.TimeEntryStatusID = (int)TimeEntryStatus.Invalid;
                        }
                        previousTimestamps[timeEntry.RFIDReaderID] = timeEntry.ReaderTimestamp;
                    }
                    else
                    {
                        previousTimestamps.Add(timeEntry.RFIDReaderID, timeEntry.ReaderTimestamp);
                    }
                }
            }
        }

        private void FindDuplicates(ref IOrderedEnumerable<TimeEntry> timeEntries)
        {
            long previousTimestamp = 0;

            foreach (var timeEntry in timeEntries)
            {
                if (previousTimestamp == timeEntry.ReaderTimestamp)
                {
                    timeEntry.StatusReason = "Duplicate";
                    timeEntry.TimeEntryStatusID = (int)TimeEntryStatus.Invalid;
                }
                previousTimestamp = timeEntry.ReaderTimestamp;
            }
        }

        //private Checkpoint GetCurrentCheckPoint(TimeEntry entry, Checkpoint lastCheckPoint)
        //{
        //    var timingLocation = _timingLocations.Where(t => string.Compare(t.Code, entry.ReaderName, true) == 0).FirstOrDefault();
        //    var checkPointsForTimingLocation = _checkpoints.Where(c => c.TimingLocation.Code == timingLocation.Code).OrderBy(c => c.Sequence);
        //    //No previous checkpoints have been hit so this must be the start
        //    if (lastCheckPoint == null)
        //    {
        //        return checkPointsForTimingLocation.First();
        //    }
        //    else
        //    {
        //        return checkPointsForTimingLocation.Where(c => c.Sequence > lastCheckPoint.Sequence).FirstOrDefault();
        //    }
        //}


        //private bool IsTooClose(TimeEntry entry, Runner runner)
        //{
        //    //If it's not marked as invalid or secondary above see if there's another time entry from the same location in the last configured timespan

        //    var entriesSameLocation = runner.TimeEntries.Where(t => t.RFIDReaderId == entry.RFIDReaderId
        //        && t.ElapsedTime < entry.ElapsedTime
        //        && entry.ElapsedTime != t.ElapsedTime
        //        && (t.Status != TimeEntryStatus.Invalid && t.Status != TimeEntryStatus.Secondary && t.Status != TimeEntryStatus.Valid));

        //    var duplicateEntry = entriesSameLocation.Where(t => (entry.ElapsedTime - t.ElapsedTime) < 60 * 60000).FirstOrDefault();
        //    if (duplicateEntry != null)
        //    {
        //        entry.Status = TimeEntryStatus.Invalid;
        //        entry.StatusReason = string.Format("Too close to previous entry: {0} minutes",
        //                Math.Round(TimeSpan.FromMilliseconds(Math.Abs(duplicateEntry.ElapsedTime - entry.ElapsedTime)).TotalMinutes));
        //        return true;
        //    }

        //    return false;
        //}


        //private bool IsEntryPastFinish(Checkpoint lastCheckpoint, TimeEntry entry)
        //{
        //    //TODO: Figure out how to make this work if the last checkpoint isn't greater than 100 miles.  I.E. Missing the final HQ split and getting to the airport a 9th time.
        //    return lastCheckpoint != null && lastCheckpoint.TotalMiles >= 100;
        //}


        //private Split CreateSplit(Checkpoint currentCheckPoint, TimeEntry entry, Runner runner)
        //{
        //    var split = new Split()
        //    {
        //        CheckPoint = currentCheckPoint,
        //        ElapsedMilliseconds = entry.ElapsedTime,
        //        RaceXRunnerId = entry.RaceXRunnerId,
        //        Runner = runner,
        //        SplitTime = entry.AbsoluteTime,
        //        TimeEntry = entry
        //    };
        //    entry.Split = split;
        //    return split;
        //}

        #endregion
    }
}
