using NServiceBus;

namespace Demo.Workflow.Messages;

public abstract class WorkflowEvent : IEvent
{
    protected WorkflowEvent(Guid workflowId)
    {
        WorkflowId = workflowId;
    }

    public Guid WorkflowId { get; set; }
}
