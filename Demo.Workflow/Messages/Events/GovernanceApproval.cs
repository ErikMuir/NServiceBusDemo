namespace Demo.Workflow.Messages;

public class GovernanceApproval : WorkflowEvent
{
    public GovernanceApproval(Guid workflowId) : base(workflowId) { }
}
