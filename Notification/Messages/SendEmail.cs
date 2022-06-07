using System.Net.Mail;
using NServiceBus;

namespace Notification.Messages;

public class SendEmail : ICommand
{
    public Guid WorkflowId { get; set; }
    public NotificationType Type { get; set; }
    public string From { get; set; } = "no-reply@example.com";
    public string To { get; set; } = default!;
    public string Subject { get; set; } = default!;
    public string? Body { get; set; }
}
