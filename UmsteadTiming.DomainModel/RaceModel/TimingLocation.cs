namespace UltimateTiming.DomainModel
{
    public class TimingLocation : RaceObject
    {

        public TimingLocation() : base()
        {
        }

        public TimingLocation(string id) : base(id)
        {
        }


        private string _description;
        public string Description
        {
            get { return _description; }
            set
            {
                if (value != _description)
                {
                    _description = value;
                    SetPropertyChanged();
                }
            }

        }


        private string _code;
        public string Code
        {
            get { return _code; }
            set
            {
                if (value != _code)
                {
                    _code = value;
                    SetPropertyChanged();
                }
            }

        }

        public string RaceId { get; set; }
    }
}
