using Demo.Notification.Models;
using NServiceBus;

namespace Demo.Notification.Messages.Events;

public class NotificationSent : IEvent
{
    public NotificationSent(NotificationModel notification)
    {
        Notification = notification;
        SentAtUtc = DateTime.UtcNow;
    }

    public NotificationModel Notification { get; }
    public DateTime SentAtUtc { get; }
}
