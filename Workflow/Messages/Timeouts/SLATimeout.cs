namespace Workflow.Messages;

public class SLATimeout
{
    public SLATimeout() { }

    public SLATimeout(SLATimeout timeout)
    {
        WorkflowId = timeout.WorkflowId;
        Team = timeout.Team;
        SLAWindow = timeout.SLAWindow;
        StartUtc = timeout.StartUtc;
    }

    public Guid WorkflowId { get; set; }
    public string Team { get; set; } = default!;
    public TimeSpan SLAWindow { get; set; }
    public DateTime StartUtc { get; set; }
    public DateTime ExpiresUtc => StartUtc.Add(SLAWindow);
}

public class SLAWarning : SLATimeout
{
    public SLAWarning(SLATimeout timeout) : base(timeout) { }
}

public class SLAExpired : SLATimeout
{
    public SLAExpired(SLATimeout timeout) : base(timeout) { }
}
