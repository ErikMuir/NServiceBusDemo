using NServiceBus;

namespace Workflow.Sagas;

public class WorkflowSagaData : ContainSagaData
{
    public Guid WorkflowId { get; set; }
    public WorkflowStatus Status { get; set; }
    public string UserEmail { get; set; } = default!;

    public DateTime? CreatedUtc { get; set; }
    public DateTime? FormSubmittedUtc { get; set; }
    public DateTime? GovernanceDeniedUtc { get; set; }
    public DateTime? GovernanceApprovedUtc { get; set; }
    public DateTime? HardwareAllocatedUtc { get; set; }
    public DateTime? NetworkingConfiguredUtc { get; set; }
    public DateTime? DataCenterProcessedUtc { get; set; }
    public DateTime? CompletedUtc { get; set; }
}
