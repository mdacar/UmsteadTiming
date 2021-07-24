namespace UltimateTiming.DomainModel
{
    public sealed class ReaderNames
    {

        private readonly string value;

        public static readonly ReaderNames Airport = new ReaderNames("Airport_Reader");
        public static readonly ReaderNames AidStation = new ReaderNames("Aid_Reader");
        public static readonly ReaderNames Headquarters = new ReaderNames("HQ_Reader");

        public ReaderNames(string value)
        {
            this.value = value;
        }

        public override string ToString()
        {
            return value;
        }

        public bool IsEqual(string val)
        {
            return string.Compare(value, val.ToString(), true) == 0;
        }


    }
}
