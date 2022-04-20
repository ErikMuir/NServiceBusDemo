namespace Demo.Workflow.Messages.Events;

public class NetworkingConfigured : WorkflowEvent
{
    public NetworkingConfigured(Guid workflowId) : base(workflowId) { }
}
