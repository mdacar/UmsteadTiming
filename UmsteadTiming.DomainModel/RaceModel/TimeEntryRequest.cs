using System;

namespace UltimateTiming.DomainModel
{
    public class TimeEntryRequest
    {

        public int RFIDReaderID { get; set; }

        public long TimeStamp { get; set; }

        public DateTime AbsoluteTime { get; set; }

        public Guid RaceXRunnerID { get; set; }

        public int? Source { get; set; }

        public string TagType { get; set; }
    }
}
