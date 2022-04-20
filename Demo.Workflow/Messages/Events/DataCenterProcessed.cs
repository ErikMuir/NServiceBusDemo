namespace Demo.Workflow.Messages.Events;

public class DataCenterProcessed : WorkflowEvent
{
    public DataCenterProcessed(Guid workflowId) : base(workflowId) { }
}
