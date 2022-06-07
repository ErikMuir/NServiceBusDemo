namespace Workflow.Messages;

public class HardwareAllocated : WorkflowEvent
{
    public HardwareAllocated(Guid workflowId) : base(workflowId) { }
}
