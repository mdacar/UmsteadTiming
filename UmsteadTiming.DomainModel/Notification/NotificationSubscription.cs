using System;
using System.ComponentModel.DataAnnotations;

namespace UltimateTiming.DomainModel.Notification
{
    public class NotificationSubscription
    {
        [Key]
        public Guid ID { get; set; }
        public NotificationType NotificationType { get; set; }

        public string NotificationAddress { get; set; }

        public Guid RaceXRunnerId { get; set; }
        public int NotificationTypeID { get { return (int)NotificationType; } }
    }
}
