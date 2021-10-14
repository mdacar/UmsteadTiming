using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TimeEntryProcessor.Processor.Services;

namespace TimeEntryProcessor.Processor.Models
{
    public class Race
    {
        public string ID { get; set; }
        public string Name { get; set; }
        public DateTime StartDate { get; set; }

        public List<Checkpoint> Checkpoints { get; set; }
        public List<TimingLocation> TimingLocations { get; set; }
        public List<TimeEntrySource> TimeEntrySources { get; set; }
        public List<RFIDReader> RFIDReaders { get; set; }
        public List<Runner> Runners { get; set; }

        

        internal AddTimeEntryResponse AddTimeEntry(TimeEntry entry)
        {
            var sorter = new TimeEntrySorter(TimingLocations, Checkpoints, StartDate);

            var runner = Runners.Where(r => r.RaceXRunnerID == entry.RaceXRunnerID).FirstOrDefault();
            
            if (runner != null)
            {
                runner.TimeEntries.Add(entry);
                var response = sorter.Sort(runner);
                return runner.AddTimeEntry(entry);   
            }
            else
            {
                var response = new AddTimeEntryResponse();
                response.Exceptions.Add(new Exception($"Runner {entry.RaceXRunnerID} not found"));
                return response;
            }

            
        }

        internal void AddTimeEntrySources(IEnumerable<TimeEntrySource> timeEntrySources)
        {
            TimeEntrySources = new List<TimeEntrySource>(timeEntrySources);
        }

        internal void AddRFIDReaders(IEnumerable<RFIDReader> readers)
        {
            RFIDReaders = new List<RFIDReader>(readers);
        }

        internal void AddTimingLocations(IEnumerable<TimingLocation> timingLocations)
        {
            TimingLocations = new List<TimingLocation>(timingLocations);
        }

        internal void AddCheckPoints(IEnumerable<Checkpoint> checkpoints)
        {
            Checkpoints = new List<Checkpoint>(checkpoints);
        }
    }
}
