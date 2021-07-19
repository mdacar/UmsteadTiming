using System;

namespace TimeEntryProcessor.Processor
{
    internal class NotificationRequest
    {

        public Guid RaceXRunnerID { get; set; }
        public string Message { get; set; }
        public string NotificationType { get; set; }


    }
}