using System.Collections.Generic;

namespace UltimateTiming.DomainModel
{
    public class AddTimeEntryResponse
    {

        public AddTimeEntryResponse()
        {
            NewTimeEntry = new List<TimeEntry>();
        }
        public List<TimeEntry> NewTimeEntry { get; set; }
        public Split NewSplit { get; set; }

        public IList<string> Errors { get; set; }

    }
}
