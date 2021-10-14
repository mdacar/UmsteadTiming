using System;
using System.Collections.Generic;

namespace TimeEntryProcessor.Processor.Models
{
    internal class AddTimeEntryResponse
    {
        public AddTimeEntryResponse()
        {
            Exceptions = new List<Exception>();
        }

        public Split NewSplit { get; internal set; }
        public Runner Runner { get; set; }
        public List<Exception> Exceptions { get; set; }
        public string UpdatedTimeEntryStatusReason { get; set; }
        public TimeEntryStatus UpdatedTimeEntryStatus { get; set; }
    }
}