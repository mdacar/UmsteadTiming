using System.Collections.Generic;

namespace UltimateTiming.DomainModel.Sorting
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
