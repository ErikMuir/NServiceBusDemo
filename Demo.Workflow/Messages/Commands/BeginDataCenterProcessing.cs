namespace Demo.Workflow.Messages.Commands;

public class BeginDataCenterProcessing : WorkflowCommand
{
    public BeginDataCenterProcessing(Guid workflowId) : base(workflowId) { }
}
