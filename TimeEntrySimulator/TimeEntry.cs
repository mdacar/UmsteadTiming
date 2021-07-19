using Newtonsoft.Json;
using System;

namespace TimeEntrySimulator
{
    [Serializable()]
    public class TimeEntry
    {
        public string ReaderName { get; set; }
        public string UnixTime { get; set; }
        public int AntennaNumber { get; set; }
        public string TagId { get; set; }

        [NonSerialized()]
        public DateTime TimeOfDay;

        public override string ToString()
        {
            return JsonConvert.SerializeObject(this);
        }
    }
}
