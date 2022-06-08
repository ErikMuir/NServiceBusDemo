using NServiceBus;
using Notification.Messages;
using System.Net.Mail;
using MuirDev.ConsoleTools;

namespace Notification.Handlers;

public class SendEmailHandler : IHandleMessages<SendEmail>
{
    static readonly FluentConsole log = new();

    public Task Handle(SendEmail message, IMessageHandlerContext context)
    {
        log.Info($"Sending {message.Type} email to {message.To}");
        var email = new MailMessage(message.From, message.To, message.Subject, null);
        return new SmtpClient().SendMailAsync(email);
    }
}
