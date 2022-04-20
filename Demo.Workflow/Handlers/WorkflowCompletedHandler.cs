using NServiceBus;
using NServiceBus.Logging;
using Demo.Workflow.Messages.Events;

namespace Demo.Workflow.Handlers;

public class WorkflowCompletedHandler : IHandleMessages<WorkflowCompleted>
{
    static readonly ILog log = LogManager.GetLogger<WorkflowCompletedHandler>();

    public Task Handle(WorkflowCompleted message, IMessageHandlerContext context)
    {
        log.Info($"Notifying user {message.UserToNotify} that workflow {message.WorkflowId} was completed {message.CompletedUtc}...");
        return Task.CompletedTask;
    }
}
