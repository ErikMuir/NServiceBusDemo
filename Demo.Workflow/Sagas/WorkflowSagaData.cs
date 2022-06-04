using NServiceBus;

namespace Demo.Workflow.Sagas;

public class WorkflowSagaData : ContainSagaData
{
    public Guid WorkflowId { get; set; }
    public WorkflowStatus Status { get; set; }
    public string UserEmail { get; set; } = default!;
    public DateTime CreatedUtc { get; set; }
    public DateTime CompletedUtc { get; set; }

    public bool IsRequisitionFormSubmitted { get; set; }
    public bool? HasGovernanceApproval { get; set; }
    public bool IsHardwareAllocated { get; set; }
    public bool IsNetworkConfigured { get; set; }
    public bool IsDataCenterProcessed { get; set; }
}
