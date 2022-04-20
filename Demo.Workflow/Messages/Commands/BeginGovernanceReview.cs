namespace Demo.Workflow.Messages.Commands;

public class BeginGovernanceReview : WorkflowCommand
{
    public BeginGovernanceReview(Guid workflowId) : base(workflowId) { }
}
