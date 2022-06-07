namespace Workflow.Messages;

public class GovernanceDenial : WorkflowEvent
{
    public GovernanceDenial(Guid workflowId) : base(workflowId) { }
}
