using NServiceBus;
using NServiceBus.Logging;
using Demo.Workflow.Messages.Commands;

namespace Demo.Workflow.Handlers;

public class NotifyGovernanceDenialHandler : IHandleMessages<NotifyGovernanceDenial>
{
    static readonly ILog log = LogManager.GetLogger<NotifyGovernanceDenialHandler>();

    public Task Handle(NotifyGovernanceDenial message, IMessageHandlerContext context)
    {
        log.Info($"Notifying user {message.UserToNotify} that governance review was denied for workflow {message.WorkflowId}...");
        return Task.CompletedTask;
    }
}
