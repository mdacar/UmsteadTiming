using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ReaderEventAPI
{
    public class TagRead
    {

        public string TagId { get; set; }
        public int AntennaNumber { get; set; }
        public string TimeStamp { get; set; }
        public string TagType { get; set; }
        public string ReaderName { get; set; }


        public string TimeEntrySource { get; set; }

        public override string ToString()
        {
            return $"TagID: {TagId}\nReaderName: {ReaderName}\nTimeStamp: {TimeStamp}\nTagType: {TagType}";
        }
    }
}
