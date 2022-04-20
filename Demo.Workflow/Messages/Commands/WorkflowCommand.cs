using NServiceBus;

namespace Demo.Workflow.Messages.Commands;

public abstract class WorkflowCommand : ICommand, IWorkflowMessage
{
    protected WorkflowCommand(Guid workflowId)
    {
        WorkflowId = workflowId;
    }

    public Guid WorkflowId { get; set; }
}
