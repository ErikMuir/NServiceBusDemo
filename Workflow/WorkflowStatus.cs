using System.Text.Json.Serialization;

namespace Workflow;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum WorkflowStatus
{
    Unknown,
    Created,
    Submitted,
    Denied,
    Approved,
    Designed,
    Complete,
}

public static class WorkflowStatusExtensions
{
    public static string GetDescription(this WorkflowStatus status) => status switch
    {
        WorkflowStatus.Unknown => "Could not retrieve workflow status.",
        WorkflowStatus.Created => "Waiting for requisition form submission.",
        WorkflowStatus.Submitted => "Waiting for governance review.",
        WorkflowStatus.Denied => "Optionally make changes to the requisition form and resubmit.",
        WorkflowStatus.Approved => "Waiting for design.",
        WorkflowStatus.Designed => "Waiting for implementation.",
        WorkflowStatus.Complete => "Implementation complete. Request fulfilled.",
        _ => throw new NotSupportedException(),
    };
}
