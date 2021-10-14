using System.Collections.Generic;

namespace UltimateTiming.DomainModel
{
    public class AgeGroup : RaceObject
    {

        public AgeGroup() : base()
        {
            Runners = new List<Runner>();
        }

        public AgeGroup(string id) : base(id)
        {
            Runners = new List<Runner>();
        }

        private string _raceId;
        public string RaceID
        {
            get { return _raceId; }
            set
            {
                if (_raceId != value)
                {
                    _raceId = value;
                    SetPropertyChanged();
                }
            }
        }


        private int _minimumAge;
        public int MinimumAge
        {
            get { return _minimumAge; }
            set
            {
                if (_minimumAge != value)
                {
                    _minimumAge = value;
                    SetPropertyChanged();
                }
            }
        }


        private int _maximumAge;
        public int MaximumAge
        {
            get { return _maximumAge; }
            set
            {
                if (_maximumAge != value)
                {
                    _maximumAge = value;
                    SetPropertyChanged();
                }
            }
        }


        private string _description;
        public string Description
        {
            get { return _description; }
            set
            {
                if (_description != value)
                {
                    _description = value;
                    SetPropertyChanged();
                }
            }
        }


        private string _sex;
        public string Sex
        {
            get { return _sex; }
            set
            {
                if (_sex != value)
                {
                    _sex = value;
                    SetPropertyChanged();
                }
            }
        }

        public List<Runner> Runners { get; set; }

    }
}
