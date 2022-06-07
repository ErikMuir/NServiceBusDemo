namespace Workflow.Messages;

public class RequisitionFormSubmitted : WorkflowEvent
{
    public RequisitionFormSubmitted(Guid workflowId) : base(workflowId) { }
}
