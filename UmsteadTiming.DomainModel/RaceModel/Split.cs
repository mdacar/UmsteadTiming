using System;
using UltimateTiming.DomainModel.Utility;

namespace UltimateTiming.DomainModel
{
    public class Split : RaceObject
    {

        public Split() : base()
        {

        }

        public Split(string id) : base(id)
        {

        }


        private CheckPoint _checkPoint;

        public CheckPoint CheckPoint
        {
            get { return _checkPoint; }
            set
            {
                if (_checkPoint != value)
                {
                    _checkPoint = value;
                    CheckPointId = _checkPoint.Id;
                    SetPropertyChanged();
                }
            }
        }


        private DateTime _splitTime;

        public DateTime SplitTime
        {
            get { return _splitTime; }
            set
            {
                if (_splitTime != value)
                {
                    _splitTime = value;
                    SetPropertyChanged();
                }
            }
        }

        private long _elapsedMilliseconds;
        public long ElapsedMilliseconds
        {
            get { return _elapsedMilliseconds; }
            set
            {
                if (value != _elapsedMilliseconds)
                {
                    _elapsedMilliseconds = value;
                    SetPropertyChanged();
                }
            }

        }

        public string ElapsedTime
        {
            get
            {
                return MillisecondConverter.GetVerboseTimeFromMilliseconds(ElapsedMilliseconds);
            }
        }

        public string ElapsedTimeShort
        {
            get
            {
                return MillisecondConverter.GetTimeFromMilliseconds(ElapsedMilliseconds);
            }
        }

        private string _raceId;
        public string RaceId
        {
            get { return _raceId; }
            set
            {
                if (value != _raceId)
                {
                    _raceId = value;
                    SetPropertyChanged();
                }
            }

        }


        public Runner Runner { get; set; }

        public string RaceXRunnerId { get; set; }

        public string CheckPointId { get; set; }


        public int CheckPointSequence
        {
            get
            {
                if (CheckPoint != null)
                {
                    return CheckPoint.Sequence;
                }
                return -1;
            }
        }

        public decimal TotalMiles
        {
            get
            {
                if (CheckPoint != null)
                {
                    return CheckPoint.TotalMiles;
                }
                return -1M;
            }
        }



        public override string ToString()
        {
            return ElapsedTime;
        }

        public TimeEntry TimeEntry { get; set; }

        public TimeSpan OverallPace { get; set; }
        public TimeSpan SplitPace { get; set; }

        public TimeSpan SplitElapsedTime { get; set; }
        public int PlaceAtCheckPoint { get; internal set; }

    }
}
