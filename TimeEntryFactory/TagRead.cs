using System;
using System.Collections.Generic;
using System.Text;

namespace TimeEntryFactory
{
    public class TagRead
    {
        public string TagId { get; set; }
        public int AntennaNumber { get; set; }
        public string TimeStamp { get; set; }
        public string TagType { get; set; }
        public string ReaderName { get; set; }


        public string TimeEntrySource { get; set; }
    }
}
