using System.Text.Json.Serialization;

namespace Demo.Workflow
{
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum WorkflowStatus
    {
        None,
        Created,
        Submitted,
        Denied,
        ApprovedAndBeingDesigned,
        DesignedAndBeingImplemented,
        Complete,
    }

    public static class WorkflowStatusExtensions
    {
        public static string ToFriendlyString(this WorkflowStatus status) => status switch
        {
            WorkflowStatus.None => "Not yet created.",
            WorkflowStatus.Created => "Workflow created. Waiting for requisition form submission.",
            WorkflowStatus.Submitted => "Requisition form submitted. Waiting for governance review.",
            WorkflowStatus.Denied => "Request denied. Optionally make changes to requisition form and resubmit.",
            WorkflowStatus.ApprovedAndBeingDesigned => "Request approved. Currently in design phase.",
            WorkflowStatus.DesignedAndBeingImplemented => "Design phase complete. Currently being implemented.",
            WorkflowStatus.Complete => "Implementation complete. Request fulfilled.",
            _ => throw new NotSupportedException(),
        };
    }
}
