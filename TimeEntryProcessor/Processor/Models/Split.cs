namespace TimeEntryProcessor.Processor.Models
{
    public class Split
    {
        public Split()
        {
            NotificationMessageFormat = "{{RunnerName}} just passed {{CheckPointName}} ({{Distance}}) in {{SplitTime}}!";
        }
        public bool SendNotifications { get; internal set; }

        public decimal Distance { get; set; }
        public string NotificationMessageFormat { get; set; }
        public string SplitTimeFormatted { get; set; }

        public string CheckPointName { get; set; }
    }
}