using System;

namespace UltimateTiming.DomainModel
{
    public class CheckPoint : RaceObject
    {

        public CheckPoint(string id) : base(id) { }

        public CheckPoint() : base() { }


        private string _shortName;

        public string ShortName
        {
            get { return _shortName; }
            set
            {
                if (_shortName != value)
                {
                    _shortName = value;
                    SetPropertyChanged();
                }
            }
        }


        private string _longName;

        public string LongName
        {
            get { return _longName; }
            set
            {
                if (_longName != value)
                {
                    _longName = value;
                    SetPropertyChanged();
                }
            }
        }

        private decimal _totalMiles;

        public decimal TotalMiles
        {
            get { return _totalMiles; }
            set
            {
                if (value != _totalMiles)
                {
                    _totalMiles = value;
                    SetPropertyChanged();
                }
            }

        }



        private TimeSpan _minTime;
        public TimeSpan MinTime
        {
            get { return _minTime; }
            set
            {
                if (value != _minTime)
                {
                    _minTime = value;
                    SetPropertyChanged();
                }
            }

        }



        public TimeSpan MaxTime { get; set; }
        public string NextCheckpoint { get; set; }


        private int _sequence;
        public int Sequence
        {
            get { return _sequence; }
            set
            {
                if (value != _sequence)
                {
                    _sequence = value;
                    SetPropertyChanged();
                }
            }

        }


        public int TimingLocationSequence { get; set; }


        public string RaceId { get; set; }


        private TimingLocation _timingLocation;

        public TimingLocation TimingLocation
        {
            get { return _timingLocation; }
            set
            {
                _timingLocation = value;
            }
        }

        public string TimingLocationId { get; set; }



        private bool _sendNotifications;

        public bool SendNotifications
        {
            get { return _sendNotifications; }
            set { _sendNotifications = value; }
        }

        public string SMSNotificationText { get; set; }
        public bool FinalLap { get; internal set; }

    }
}
