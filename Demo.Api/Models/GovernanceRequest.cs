namespace Demo.Api.Models;

public class GovernanceRequest
{
    public GovernanceRequest(bool isApproved)
    {
        IsApproved = isApproved;
    }

    public bool IsApproved { get; set; }
}
