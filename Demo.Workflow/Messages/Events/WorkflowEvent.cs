using NServiceBus;

namespace Demo.Workflow.Messages.Events;

public abstract class WorkflowEvent : IEvent, IWorkflowMessage
{
    protected WorkflowEvent(Guid workflowId)
    {
        WorkflowId = workflowId;
    }

    public Guid WorkflowId { get; set; }
}
