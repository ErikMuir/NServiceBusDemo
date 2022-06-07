namespace Workflow.Messages;

public class NetworkingConfigured : WorkflowEvent
{
    public NetworkingConfigured(Guid workflowId) : base(workflowId) { }
}
