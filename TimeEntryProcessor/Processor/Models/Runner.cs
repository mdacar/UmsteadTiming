using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TimeEntryProcessor.Processor.Models
{
    public class Runner
    {
        public Guid RaceXRunnerID { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public List<TimeEntry> TimeEntries { get;set; }
        public List<Split> Splits { get; set; }


        public Runner()
        {
            TimeEntries = new List<TimeEntry>();
            Splits = new List<Split>();
        }


        internal AddTimeEntryResponse AddTimeEntry(TimeEntry entry)
        {
            // 1 - Check for duplicates
            // 2 - Check if it's past the finish
            // 3 - Check for too close to the previous entry at the same location


            var response = new AddTimeEntryResponse();

            response.Runner = this;
            if (IsDuplicateTimeEntry(entry))
            {
                response.Exceptions.Add(new Exception($"Duplicate time entry for {RaceXRunnerID} at {entry.RFIDReaderID} for {entry.ReaderTimestamp}"));
                response.UpdatedTimeEntryStatus = TimeEntryStatus.Invalid;
                response.UpdatedTimeEntryStatusReason = "Duplicate";
            }
            else
            {
                if (IsEntryPastFinish(entry))
                {
                    response.Exceptions.Add(new Exception($"Entry is past finish for {RaceXRunnerID} at {entry.RFIDReaderID} for {entry.ReaderTimestamp}"));
                    response.UpdatedTimeEntryStatus = TimeEntryStatus.Invalid;
                    response.UpdatedTimeEntryStatusReason = "Past Finish";
                }
                else
                {
                    if (IsEntryTooClose(entry))
                    {
                        response.Exceptions.Add(new Exception($"Entry is too close to previous entry for {RaceXRunnerID} at {entry.RFIDReaderID} for {entry.ReaderTimestamp}"));
                        response.UpdatedTimeEntryStatus = TimeEntryStatus.Invalid;
                        response.UpdatedTimeEntryStatusReason = "Too close to previous";
                    }
                    else
                    {
                        //If we get here it's a valid split.  Find out what checkpoint this split belongs in.

                    }
                }
            }

            return response;
        }

        public bool HasStartEntry()
        {

            return false;
        }

        private bool IsEntryTooClose(TimeEntry entry)
        {
            //Get the latest time entry from the same RFID Reader and prior to this timestamp
            //If the difference between the times is less than 1 hr return false

            var previousTimeEntry = TimeEntries
                .Where(t => t.RFIDReaderID == entry.RFIDReaderID && t.ReaderTimestamp < entry.ReaderTimestamp)
                .OrderByDescending(t => t.ReaderTimestamp)
                .FirstOrDefault();

            if (previousTimeEntry != null)
            {
                return TimeSpan.FromMilliseconds(entry.ReaderTimestamp - previousTimeEntry.ReaderTimestamp).TotalHours < 1;
            }

            return false;
        }

        private bool IsEntryPastFinish(TimeEntry entry)
        {
            //Does the runner already have a split that is the finish checkpoint?
            return false;
        }

        private bool IsDuplicateTimeEntry(TimeEntry entry)
        {
            return TimeEntries.Any(t => t.ReaderTimestamp == entry.ReaderTimestamp);
        }
    }
}
