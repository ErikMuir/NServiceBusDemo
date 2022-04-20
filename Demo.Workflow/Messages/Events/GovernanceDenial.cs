namespace Demo.Workflow.Messages.Events;

public class GovernanceDenial : WorkflowEvent
{
    public GovernanceDenial(Guid workflowId) : base(workflowId) { }
}
