using System.Collections.Generic;

namespace UltimateTiming.DomainModel
{


    public class TriviaMaster
    {

        public List<Runner> FastestMale { get; set; }
        public List<Runner> FastestFemale { get; set; }
        public List<Runner> FastestMaleMaster { get; set; }
        public List<Runner> FastestFemaleMaster { get; set; }
        public List<Runner> MostFinishes { get; set; }
        public List<Runner> MostWinsMale { get; set; }
        public List<Runner> MostWinsFemale { get; set; }
        public List<Runner> SlowestWinningTimes { get; set; }
        public List<Runner> YoungestFinisherMale { get; set; }
        public List<Runner> YoungestFinisherFemale { get; set; }
        public List<Runner> OldestFinisherMale { get; set; }
        public List<Runner> OldestFinisherFemale { get; set; }

        public List<FinishCountRunner> NumberOfFinishes { get; set; }

        public List<Runner> PastWinners { get; set; }
        public List<Runner> FiveHundredMileClub { get; set; }

        public List<Runner> HistoryByTime { get; set; }

    }


}
