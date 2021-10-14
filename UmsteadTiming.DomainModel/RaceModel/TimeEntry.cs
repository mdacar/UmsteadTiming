using Newtonsoft.Json;
using System;
using System.Security.Cryptography;
using System.Text;

namespace UltimateTiming.DomainModel
{
    public class TimeEntry : RaceObject
    {

        public TimeEntry() : base()
        {

        }

        public TimeEntry(string id) : base(id)
        {
        }


        public string TagType { get; set; }


        private string _raceXRunnerId;
        public string RaceXRunnerId
        {
            get { return _raceXRunnerId; }
            set
            {
                if (value != _raceXRunnerId)
                {
                    _raceXRunnerId = value;
                }
            }

        }



        private Int64 _elapsedTime;
        public Int64 ElapsedTime
        {
            get { return _elapsedTime; }
            set
            {
                if (value != _elapsedTime)
                {
                    _elapsedTime = value;
                }
            }

        }


        private DateTime _absoluteTime;
        public DateTime AbsoluteTime
        {
            get { return _absoluteTime; }
            set
            {

                _absoluteTime = value;
            }

        }


        private Int64 _readerTimestamp;
        public Int64 ReaderTimestamp
        {
            get { return _readerTimestamp; }
            set
            {
                if (value != _readerTimestamp)
                {
                    _readerTimestamp = value;
                }
            }

        }


        private RFIDReader _reader;
        public RFIDReader Reader
        {
            get { return _reader; }
            set
            {
                if (value != _reader)
                {
                    _reader = value;
                }
            }
        }


        private TimeEntryStatus _status;
        [JsonProperty("TimeEntryStatusID")]
        public TimeEntryStatus Status
        {
            get { return _status; }
            set
            {
                if (value != _status)
                {
                    _status = value;
                }
            }
        }


        private string _statusReason;
        public string StatusReason
        {
            get { return _statusReason; }
            set
            {
                if (value != _statusReason)
                {
                    _statusReason = value;
                }
            }

        }


        private TimeEntrySource _timeEntrySource;
        public TimeEntrySource Source
        {
            get { return _timeEntrySource; }
            set
            {
                if (value != _timeEntrySource)
                {
                    _timeEntrySource = value;
                }
            }

        }


        public int TimeEntrySourceId { get; set; }
        public int RFIDReaderId { get; set; }


        public Split Split { get; set; }

        public DateTime UpdatedDate { get; set; }
        public string UpdatedDateUnixTime
        {
            get
            {
                return UnixTimeConverter.GetUnixTime(UpdatedDate);
            }
        }

        public string CheckSum
        {
            get
            {
                using (var md5 = MD5.Create())
                {
                    string objectData = $"{Id}{Status}{StatusReason}{TimeEntrySourceId}{RFIDReaderId}{RaceXRunnerId}{ElapsedTime}{AbsoluteTime}{UpdatedDate}";
                    return Encoding.ASCII.GetString(md5.ComputeHash(Encoding.ASCII.GetBytes(objectData)));
                }
            }
        }

        public string ReaderName { get; set; }
    }
}
