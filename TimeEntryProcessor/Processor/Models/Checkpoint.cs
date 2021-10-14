namespace TimeEntryProcessor.Processor.Models
{
    public class Checkpoint
    {
        public string ID { get; set; }
        public string Description { get; set; }
        public int Sequence { get; set; }
        public decimal Distance { get; set; }
        public string TimingLocationID { get; set; }
        public int TiminingLocationSequence { get; set; }
        public string ShortName { get; set; }
        public string SMSNotificationText { get; set; }
        public bool SendNotifications { get; set; }
        public bool IsFinalLap { get; set; }

    }
}