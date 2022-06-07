namespace Workflow.Messages;

public class BeginWorkflow : WorkflowCommand
{
    public BeginWorkflow(Guid workflowId, string userEmail) : base(workflowId)
    {
        UserEmail = userEmail;
    }

    public string UserEmail { get; set; } = default!;
}
