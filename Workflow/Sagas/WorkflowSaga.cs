using NServiceBus;
using NServiceBus.Logging;
using Workflow.Messages;
using Notification;
using Notification.Messages;

namespace Workflow.Sagas;

public class WorkflowSaga : Saga<WorkflowSagaData>,
    IAmStartedByMessages<BeginWorkflow>,
    IHandleMessages<RequisitionFormSubmitted>,
    IHandleMessages<GovernanceApproval>,
    IHandleMessages<GovernanceDenial>,
    IHandleMessages<HardwareAllocated>,
    IHandleMessages<NetworkingConfigured>,
    IHandleMessages<DataCenterProcessed>,
    IHandleTimeouts<DataCenterSLAWarning>,
    IHandleTimeouts<DataCenterSLAExpired>
{
    static readonly ILog _log = LogManager.GetLogger<WorkflowSaga>();

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
            .ToMessage<DataCenterSLAWarning>(msg => msg.WorkflowId)
            .ToMessage<DataCenterSLAExpired>(msg => msg.WorkflowId);
    }

    #region Handlers
    public Task Handle(BeginWorkflow message, IMessageHandlerContext context)
    {
        _log.Info($"{message.WorkflowId} - Starting Workflow Saga");

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
            _log.Warn($"{Data.WorkflowId} - Cannot process {message.GetType().Name} events while in {Data.Status} status!");
            return Task.CompletedTask;
        }

        _log.Info($"{Data.WorkflowId} - Requisition form submitted");

        Data.Status = WorkflowStatus.Submitted;

        var email = GetEmailCommand(NotificationType.BeginGovernance);
        return context.Send(email);
    }

    public Task Handle(GovernanceDenial message, IMessageHandlerContext context)
    {
        if (Data.Status != WorkflowStatus.Submitted)
        {
            _log.Warn($"{Data.WorkflowId} - Cannot process {message.GetType().Name} events while in {Data.Status} status!");
            return Task.CompletedTask;
        }

        _log.Info($"{Data.WorkflowId} - Governance has denied the request");

        Data.Status = WorkflowStatus.Denied;

        var email = GetEmailCommand(NotificationType.GovernanceDenied);
        return context.Send(email);
    }

    public Task Handle(GovernanceApproval message, IMessageHandlerContext context)
    {
        if (Data.Status != WorkflowStatus.Submitted)
        {
            _log.Warn($"{Data.WorkflowId} - Cannot process {message.GetType().Name} events while in {Data.Status} status!");
            return Task.CompletedTask;
        }

        _log.Info($"{Data.WorkflowId} - Governance has approved the request");

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
            _log.Warn($"{Data.WorkflowId} - Cannot process {message.GetType().Name} events while in {Data.Status} status!");
            return Task.CompletedTask;
        }

        if (Data.HardwareAllocatedUtc is null)
        {
            _log.Info($"{Data.WorkflowId} - Hardware allocated");
            Data.HardwareAllocatedUtc = DateTime.UtcNow;
        }

        return ProcessHardwareAndNetworking(context);
    }

    public Task Handle(NetworkingConfigured message, IMessageHandlerContext context)
    {
        if (Data.Status != WorkflowStatus.Approved)
        {
            _log.Warn($"{Data.WorkflowId} - Cannot process {message.GetType().Name} events while in {Data.Status} status!");
            return Task.CompletedTask;
        }

        if (Data.NetworkingConfiguredUtc is null)
        {
            _log.Info($"{Data.WorkflowId} - Network configured");
            Data.NetworkingConfiguredUtc = DateTime.UtcNow;
        }

        return ProcessHardwareAndNetworking(context);
    }

    public Task Handle(DataCenterProcessed message, IMessageHandlerContext context)
    {
        if (Data.Status != WorkflowStatus.Designed)
        {
            _log.Warn($"{Data.WorkflowId} - Cannot process {message.GetType().Name} events while in {Data.Status} status!");
            return Task.CompletedTask;
        }

        if (!IsDesignPhaseComplete())
        {
            _log.Warn($"{Data.WorkflowId} - Waiting on other design teams");
            return Task.CompletedTask;
        }

        _log.Info($"{Data.WorkflowId} - Data Center processed");

        Data.Status = WorkflowStatus.Complete;
        Data.DataCenterProcessedUtc = DateTime.UtcNow;
        Data.CompletedUtc = DateTime.UtcNow;

        var email = GetEmailCommand(NotificationType.RequestFulfilled);
        return context.Send(email);
    }
    #endregion

    #region Timeouts
    public Task Timeout(DataCenterSLAWarning timeout, IMessageHandlerContext context)
    {
        if (Data.DataCenterProcessedUtc is not null)
        {
            return Task.CompletedTask;
        }

        _log.Warn($"{Data.WorkflowId} - WARNING! Data Center SLA about to expire");
        var email = GetEmailCommand(NotificationType.DataCenterSLAWarning);
        return context.Send(email);
    }

    public Task Timeout(DataCenterSLAExpired timeout, IMessageHandlerContext context)
    {
        if (Data.DataCenterProcessedUtc is not null)
        {
            return Task.CompletedTask;
        }

        _log.Warn($"{Data.WorkflowId} - ALERT! Data Center SLA has expired");
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

        // begin data center processing
        var email = GetEmailCommand(NotificationType.BeginDataCenter);
        var beginDataCenterProcessingTask = context.Send(email);

        // data center SLA prep
        var startUtc = DateTime.UtcNow;
        var dataCenterSLA = TimeSpan.FromMinutes(1); // would probably come from config
        var timeout = new SLATimeout
        {
            WorkflowId = Data.WorkflowId,
            SLAWindow = dataCenterSLA,
            StartUtc = startUtc,
        };

        // create SLA warning timeout
        var slaWarning = new DataCenterSLAWarning(timeout);
        var warnAt = startUtc.Add(dataCenterSLA * 0.8);
        var slaWarningTask = RequestTimeout<DataCenterSLAWarning>(context, warnAt, slaWarning);

        // create SLA expired timeout
        var slaExpired = new DataCenterSLAExpired(timeout);
        var expireAt = startUtc.Add(dataCenterSLA);
        var slaExpiredTask = RequestTimeout<DataCenterSLAExpired>(context, expireAt, slaExpired);

        // await all tasks
        return Task.WhenAll(new Task[]
        {
            beginDataCenterProcessingTask,
            slaWarningTask,
            slaExpiredTask,
        });
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
    #endregion
}
