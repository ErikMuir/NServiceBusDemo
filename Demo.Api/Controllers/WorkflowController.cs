using Demo.Api.Models;
using Demo.Workflow.Messages;
using Microsoft.AspNetCore.Mvc;
using NServiceBus;

namespace Demo.Api.Controllers;

[ApiController]
[Route("[controller]")]
public class WorkflowController : ControllerBase
{
    private readonly IMessageSession _messageSession;
    private readonly ILogger<WorkflowController> _logger;

    public WorkflowController(IMessageSession messageSession, ILogger<WorkflowController> logger)
    {
        _messageSession = messageSession;
        _logger = logger;
    }

    [HttpPost]
    public async Task<IActionResult> BeginWorkflow([FromBody] BeginWorkflowRequest requestBody)
    {
        var workflowId = Guid.NewGuid();
        var message = new BeginWorkflow(workflowId, requestBody.UserEmail);

        await _messageSession.Send(message)
            .ConfigureAwait(false);

        return Ok(new { workflowId });
    }

    [HttpPost("{workflowId}/requisition")]
    public async Task<IActionResult> RequisitionForm(Guid workflowId)
    {
        var message = new RequisitionFormSubmitted(workflowId);

        await _messageSession.Publish(message)
            .ConfigureAwait(false);

        return Ok();
    }

    [HttpPost("{workflowId}/governance")]
    public async Task<IActionResult> Governance(Guid workflowId, [FromBody] GovernanceRequest requestBody)
    {
        IEvent message = requestBody.IsApproved
            ? new GovernanceApproval(workflowId)
            : new GovernanceDenial(workflowId);

        await _messageSession.Publish(message)
            .ConfigureAwait(false);

        return Ok();
    }

    [HttpPost("{workflowId}/hardware")]
    public async Task<IActionResult> Hardware(Guid workflowId)
    {
        var message = new HardwareAllocated(workflowId);

        await _messageSession.Publish(message)
            .ConfigureAwait(false);

        return Ok();
    }

    [HttpPost("{workflowId}/networking")]
    public async Task<IActionResult> Networking(Guid workflowId)
    {
        var message = new NetworkingConfigured(workflowId);

        await _messageSession.Publish(message)
            .ConfigureAwait(false);

        return Ok();
    }

    [HttpPost("{workflowId}/data-center")]
    public async Task<IActionResult> DataCenter(Guid workflowId)
    {
        var message = new DataCenterProcessed(workflowId);

        await _messageSession.Publish(message)
            .ConfigureAwait(false);

        return Ok();
    }
}