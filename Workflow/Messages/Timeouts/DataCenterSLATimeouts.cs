namespace Workflow.Messages;

public class DataCenterSLAWarning : SLATimeout
{
    public DataCenterSLAWarning(SLATimeout timeout) : base(timeout) { }
}

public class DataCenterSLAExpired : SLATimeout
{
    public DataCenterSLAExpired(SLATimeout timeout) : base(timeout) { }
}
