namespace Demo.Workflow.Messages.Events;

public class WorkflowCreated : WorkflowEvent
{
    public WorkflowCreated(Guid workflowId, string user) : base(workflowId)
    {
        User = user;
    }

    public string User { get; set; }
}
