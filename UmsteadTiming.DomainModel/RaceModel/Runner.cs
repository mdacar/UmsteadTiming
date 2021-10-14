using System;
using System.Collections.Generic;
using System.Linq;
using UltimateTiming.DomainModel.Sorting;

namespace UltimateTiming.DomainModel
{
    public class Runner : RaceObject
    {

        public Runner() : base()
        {
            Initialize();
        }

        public Runner(string id) : base(id)
        {
            Initialize();
        }

        private void Initialize()
        {
            _primaryTimeEntries = new List<TimeEntry>();
            _splits = new List<Split>();
        }

        public Race Race { get; set; }

        private string _firstName;
        public string FirstName
        {
            get { return _firstName; }
            set
            {
                if (_firstName != value)
                {
                    _firstName = value;
                    
                }
            }
        }

        private string _lastName;
        public string LastName
        {
            get { return _lastName; }
            set
            {
                if (_lastName != value)
                {
                    _lastName = value;
                    
                }
            }
        }



        private int _number;
        public int Number
        {
            get { return _number; }
            set
            {
                if (_number != value)
                {
                    _number = value;
                    
                }
            }
        }


        private string _tagId;
        public string TagId
        {
            get { return _tagId; }
            set
            {
                if (value != _tagId)
                {
                    _tagId = value;
                    
                }
            }
        }

        private string _runnerId;
        public string RunnerId
        {
            get { return _runnerId; }
            set
            {
                if (_runnerId != value)
                {
                    _runnerId = value;
                    
                }
            }
        }


        private DateTime _dateOfBirth;

        public DateTime DateOfBirth
        {
            get { return _dateOfBirth; }
            set
            {
                if (_dateOfBirth != value)
                {
                    _dateOfBirth = value;
                    
                }
            }
        }



        private List<Split> _splits;
        public IReadOnlyCollection<Split> Splits
        {
            get
            {
                var paceCalc = new PaceCalculation.PaceCalculator();
                _splits = paceCalc.CalculatePace(_splits).ToList();
                return _splits.OrderByDescending(s => s.ElapsedMilliseconds).ToList().AsReadOnly();
            }
            private set { _splits = new List<Split>(value); }
        }

        public Split GetSplit(string checkpointId)
        {
            return _splits.Where(s => s.CheckPointId == checkpointId).FirstOrDefault();
        }


        public Split LastSplit
        {
            get
            {
                if (_splits != null)
                {
                    if (_splits.Count > 0)
                    {
                        return _splits.Where(s => s.ElapsedMilliseconds == _splits.Max(s2 => s2.ElapsedMilliseconds)).FirstOrDefault();
                    }
                }
                return null;
            }
        }


        private List<TimeEntry> _primaryTimeEntries;


        public List<TimeEntry> TimeEntries
        {
            get
            {
                return new List<TimeEntry>(_primaryTimeEntries);                
            }
        }


        private int _ageOnRaceday;
        public int AgeOnRaceday
        {
            get { return _ageOnRaceday; }
            set
            {
                if (_ageOnRaceday != value)
                {
                    _ageOnRaceday = value;
                }
            }
        }



        private string _city;
        public string City
        {
            get { return _city; }
            set
            {
                if (_city != value)
                {
                    _city = value;
                    
                }
            }
        }



        private string _state;
        public string State
        {
            get { return _state; }
            set
            {
                if (_state != value)
                {
                    _state = value;
                    
                }
            }
        }



        public string RaceXRunnerId { get; set; }



        private string _middleInitial;
        public string MiddleInitial
        {
            get { return _middleInitial; }
            set
            {
                if (value != _middleInitial)
                {
                    _middleInitial = value;
                    
                }
            }

        }



        private string _sex;
        public string Sex
        {
            get { return _sex; }
            set
            {
                if (value != _sex)
                {
                    _sex = value;
                    
                }
            }

        }


        private string _fullName;
        public string FullName
        {
            get
            {
                if (string.IsNullOrWhiteSpace(_fullName))
                {
                    _fullName = (FirstName + " " + LastName).Trim();
                }
                return _fullName.Replace("_", string.Empty);
            }
            set
            {
                if (value != _fullName)
                {
                    _fullName = value;
                    
                }
            }

        }



        internal Split GetStartSplit()
        {
            var startSplit = Splits.Where(s => s.TotalMiles == 0).FirstOrDefault();
            return startSplit;
        }

        private bool _stopped;
        public bool Stopped
        {
            get { return _stopped; }
            set
            {
                if (value != _stopped)
                {
                    _stopped = value;
                    

                }
            }
        }

        private int _past100MileFinishes;
        public int Past100MileFinishes
        {
            get { return _past100MileFinishes; }
            set
            {
                if (value != _past100MileFinishes)
                {
                    _past100MileFinishes = value;
                    

                }
            }
        }

        private int _past50MileFinishes;
        public int Past50MileFinishes
        {
            get { return _past50MileFinishes; }
            set
            {
                if (value != _past50MileFinishes)
                {
                    _past50MileFinishes = value;
                    

                }
            }
        }

        //public Split LastSplit
        //{
        //    get { return Splits.OrderByDescending(s => s.CheckPoint.Sequence).FirstOrDefault(); }
        //}

        public string Pace
        {
            get
            {
                //try
                //{
                //    if (LastSplit != null)
                //    {
                //        var lastSplit = LastSplit;

                //        return MillisecondConverter.GetVerboseTimeFromMilliseconds(Convert.ToInt64(lastSplit.ElapsedMilliseconds / lastSplit.CheckPoint.TotalMiles));
                //    }
                //}
                //catch { }

                return string.Empty;
            }
        }

        public int OverallPlace { get; set; }
        public int AgeGroupPlace { get; set; }
        public int SexPlace { get; set; }


        public TimeEntry AddTimeEntry(TimeEntry timeEntry)
        {
            if (!_primaryTimeEntries.Any(t => t.ReaderTimestamp == timeEntry.ReaderTimestamp))
            {
                //only add to the list if it's not already there - using the timestamp as a unique identifier
                _primaryTimeEntries.Add(timeEntry);
            }

            return timeEntry;
        }


        public TimeEntry DeleteTimeEntry(TimeEntry timeEntry)
        {
            timeEntry.Status = TimeEntryStatus.Invalid;
            return timeEntry;
        }

        public IList<string> SortTimeEntries(ITimeEntrySorter sorter)
        {
            //ResetAllTimeEntries();
            var response = sorter.Sort(this);
            Splits = response.Splits.ToList().AsReadOnly();
            return response.Errors;
        }

        public RunnerStatus Status
        {
            get
            {
                if (DateTime.Now.Subtract(Race.StartDate).TotalHours > 26 && Started() && !OnFinalLap() && !IsFinished())
                {
                    Stopped = true;
                    return RunnerStatus.Stopped;
                }

                if (Stopped)
                {
                    return RunnerStatus.Stopped;
                }

                if (IsFinished())
                {
                    return RunnerStatus.Finished;
                }

                if (!Started())
                {
                    return RunnerStatus.DidNotStart;
                }

                if (DateTime.UtcNow.Subtract(TimeEntries.Max(t => t.AbsoluteTime)).TotalHours > 3)
                {
                    return RunnerStatus.ProbablyStopped;
                }

                return RunnerStatus.StillOnCourse;
            }
        }

        public bool OnFinalLap()
        {
            //Check if the race has a final lap specified and if the runner has any splits (don't care about people who haven't started)
            if (Race.CheckPoints.Any(c => c.FinalLap) && Splits.Any())
            {
                return Splits.Any(s => s.CheckPoint.FinalLap) && !IsFinished();
            }
            //If there isn't a lap 7 then I don't know what 
            return false;

        }

        public bool IsFinished()
        {
            if (Race.CheckPoints.Any(c => c.TotalMiles == 100M))
            {
                return Splits.Any(s => s.TotalMiles >= 100M);
            }
            return false;
        }

        public bool FiftyMileFinisher
        {
            get
            {
                return Splits.Any(s => s.TotalMiles == 50M) && !Splits.Any(s => s.TotalMiles == 100M);
            }
        }


        public Split FiftyMileSplit
        {
            get
            {
                return Splits.Where(s => s.TotalMiles == 50M).FirstOrDefault();
            }
        }


        private bool Started()
        {
            return TimeEntries.Count > 0;
        }


        public override string ToString()
        {
            return LastName + ", " + FirstName;
        }


        public void AddTimeEntries(IList<TimeEntry> list)
        {
            foreach (var entry in list)
            {
                AddTimeEntry(entry);
            }
        }

        public void Stop()
        {
            this.Stopped = true;
        }

        public void Unstop()
        {
            this.Stopped = false;
        }



        public int Place { get; set; }
        public string AgeGroup { get; set; }
        public string PostalCode { get; set; }
        public string Address1 { get; set; }

        public void ClearTimeEntries()
        {
            _primaryTimeEntries.Clear();
        }
    }
}
