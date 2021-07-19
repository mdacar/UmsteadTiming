using System;
using System.Collections.Generic;
using System.Text;

namespace TimeEntryFactory
{
    internal class Runner
    {
        public Guid ID { get; set; }
        public Guid RaceID { get; set; }
        public Guid RunnerID { get; set; }
        public int RunnerNumber { get; set; }
        public string State { get; set; }
        public string TagID { get; set; }
        public bool Stopped { get; set; }

    }
}
