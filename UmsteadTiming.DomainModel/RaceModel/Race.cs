using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using UltimateTiming.DomainModel.Sorting;


namespace UltimateTiming.DomainModel
{


    public class Race : RaceObject
    {



        #region Constructors
        public Race()
        {
            InitializeCollections();
        }



        public Race(string id) : base(id)
        {
            InitializeCollections();
        }


        private void InitializeCollections()
        {
            this.Runners = new ObservableCollection<Runner>();
            this.CheckPoints = new ObservableCollection<CheckPoint>();
            this.TimingLocations = new ObservableCollection<TimingLocation>();
            this.AgeGroups = new ObservableCollection<AgeGroup>();
            _timeEntrySources = new List<TimeEntrySource>();
            RFIDReaders = new List<RFIDReader>();
        }
        #endregion


        #region Properties
        public int RaceOrganizationId { get; set; }

        private string _raceName;
        public string Name
        {
            get { return _raceName; }
            set
            {
                if (_raceName != value)
                {
                    _raceName = value;
                    SetPropertyChanged();
                    SetPropertyChanged("DisplayName");
                }
            }
        }



        private ObservableCollection<Runner> _runners;

        public ObservableCollection<Runner> Runners
        {
            get
            {
                return _runners;
            }
            private set
            {
                if (_runners != value)
                {
                    _runners = value;
                    SetPropertyChanged();
                }
            }
        }



        private DateTime _startDate;
        public DateTime StartDate
        {
            get { return _startDate; }
            set
            {
                if (_startDate != value)
                {
                    _startDate = value;
                    SetPropertyChanged();
                    SetPropertyChanged("DisplayName");
                }
            }
        }



        private ObservableCollection<CheckPoint> _checkPoints;

        public ObservableCollection<CheckPoint> CheckPoints
        {
            get { return _checkPoints; }
            private set
            {
                if (_checkPoints != value)
                {
                    _checkPoints = value;
                    SetPropertyChanged();
                }
            }
        }


        private ObservableCollection<TimingLocation> _timingLocations;

        public ObservableCollection<TimingLocation> TimingLocations
        {
            get { return _timingLocations; }
            private set
            {
                if (_timingLocations != value)
                {
                    _timingLocations = value;
                    SetPropertyChanged();
                }
            }
        }


        private ObservableCollection<AgeGroup> _ageGroups;

        public ObservableCollection<AgeGroup> AgeGroups
        {
            get { return _ageGroups; }
            set { _ageGroups = value; }
        }


        public string DisplayName
        {
            get
            {
                return StartDate.Year.ToString() + " " + Name + " - " + StartDate.ToShortDateString();
            }
        }

        public string ShortName { get; set; }

        public bool Completed { get; set; }

        public override bool IsDirty()
        {
            return IsThisDirty
                || Runners.Any(r => r.IsDirty())
                || CheckPoints.Any(c => c.IsDirty())
                || this.TimingLocations.Any(t => t.IsDirty());
        }

        public int? RaceID { get; set; }


        private List<TimeEntrySource> _timeEntrySources;

        public ReadOnlyCollection<TimeEntrySource> TimeEntrySources
        {
            get
            {
                return _timeEntrySources.AsReadOnly();
            }
        }


        public List<RFIDReader> RFIDReaders { get; set; }


        public bool UseStartSplit { get; set; }
        #endregion




        #region Runner Related

        private Runner FindRunner(string tagId)
        {
            var runner = Runners.Where(r => r.TagId == tagId).FirstOrDefault();
            if (runner == null)
            {
                throw new RunnerNotFoundException(tagId);
            }

            return runner;
        }



        public void AddRunner(Runner runner)
        {
            if (!Runners.Any(r => r.TagId == runner.TagId))
            {
                runner.AgeOnRaceday = GetRunnerAge(runner);
                runner.Race = this;
                Runners.Add(runner);
                IsThisDirty = true;
            }
        }

        private int GetRunnerAge(Runner runner)
        {
            int age = StartDate.Year - runner.DateOfBirth.Year;
            if (StartDate.Month < runner.DateOfBirth.Month || (StartDate.Month == runner.DateOfBirth.Month && StartDate.Day < runner.DateOfBirth.Day)) age--;
            return age;
        }

        public void AddRunners(IEnumerable<Runner> runners)
        {
            foreach (var runner in runners)
            {
                runner.Race = this;
                runner.AgeOnRaceday = GetRunnerAge(runner);
            }
            Runners = new ObservableCollection<Runner>(runners);
            IsThisDirty = true;
        }


        private ITimeEntrySorter GetTimeEntrySorter()
        {
            return new TimeEntrySorter(TimingLocations, CheckPoints);
        }

        public IList<string> SortAllRunnerTimeEntries()
        {
            var errors = new List<string>();
            var sorter = GetTimeEntrySorter();
            if (!Completed)
            {
                foreach (var runner in Runners)
                {
                    errors.AddRange(runner.SortTimeEntries(sorter));
                }
            }
            return errors;
        }

        #endregion


        #region TimeEntry Related

        public AddTimeEntryResponse AddTimeEntry(TimeEntryRequest request)
        {
            var response = new AddTimeEntryResponse();


            try
            {

                //Always use the start of the race.  No need to do an initial start split at this time

                var runner = FindRunner(request.TagId);

                var startSplit = runner.GetStartSplit();
                if (startSplit == null && request.Reader.ReaderName != RFIDReader.HQ_READER_NAME) //If there's no start split available then add it here - should only happen if we missed the runner at the start
                {
                    var startEntry = runner.AddTimeEntry(new TimeEntry()
                    {
                        Id = Guid.NewGuid().ToString(),
                        AbsoluteTime = StartDate,
                        RaceXRunnerId = runner.RaceXRunnerId,
                        ReaderTimestamp = long.Parse(UnixTimeConverter.GetUnixTime(StartDate)),
                        ElapsedTime = 0,
                        Status = TimeEntryStatus.Unknown,
                        Reader = RFIDReaders.Where(r => r.ReaderName == RFIDReader.HQ_READER_NAME).FirstOrDefault(),
                        Source = TimeEntrySources.Where(t => t.Description == "RFID Reader").FirstOrDefault(),
                        TagType = GetTagType(request.TagType)
                    });
                    runner.SortTimeEntries(GetTimeEntrySorter());
                    response.NewTimeEntry.Add(startEntry);
                }

                var elapsedTime = GetElapsedTime(runner, request.AbsoluteTime);

                var timeEntry = runner.AddTimeEntry(new TimeEntry()
                {
                    Id = Guid.NewGuid().ToString(),
                    AbsoluteTime = request.AbsoluteTime,
                    RaceXRunnerId = runner.RaceXRunnerId,
                    ReaderTimestamp = request.TimeStamp,
                    ElapsedTime = (long)elapsedTime,
                    Status = TimeEntryStatus.Unknown,
                    Reader = request.Reader,
                    Source = request.Source,
                    TagType = GetTagType(request.TagType)
                });
                response.NewTimeEntry.Add(timeEntry);
                response.Errors = runner.SortTimeEntries(GetTimeEntrySorter());

                //SortRunnersIntoPlaces();

                response.NewSplit = runner.Splits.Where(s => s.TimeEntry.Id == timeEntry.Id).FirstOrDefault();
            }
            catch (RunnerNotFoundException)
            {
                response.Errors.Add($"Tag ID {request.TagId} was not found");
            }
            catch (Exception e)
            {
                response.Errors.Add($"Error adding time entry for {request.TagId}: {e.Message}");
            }


            return response;
        }

        private double GetElapsedTime(Runner runner, DateTime absoluteTime)
        {
            double elapsedTime = 0;
            if (UseStartSplit)
            {
                Split startSplit = runner.GetStartSplit();
                if (startSplit != null)
                {
                    elapsedTime = (absoluteTime - startSplit.SplitTime).TotalMilliseconds;

                }
            }
            else
            {
                elapsedTime = (absoluteTime - this.StartDate).TotalMilliseconds;
            }

            return elapsedTime;
        }

        private string GetTagType(string tagType)
        {
            string year = DateTime.Now.Year.ToString();
            if (tagType == "P" || tagType == year)
            {
                return "P";
            }
            else
            {
                return "B";
            }
        }

        public void SortRunnersIntoPlaces()
        {
            //Overall Place
            var orderedRunners = Runners.Where(r => r.Splits.Any())
                .OrderByDescending(r => r.LastSplit.TotalMiles)
                .ThenBy(r => r.LastSplit.ElapsedMilliseconds);

            int i = 1;
            foreach (var runner in orderedRunners)
            {
                runner.OverallPlace = i++;
                runner.Place = runner.OverallPlace;
            }

            //Sex Place
            var maleRunners = Runners.Where(r => r.Splits.Any() && r.Sex == "M")
                .OrderByDescending(r => r.LastSplit.TotalMiles)
                .ThenBy(r => r.LastSplit.ElapsedMilliseconds);

            i = 1;
            foreach (var runner in maleRunners)
            {
                runner.SexPlace = i++;
            }

            var female = Runners.Where(r => r.Splits.Any() && r.Sex == "F")
                .OrderByDescending(r => r.LastSplit.TotalMiles)
                .ThenBy(r => r.LastSplit.ElapsedMilliseconds);

            i = 1;
            foreach (var runner in female)
            {
                runner.SexPlace = i++;
            }

            //Age Group PLace
            foreach (var ageGroup in AgeGroups)
            {
                var sortedAgeGroupRunners = ageGroup.Runners.Where(r => r.Splits.Any())
                    .OrderByDescending(r => r.LastSplit.TotalMiles)
                    .ThenBy(r => r.LastSplit.ElapsedMilliseconds);
                i = 1;
                foreach (var runner in sortedAgeGroupRunners)
                {
                    runner.AgeGroupPlace = i++;
                }
            }
        }

        public TimeEntry DeleteTimeEntry(Runner runner, TimeEntry timeEntry)
        {
            var deletedTimeEntry = runner.DeleteTimeEntry(timeEntry);
            runner.SortTimeEntries(GetTimeEntrySorter());
            return deletedTimeEntry;
        }


        public IList<string> MergeTimeEntries(IEnumerable<TimeEntry> timeEntries)
        {
            var errors = new List<string>();
            var timeEntriesByRunner = timeEntries.GroupBy(t => t.RaceXRunnerId);
            foreach (var runnerGrouping in timeEntriesByRunner)
            {
                var runner = this.Runners.Where(r => r.RaceXRunnerId == runnerGrouping.Key).FirstOrDefault();
                if (runner != null)
                {
                    foreach (var timeEntry in runnerGrouping)
                    {
                        //Check if it's already in the list, if so skip adding again
                        //TODO Make sure this actually works
                        var foundEntry = runner.TimeEntries.Where(t => t.Id == timeEntry.Id
                            || (t.ReaderTimestamp == timeEntry.ReaderTimestamp && t.RaceXRunnerId == timeEntry.RaceXRunnerId)).FirstOrDefault();
                        if (foundEntry != null)
                        {
                            foundEntry.Status = timeEntry.Status;
                            foundEntry.StatusReason = timeEntry.StatusReason;
                        }
                        else
                        {
                            runner.AddTimeEntry(timeEntry);
                        }
                    }

                    errors.AddRange(runner.SortTimeEntries(GetTimeEntrySorter()));
                }
            }

            //SortRunnersIntoPlaces();

            return errors;
        }

        #endregion



        #region TimingLocation Related

        public void AddTimingLocation(TimingLocation timingLocation)
        {
            if (string.IsNullOrWhiteSpace(timingLocation.RaceId))
            {
                timingLocation.RaceId = Id;
            }
            TimingLocations.Add(timingLocation);
            IsThisDirty = true;
        }

        public void RemoveTimingLocation(TimingLocation timingLocation)
        {
            TimingLocations.Remove(timingLocation);
        }

        public void AddTimingLocations(IEnumerable<TimingLocation> timingLocations)
        {
            foreach (var tl in timingLocations)
            {
                AddTimingLocation(tl);
            }
        }

        #endregion


        #region Checkpoint Related

        public void AddCheckPoint(CheckPoint checkPoint)
        {
            if (string.IsNullOrWhiteSpace(checkPoint.RaceId))
            {
                checkPoint.RaceId = Id;
            }
            if (checkPoint.Sequence == 0)
            {
                checkPoint.Sequence = GetNextCheckPointSequence();
            }

            CheckPoints.Add(checkPoint);
            CheckPoints = new ObservableCollection<CheckPoint>(CheckPoints.OrderBy(c => c.Sequence));
            IsThisDirty = true;
        }

        private int GetNextCheckPointSequence()
        {
            if (CheckPoints.Count > 0)
            {
                return CheckPoints.Max(cp => cp.Sequence) + 1;
            }
            else
            {
                return 0;
            }
        }

        public void AddCheckPoints(IEnumerable<CheckPoint> checkPoints)
        {
            foreach (var cp in checkPoints)
            {
                if (cp.TimingLocation == null)
                {
                    cp.TimingLocation = TimingLocations.Where(tl => tl.Id == cp.TimingLocationId).FirstOrDefault();
                }
            }
            CheckPoints = new ObservableCollection<CheckPoint>(checkPoints.OrderBy(c => c.Sequence));
            IsThisDirty = true;
        }

        #endregion


        #region AgeGroup Related

        public List<AgeGroup> SortRunnersIntoAgeGroups()
        {
            var list = new List<AgeGroup>(AgeGroups);

            foreach (var runner in Runners)
            {
                var ageGroup = list.Where(a => runner.AgeOnRaceday >= a.MinimumAge && runner.AgeOnRaceday <= a.MaximumAge && string.Compare(runner.Sex, a.Sex, true) == 0).FirstOrDefault();
                if (ageGroup == null)
                {
                    //throw new IndexOutOfRangeException($"No age group specified for {runner.FullName}, age {runner.AgeOnRaceday}");
                }
                else
                {
                    runner.AgeGroup = ageGroup.Description;
                    if (!ageGroup.Runners.Any(r => r.RunnerId == runner.RunnerId))
                    {
                        ageGroup.Runners.Add(runner);
                    }

                }
            }

            return list.ToList();
        }



        public void AddAgeGroups(IEnumerable<AgeGroup> ageGroups)
        {
            foreach (var ageGroup in ageGroups)
            {
                AddAgeGroup(ageGroup);
            }
        }

        public void AddAgeGroup(AgeGroup ageGroup)
        {
            string invalidMessage = null;
            foreach (var ag in AgeGroups)
            {
                //Check to see if the minimum age is valid
                if (string.Compare(ag.Sex, ageGroup.Sex, true) == 0)
                {
                    if (ageGroup.MinimumAge >= ag.MinimumAge && ageGroup.MinimumAge <= ag.MaximumAge)
                    {
                        invalidMessage = string.Format("Minimum age conflicts with age group {0} - {1}: Minimum {2}, Maximum {3}", ag.Id, ag.Description, ag.MinimumAge, ag.MaximumAge);
                    }

                    if (ageGroup.MaximumAge <= ag.MaximumAge && ageGroup.MaximumAge >= ag.MinimumAge)
                    {
                        invalidMessage = string.Format("Maximum age conflicts with age group {0} - {1}: Minimum {2}, Maximum {3}", ag.Id, ag.Description, ag.MinimumAge, ag.MaximumAge);
                    }
                }

            }


            if (!string.IsNullOrWhiteSpace(invalidMessage))
            {
                throw new ArgumentException(invalidMessage, "ageGroup");
            }
            else
            {
                ageGroup.RaceID = Id;
                AgeGroups.Add(ageGroup);
            }
        }

        public void SetPlaces()
        {
            PlaceCalculation.PlaceCalculator calc = new PlaceCalculation.PlaceCalculator();
            calc.CalculatePlaces(this);
        }

        #endregion


        #region TimingEntrySource related
        public void AddTimeEntrySources(IEnumerable<TimeEntrySource> sources)
        {
            foreach (var source in sources)
            {
                //Make sure the race id is there
                if (string.IsNullOrWhiteSpace(source.RaceId))
                {
                    source.RaceId = Id;
                }
                //Don't add duplicates
                if (!_timeEntrySources.Any(tes => tes.Id == source.Id))
                {
                    _timeEntrySources.Add(source);
                }
            }
        }

        #endregion


        #region Counts

        public int OneHundredMileFinishers
        {
            get
            {
                return Runners.Where(r => r.IsFinished()).Count();
            }
        }

        public int StoppedRunnersCount
        {
            get
            {
                return Runners.Where(r => r.Stopped).Count();
            }
        }

        public int StillOnCourseCount
        {
            get
            {
                return Runners.Where(r => r.Status == RunnerStatus.StillOnCourse).Count();
            }
        }

        public int UnaccountedForCount
        {
            get
            {
                return Runners.Where(r => r.Status == RunnerStatus.StillOnCourse || r.Status == RunnerStatus.ProbablyStopped).Count();
            }
        }

        public int TotalStarters
        {
            get
            {
                return Runners.Where(r => r.Splits.Any()).Count();
            }
        }

        public int TotalTextMessageSignups { get; set; }

        #endregion

        #region Leaders/Winners

        public Runner FirstMale
        {
            get
            {
                return Runners.Where(r => r.Sex == "M" && r.SexPlace == 1 && r.IsFinished()).FirstOrDefault();
            }
        }

        public Runner FirstFemale
        {
            get
            {
                return Runners.Where(r => r.Sex == "F" && r.SexPlace == 1 && r.IsFinished()).FirstOrDefault();
            }
        }

        public Runner FirstMaleMaster
        {
            get
            {
                return Runners.Where(r => r.Sex == "M" && r.AgeOnRaceday >= 40 && r.IsFinished()).OrderBy(r => r.SexPlace).First();
            }
        }

        public Runner FirstFemaleMaster
        {
            get
            {
                return Runners.Where(r => r.Sex == "F" && r.AgeOnRaceday >= 40 && r.IsFinished()).OrderBy(r => r.SexPlace).First();
            }
        }
        #endregion

    }
}
