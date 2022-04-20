namespace Demo.Workflow.Messages;

public interface IWorkflowMessage
{
    Guid WorkflowId { get; set; }
}
