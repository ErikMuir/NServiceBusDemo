namespace Demo.Workflow.Messages;

public class DataCenterProcessed : WorkflowEvent
{
    public DataCenterProcessed(Guid workflowId) : base(workflowId) { }
}
