namespace Demo.Workflow.Messages.Events;

public class HardwareAllocated : WorkflowEvent
{
    public HardwareAllocated(Guid workflowId) : base(workflowId) { }
}
