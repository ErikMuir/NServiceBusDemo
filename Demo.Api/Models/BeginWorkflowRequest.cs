namespace Demo.Api.Models;

public class BeginWorkflowRequest
{
    public BeginWorkflowRequest(string requestedByUser)
    {
        RequestedByUser = requestedByUser;
    }

    public string RequestedByUser { get; set; }
}
