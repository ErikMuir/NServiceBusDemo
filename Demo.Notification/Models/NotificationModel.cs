namespace Demo.Notification.Models;

public class NotificationModel
{
    public NotificationModel(string to)
    {
        Id = Guid.NewGuid();
        To = to;
    }

    public Guid Id { get; }
    public string From { get; set; } = "no-reply@example.com";

    public string To { get; set; }
    public string? Subject { get; set; }
    public string? Body { get; set; }
}
