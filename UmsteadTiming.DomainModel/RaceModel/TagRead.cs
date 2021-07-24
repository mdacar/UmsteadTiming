using System;
using UltimateTiming.Domain;

namespace UltimateTiming.DomainModel
{
    public class TagRead
    {
        public TagRead()
        {

        }

        public string TagId { get; set; }
        public int AntennaNumber { get; set; }
        public int SignalStrength { get; set; }
        public string TimeStamp { get; set; }
        public string TagType { get; set; }
        public string ReaderName { get; set; }
        public string RunnerName { get; set; }
        public DateTime? ConvertedTimeStamp
        {
            get
            {
                if (!string.IsNullOrWhiteSpace(TimeStamp))
                {
                    return UnixTimeConverter.ConvertUnixTimeStamp(TimeStamp);
                }
                return null;
            }
        }

        public bool IsHeartbeat { get; set; }

        public string TimeEntrySource { get; set; }

        public override string ToString()
        {
            return $"TagID: {TagId}\nReaderName: {ReaderName}\nTimeStamp: {TimeStamp}\nTagType: {TagType}";
        }
    }
}
