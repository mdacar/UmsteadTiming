namespace UltimateTiming.DomainModel.Notification
{
    public interface INotificationManager
    {

        void ProcessRaceEvent(Split split);

        DomainModel.Notification.NotificationSubscription Subscribe(DomainModel.Notification.NotificationSubscription subscription);

        void Unsubscribe(string notificationId);

        void LookForUnsubscribeMessages();
    }
}
