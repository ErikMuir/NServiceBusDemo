using NServiceBus;
using Workflow.Messages;
using Notification;
using Notification.Messages;
using MuirDev.ConsoleTools;

namespace Workflow.Sagas;

public class WorkflowSaga : Saga<WorkflowSagaData>,
    IAmStartedByMessages<BeginWorkflow>,
    IHandleMessages<RequisitionFormSubmitted>,
    IHandleMessages<GovernanceApproval>,
    IHandleMessages<GovernanceDenial>,
    IHandleMessages<HardwareAllocated>,
    IHandleMessages<NetworkingConfigured>,
    IHandleMessages<DataCenterProcessed>,
    IHandleTimeouts<SLAWarning>,
    IHandleTimeouts<SLAExpired>
{
    static readonly FluentConsole _log = new();

    protected override void ConfigureHowToFindSaga(SagaPropertyMapper<WorkflowSagaData> mapper)
    {
        mapper.MapSaga(saga => saga.WorkflowId)
            .ToMessage<BeginWorkflow>(msg => msg.WorkflowId)
            .ToMessage<RequisitionFormSubmitted>(msg => msg.WorkflowId)
            .ToMessage<GovernanceApproval>(msg => msg.WorkflowId)
            .ToMessage<GovernanceDenial>(msg => msg.WorkflowId)
            .ToMessage<HardwareAllocated>(msg => msg.WorkflowId)
            .ToMessage<NetworkingConfigured>(msg => msg.WorkflowId)
            .ToMessage<DataCenterProcessed>(msg => msg.WorkflowId)
            .ToMessage<SLAWarning>(msg => msg.WorkflowId)
            .ToMessage<SLAExpired>(msg => msg.WorkflowId);
    }

    #region Handlers
    public Task Handle(BeginWorkflow message, IMessageHandlerContext context)
    {
        _log.Info("Starting Workflow Saga");

        // Data.WorkflowId = message.WorkflowId; // this happens auto-magically by the Saga framework
        Data.Status = WorkflowStatus.Created;
        Data.UserEmail = message.UserEmail;
        Data.CreatedUtc = DateTime.UtcNow;

        var email = GetEmailCommand(NotificationType.WorkflowCreated);
        return context.Send(email);
    }

    public Task Handle(RequisitionFormSubmitted message, IMessageHandlerContext context)
    {
        var validStates = new List<WorkflowStatus>
        {
            WorkflowStatus.Created,
            WorkflowStatus.Denied,
        };
        if (!validStates.Contains(Data.Status))
        {
            return CannotProcess(message);
        }

        _log.Info("Requisition form submitted");

        Data.Status = WorkflowStatus.Submitted;

        var email = GetEmailCommand(NotificationType.BeginGovernance);
        return context.Send(email);
    }

    public Task Handle(GovernanceDenial message, IMessageHandlerContext context)
    {
        if (Data.Status != WorkflowStatus.Submitted)
        {
            return CannotProcess(message);
        }

        _log.Info("Governance has denied the request");

        Data.Status = WorkflowStatus.Denied;

        var email = GetEmailCommand(NotificationType.GovernanceDenied);
        return context.Send(email);
    }

    public Task Handle(GovernanceApproval message, IMessageHandlerContext context)
    {
        if (Data.Status != WorkflowStatus.Submitted)
        {
            return CannotProcess(message);
        }

        _log.Info("Governance has approved the request");

        Data.Status = WorkflowStatus.Approved;

        var hardwareEmail = GetEmailCommand(NotificationType.BeginHardware);
        var hardwareTask = context.Send(hardwareEmail);

        var networkingEmail = GetEmailCommand(NotificationType.BeginNetworking);
        var networkingTask = context.Send(networkingEmail);

        return Task.WhenAll(new Task[]
        {
            hardwareTask,
            networkingTask,
        });
    }

    public Task Handle(HardwareAllocated message, IMessageHandlerContext context)
    {
        if (Data.Status != WorkflowStatus.Approved)
        {
            return CannotProcess(message);
        }

        if (Data.HardwareAllocatedUtc is null)
        {
            _log.Info("Hardware allocated");
            Data.HardwareAllocatedUtc = DateTime.UtcNow;
        }
        else
        {
            _log.Info("Hardware already allocated");
        }

        return ProcessHardwareAndNetworking(context);
    }

    public Task Handle(NetworkingConfigured message, IMessageHandlerContext context)
    {
        if (Data.Status != WorkflowStatus.Approved)
        {
            return CannotProcess(message);
        }

        if (Data.NetworkingConfiguredUtc is null)
        {
            _log.Info("Network configured");
            Data.NetworkingConfiguredUtc = DateTime.UtcNow;
        }
        else
        {
            _log.Info("Network already configured");
        }

        return ProcessHardwareAndNetworking(context);
    }

    public Task Handle(DataCenterProcessed message, IMessageHandlerContext context)
    {
        if (Data.Status != WorkflowStatus.Designed)
        {
            return CannotProcess(message);
        }

        if (!IsDesignPhaseComplete())
        {
            _log.Warning("Waiting on other design teams");
            return Task.CompletedTask;
        }

        _log.Info("Data Center processed");

        Data.Status = WorkflowStatus.Complete;
        Data.DataCenterProcessedUtc = DateTime.UtcNow;
        Data.CompletedUtc = DateTime.UtcNow;

        var email = GetEmailCommand(NotificationType.RequestFulfilled);
        return context.Send(email);
    }
    #endregion

    #region Timeouts
    public Task Timeout(SLAWarning timeout, IMessageHandlerContext context)
    {
        if (Data.DataCenterProcessedUtc is not null)
        {
            return Task.CompletedTask;
        }

        _log.Warning($"WARNING! {timeout.Team} SLA about to expire");
        var email = GetEmailCommand(NotificationType.DataCenterSLAWarning);
        return context.Send(email);
    }

    public Task Timeout(SLAExpired timeout, IMessageHandlerContext context)
    {
        if (Data.DataCenterProcessedUtc is not null)
        {
            return Task.CompletedTask;
        }

        _log.Warning($"ALERT! {timeout.Team} SLA has expired");
        var email = GetEmailCommand(NotificationType.DataCenterSLAExpired);
        return context.Send(email);
    }
    #endregion

    #region Private Methods
    private Task ProcessHardwareAndNetworking(IMessageHandlerContext context)
    {
        if (!IsDesignPhaseComplete())
        {
            return Task.CompletedTask;
        }

        Data.Status = WorkflowStatus.Designed;

        var email = GetEmailCommand(NotificationType.BeginDataCenter);
        var timeout = new SLATimeout
        {
            WorkflowId = Data.WorkflowId,
            Team = "Data Center",
            SLAWindow = TimeSpan.FromMinutes(1), // would probably come from config
            StartUtc = DateTime.UtcNow,
        };

        return Task.WhenAll(new Task[]
        {
            context.Send(email),
            CreateSLAWarning(context, timeout),
            CreateSLAExpired(context, timeout),
        });
    }

    private Task CreateSLAWarning(IMessageHandlerContext context, SLATimeout timeout)
    {
        var slaWarning = new SLAWarning(timeout);
        var triggerAt = timeout.StartUtc.Add(timeout.SLAWindow * 0.8);
        return RequestTimeout<SLAWarning>(context, triggerAt, slaWarning);
    }

    private Task CreateSLAExpired(IMessageHandlerContext context, SLATimeout timeout)
    {
        var slaExpired = new SLAExpired(timeout);
        var triggerAt = timeout.StartUtc.Add(timeout.SLAWindow);
        return RequestTimeout<SLAExpired>(context, triggerAt, slaExpired);
    }

    private SendEmail GetEmailCommand(NotificationType type)
        => new SendEmail
        {
            WorkflowId = Data.WorkflowId,
            Type = type,
            To = GetRecipient(type),
            Subject = type.ToString(),
            Body = $"Workflow ID: {Data.WorkflowId}",
        };

    private string GetRecipient(NotificationType type)
        => type switch
        {
            NotificationType.WorkflowCreated => Data.UserEmail,
            NotificationType.GovernanceDenied => Data.UserEmail,
            NotificationType.RequestFulfilled => Data.UserEmail,
            NotificationType.BeginGovernance => "governance@example.com",
            NotificationType.BeginHardware => "hardware@example.com",
            NotificationType.BeginNetworking => "networking@example.com",
            NotificationType.BeginDataCenter => "datacenter@example.com",
            NotificationType.DataCenterSLAWarning => "datacenter@example.com",
            NotificationType.DataCenterSLAExpired => "datacenter@example.com",
            _ => throw new NotSupportedException(),
        };

    private bool IsDesignPhaseComplete()
        => Data.HardwareAllocatedUtc.HasValue && Data.NetworkingConfiguredUtc.HasValue;

    private Task CannotProcess(IMessage message)
    {
        _log.Warning($"Cannot process {message.GetType().Name} events while in {Data.Status} status!");
        return Task.CompletedTask;
    }
    #endregion
}
