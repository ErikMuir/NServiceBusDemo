namespace Demo.Workflow.Messages.Commands;

public class BeginWorkflow : WorkflowCommand
{
    public BeginWorkflow(Guid workflowId, string user) : base(workflowId)
    {
        User = user;
    }

    public string User { get; set; }
}
