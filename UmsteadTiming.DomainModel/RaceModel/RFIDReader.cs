using System;


namespace UltimateTiming.DomainModel
{


    public class RFIDReader
    {

        public const string HQ_READER_NAME = "HQ_Reader";


        public string ReaderName { get; set; }
        public int Id { get; set; }
        public string HeartbeatTagId { get; set; }
        public ReaderStatus Status
        {
            get
            {
                double minutesSinceLastHeartbeat = DateTime.UtcNow.Subtract(LastHeartbeat).TotalMinutes;
                if (minutesSinceLastHeartbeat < 10)
                {
                    return ReaderStatus.Good;
                }
                else if (minutesSinceLastHeartbeat >= 10 && minutesSinceLastHeartbeat < 15)
                {
                    return ReaderStatus.Caution;
                }
                else
                {
                    return ReaderStatus.Stopped;
                }
            }
        }
        public DateTime LastHeartbeat { get; set; }

    }
}
