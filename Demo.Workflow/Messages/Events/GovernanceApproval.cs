namespace Demo.Workflow.Messages.Events;

public class GovernanceApproval : WorkflowEvent
{
    public GovernanceApproval(Guid workflowId) : base(workflowId) { }
}
