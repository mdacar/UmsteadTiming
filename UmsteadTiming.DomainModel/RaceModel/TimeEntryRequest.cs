﻿using System;

namespace UltimateTiming.DomainModel
{
    public class TimeEntryRequest
    {

        public RFIDReader Reader { get; set; }

        public long TimeStamp { get; set; }

        public DateTime AbsoluteTime { get; set; }

        public string TagId { get; set; }

        public TimeEntrySource Source { get; set; }

        public string TagType { get; set; }
    }
}