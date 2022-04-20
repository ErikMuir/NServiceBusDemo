using NServiceBus;
using NServiceBus.Logging;
using Demo.Workflow.Messages.Commands;

namespace Demo.Workflow.Handlers;

public class BeginHardwareAndNetworkingHandler : IHandleMessages<BeginHardwareAndNetworking>
{
    static readonly ILog log = LogManager.GetLogger<BeginHardwareAndNetworking>();

    public Task Handle(BeginHardwareAndNetworking message, IMessageHandlerContext context)
    {
        log.Info($"Notifying Hardware team that workflow {message.WorkflowId} is ready...");
        log.Info($"Notifying Networking team that workflow {message.WorkflowId} is ready...");
        return Task.CompletedTask;
    }
}
