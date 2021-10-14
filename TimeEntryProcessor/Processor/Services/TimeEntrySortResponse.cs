using System;
using System.Collections.Generic;
using System.Text;
using TimeEntryProcessor.Processor.Models;

namespace TimeEntryProcessor.Processor.Services
{
    public class TimeEntrySortResponse
    {

        public TimeEntrySortResponse()
        {
            Splits = new List<Split>();
            Errors = new List<string>();
        }

        public IList<Split> Splits { get; set; }
        public IList<string> Errors { get; set; }

    }
}
