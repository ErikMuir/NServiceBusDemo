using Demo.Notification.Models;
using NServiceBus;

namespace Demo.Notification.Messages.Commands;

public class SendNotification : ICommand
{
    public SendNotification(NotificationModel notification)
    {
        Notification = notification;
    }

    public NotificationModel Notification { get; set; }
}
