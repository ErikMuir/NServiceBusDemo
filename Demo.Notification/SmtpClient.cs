using System.Net.Mail;

namespace Demo.Notification
{
    public class SmtpClient
    {
        public Task SendMailAsync(MailMessage message) => Task.CompletedTask;
    }
}
