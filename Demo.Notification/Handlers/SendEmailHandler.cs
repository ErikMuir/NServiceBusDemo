using NServiceBus;
using NServiceBus.Logging;
using Demo.Notification.Messages;
using System.Net.Mail;

namespace Demo.Notification.Handlers;

public class SendEmailHandler : IHandleMessages<SendEmail>
{
    static readonly ILog log = LogManager.GetLogger<SendEmailHandler>();

    public Task Handle(SendEmail message, IMessageHandlerContext context) 
    {
        log.InfoFormat($"{message.WorkflowId} - Sending {message.Type} email to {message.To}");
        var email = new MailMessage(message.From, message.To, message.Subject, null);
        return new SmtpClient().SendMailAsync(email);
    }
}
