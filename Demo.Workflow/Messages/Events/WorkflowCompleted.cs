namespace Demo.Workflow.Messages.Events;

public class WorkflowCompleted : WorkflowEvent
{
    public WorkflowCompleted(Guid workflowId, DateTime completedUtc, string userToNotify) : base(workflowId)
    {
        CompletedUtc = completedUtc;
        UserToNotify = userToNotify;
    }

    public DateTime CompletedUtc { get; set; }
    public string UserToNotify { get; set; }
}
