using Api.Models;
using Workflow;
using Workflow.Messages;
using Microsoft.AspNetCore.Mvc;
using NServiceBus;
using MuirDev.ConsoleTools;

namespace Api.Controllers;

[ApiController]
[Route("[controller]")]
public class WorkflowController : ControllerBase
{
    private readonly IMessageSession _messageSession;
    static readonly FluentConsole _log = new();

    public WorkflowController(IMessageSession messageSession)
    {
        _messageSession = messageSession;
    }

    [HttpPost]
    public async Task<IActionResult> BeginWorkflow([FromBody] BeginWorkflowRequest requestBody)
    {
        var workflowId = Guid.NewGuid();
        var message = new BeginWorkflow(workflowId, requestBody.UserEmail);

        _log.Info("Sending BeginWorkflow command to Workflow service");

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

        _log.Info($"Status: {status}. {description}");

        return Ok(new { status, description });
    }


    [HttpPost("{workflowId}/requisition")]
    public async Task<IActionResult> RequisitionForm(Guid workflowId)
    {
        var message = new RequisitionFormSubmitted(workflowId);

        _log.Info("Publishing RequisitionFormSubmitted event");

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

        _log.Info($"Publishing {message.GetType().Name} event");

        await _messageSession.Publish(message)
            .ConfigureAwait(false);

        return Ok();
    }

    [HttpPost("{workflowId}/hardware")]
    public async Task<IActionResult> Hardware(Guid workflowId)
    {
        var message = new HardwareAllocated(workflowId);

        _log.Info("Publishing HardwareAllocated event");

        await _messageSession.Publish(message)
            .ConfigureAwait(false);

        return Ok();
    }

    [HttpPost("{workflowId}/networking")]
    public async Task<IActionResult> Networking(Guid workflowId)
    {
        var message = new NetworkingConfigured(workflowId);

        _log.Info("Publishing NetworkingConfigured event");

        await _messageSession.Publish(message)
            .ConfigureAwait(false);

        return Ok();
    }

    [HttpPost("{workflowId}/data-center")]
    public async Task<IActionResult> DataCenter(Guid workflowId)
    {
        var message = new DataCenterProcessed(workflowId);

        _log.Info("Publishing DataCenterProcessed event");

        await _messageSession.Publish(message)
            .ConfigureAwait(false);

        return Ok();
    }
}
