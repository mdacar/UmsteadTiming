using System;
using System.Collections.Generic;
using System.Text;

namespace TimeEntryProcessor
{
    public enum TimeEntryStatus
    {
        Valid = 1,
        Error = 2,
        Invalid = 3,
        ModifiedValid = 4,
        Unknown = 5,
        Secondary = 6
    }

    public class TimeEntry
    {
        public Guid ID { get; set; }
        public Guid RaceXRunnerID { get; set; }
        public long ElapsedTime { get; set; }
        public DateTime AbsoluteTime { get; set; }
        public long ReaderTimestamp { get; set; }
        public int RFIDReaderID { get; set; }
        public int TimeEntryStatusID { get; set; }
        public int? TimeEntrySource { get; set; }
        public string TagType { get; set; }
        public string StatusReason { get; internal set; }
    }
}
