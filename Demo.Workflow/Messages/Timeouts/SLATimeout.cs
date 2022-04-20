namespace Demo.Workflow.Messages.Timeouts;

public class SLATimeout : IWorkflowMessage
{
    public SLATimeout() { }

    public SLATimeout(SLATimeout timeout)
    {
        WorkflowId = timeout.WorkflowId;
        SLAWindow = timeout.SLAWindow;
        StartUtc = timeout.StartUtc;
    }

    public Guid WorkflowId { get; set; }
    public TimeSpan SLAWindow { get; set; }
    public DateTime StartUtc { get; set; }
    public DateTime ExpiresUtc => StartUtc.Add(SLAWindow);
}
