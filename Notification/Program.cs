using Microsoft.Extensions.Hosting;
using NServiceBus;

Console.Title = "Notification Service";

var host = Host
    .CreateDefaultBuilder(args)
    .UseNServiceBus(context =>
    {
        var endpointConfiguration = new EndpointConfiguration("Notification");

        var transport = endpointConfiguration.UseTransport<LearningTransport>();

        endpointConfiguration.UsePersistence<LearningPersistence>();
        endpointConfiguration.SendFailedMessagesTo("error");
        endpointConfiguration.AuditProcessedMessagesTo("audit");

        return endpointConfiguration;
    });

await host.RunConsoleAsync();
