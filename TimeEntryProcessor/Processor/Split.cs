namespace TimeEntryProcessor.Processor
{
    internal class Split
    {
        public bool SendNotifications { get; internal set; }

        public decimal Distance { get; set; }
        public string NotificationMessageFormat { get; set; }
        public string SplitTimeFormatted { get; set; }

        public Runner Runner { get; set; }
        public string CheckPointName { get; set; }
    }
}