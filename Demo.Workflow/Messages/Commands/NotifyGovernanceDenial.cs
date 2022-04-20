namespace Demo.Workflow.Messages.Commands;

public class NotifyGovernanceDenial : WorkflowCommand
{
    public NotifyGovernanceDenial(Guid workflowId, string userToNotify) : base(workflowId)
    {
        UserToNotify = userToNotify;
    }

    public string UserToNotify { get; set; }
}
