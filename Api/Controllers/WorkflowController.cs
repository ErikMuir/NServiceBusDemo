using Api.Models;
using Workflow;
using Workflow.Messages;
using Microsoft.AspNetCore.Mvc;
using NServiceBus;

namespace Api.Controllers;

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

        _logger.LogInformation($"{workflowId} - Sending BeginWorkflow command to Workflow service");

        await _messageSession.Send(message)
            .ConfigureAwait(false);

        return Ok(new { workflowId });
    }

    [HttpGet("{workflowId}")]
    public IActionResult GetWorkflowStatus(Guid workflowId)
    {
        var sagaData = SagaPersistenceHelper.GetSagaData(workflowId);
        var status = sagaData?.Status ?? WorkflowStatus.Unknown;
        var description = status.GetDescription();

        _logger.LogInformation($"{workflowId} - Status: {status}. {description}");

        return Ok(new { status, description });
    }


    [HttpPost("{workflowId}/requisition")]
    public async Task<IActionResult> RequisitionForm(Guid workflowId)
    {
        var message = new RequisitionFormSubmitted(workflowId);

        _logger.LogInformation($"{workflowId} - Publishing RequisitionFormSubmitted event");

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

        _logger.LogInformation($"{workflowId} - Publishing {message.GetType().Name} event");

        await _messageSession.Publish(message)
            .ConfigureAwait(false);

        return Ok();
    }

    [HttpPost("{workflowId}/hardware")]
    public async Task<IActionResult> Hardware(Guid workflowId)
    {
        var message = new HardwareAllocated(workflowId);

        _logger.LogInformation($"{workflowId} - Publishing HardwareAllocated event");

        await _messageSession.Publish(message)
            .ConfigureAwait(false);

        return Ok();
    }

    [HttpPost("{workflowId}/networking")]
    public async Task<IActionResult> Networking(Guid workflowId)
    {
        var message = new NetworkingConfigured(workflowId);

        _logger.LogInformation($"{workflowId} - Publishing NetworkingConfigured event");

        await _messageSession.Publish(message)
            .ConfigureAwait(false);

        return Ok();
    }

    [HttpPost("{workflowId}/data-center")]
    public async Task<IActionResult> DataCenter(Guid workflowId)
    {
        var message = new DataCenterProcessed(workflowId);

        _logger.LogInformation($"{workflowId} - Publishing DataCenterProcessed event");

        await _messageSession.Publish(message)
            .ConfigureAwait(false);

        return Ok();
    }
}
