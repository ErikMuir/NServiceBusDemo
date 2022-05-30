namespace Demo.Api.Models;

public class BeginWorkflowRequest
{
    public BeginWorkflowRequest(string userEmail)
    {
        UserEmail = userEmail;
    }

    public string UserEmail { get; set; }
}
