using Newtonsoft.Json;
using System;


namespace ReaderEventAPI
{
    public class TimeEntry
    {
        public string ReaderName { get; set; }
        public string UnixTime { get; set; }
        public int AntennaNumber { get; set; }
        public string TagId { get; set; }


        public override string ToString()
        {
            return JsonConvert.SerializeObject(this);
        }
    }
}
