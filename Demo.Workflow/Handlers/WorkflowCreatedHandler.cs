using NServiceBus;
using NServiceBus.Logging;
using Demo.Workflow.Messages.Events;

namespace Demo.Workflow.Handlers;

public class WorkflowCreatedHandler : IHandleMessages<WorkflowCreated>
{
    static readonly ILog log = LogManager.GetLogger<WorkflowCreatedHandler>();

    public Task Handle(WorkflowCreated message, IMessageHandlerContext context)
    {
        log.Info($"Notifying user {message.User} that workflow {message.WorkflowId} has been created...");
        return Task.CompletedTask;
    }
}
