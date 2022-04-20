namespace Demo.Workflow.Messages.Commands;

public class BeginHardwareAndNetworking : WorkflowCommand
{
    public BeginHardwareAndNetworking(Guid workflowId) : base(workflowId) { }
}
