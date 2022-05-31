﻿using Demo.Notification.Messages;
using Microsoft.Extensions.Hosting;
using NServiceBus;

var host = Host
    .CreateDefaultBuilder(args)
    .UseNServiceBus(context =>
    {
        var endpointConfiguration = new EndpointConfiguration("Demo.Workflow");

        var transport = endpointConfiguration.UseTransport<LearningTransport>();

        transport.Routing().RouteToEndpoint(
            assembly: typeof(SendEmail).Assembly,
            destination: "Demo.Notification");

        endpointConfiguration.UsePersistence<LearningPersistence>();
        endpointConfiguration.SendFailedMessagesTo("error");
        endpointConfiguration.AuditProcessedMessagesTo("audit");

        return endpointConfiguration;
    });

await host.RunConsoleAsync();