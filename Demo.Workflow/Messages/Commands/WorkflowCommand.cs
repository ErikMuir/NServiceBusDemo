using NServiceBus;

namespace Demo.Workflow.Messages;

public abstract class WorkflowCommand : ICommand
{
    public WorkflowCommand(Guid workflowId)
    {
        WorkflowId = workflowId;
    }

    public Guid WorkflowId { get; set; }
}
