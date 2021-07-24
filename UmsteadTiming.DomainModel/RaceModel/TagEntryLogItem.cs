namespace UltimateTiming.DomainModel
{
    public class TagEntryLogItem
    {

        public int Id { get; set; }
        public RFIDReader Reader { get; set; }
        public string TagId { get; set; }
        public string Timestamp { get; set; }

    }
}
