using Demo.Notification.Messages.Events;
using Microsoft.Extensions.Hosting;
using NServiceBus;

Console.Title = "Demo.Notification";

var host = Host
    .CreateDefaultBuilder(args)
    .UseNServiceBus(context =>
    {
        var endpointConfiguration = new EndpointConfiguration("Demo.Notification");

        var transport = endpointConfiguration.UseTransport<LearningTransport>();

        transport.Routing().RouteToEndpoint(
            messageType: typeof(NotificationSent),
            destination: "Demo.Workflow");

        endpointConfiguration.UsePersistence<LearningPersistence>();
        endpointConfiguration.SendFailedMessagesTo("error");
        endpointConfiguration.AuditProcessedMessagesTo("audit");

        return endpointConfiguration;
    });

await host.RunConsoleAsync();
