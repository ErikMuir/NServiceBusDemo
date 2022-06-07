using System.Net.Mail;

namespace Notification
{
    public class SmtpClient
    {
        public Task SendMailAsync(MailMessage message) => Task.CompletedTask;
    }
}
