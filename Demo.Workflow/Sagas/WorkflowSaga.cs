using NServiceBus;
using NServiceBus.Logging;
using Demo.Workflow.Messages;
using Demo.Notification;
using Demo.Notification.Messages;

namespace Demo.Workflow.Sagas;

public class WorkflowSaga :
    Saga<WorkflowSagaData>,
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
        _log.Info($"Creating workflow with ID {message.WorkflowId}");

        // Data.WorkflowId = message.WorkflowId; // this happens auto-magically by the Saga framework
        Data.UserEmail = message.UserEmail;
        Data.CreatedUtc = DateTime.UtcNow;

        var email = GetEmailCommand(NotificationType.WorkflowCreated);
        return context.Send(email);
    }

    public Task Handle(RequisitionFormSubmitted message, IMessageHandlerContext context)
    {
        _log.Info($"Requisition form submitted for workflow {message.WorkflowId}");

        Data.IsQuestionnaireSubmitted = true;
        Data.HasGovernanceApproval = null;

        var email = GetEmailCommand(NotificationType.BeginGovernance);
        return context.Send(email);
    }

    public Task Handle(GovernanceDenial message, IMessageHandlerContext context)
    {
        if (!Data.IsQuestionnaireSubmitted || Data.HasGovernanceApproval.HasValue)
        {
            _log.Warn($"Workflow {message.WorkflowId} not yet ready for governance review");
            return Task.CompletedTask;
        }

        _log.Info($"Governance has denied workflow {message.WorkflowId}");

        Data.HasGovernanceApproval = false;

        var email = GetEmailCommand(NotificationType.GovernanceDenied);
        return context.Send(email);
    }

    public Task Handle(GovernanceApproval message, IMessageHandlerContext context)
    {
        if (!Data.IsQuestionnaireSubmitted || Data.HasGovernanceApproval.HasValue)
        {
            _log.Warn($"Workflow {message.WorkflowId} not yet ready for governance review");
            return Task.CompletedTask;
        }

        _log.Info($"Governance has approved workflow {message.WorkflowId}");

        Data.HasGovernanceApproval = true;

        var hardwareEmail = GetEmailCommand(NotificationType.BeginHardware);
        var hardwareTask = context.Send(hardwareEmail);

        var networkingEmail = GetEmailCommand(NotificationType.BeginHardware);
        var networkingTask = context.Send(networkingEmail);

        return Task.WhenAll(new Task[]
        {
            hardwareTask,
            networkingTask,
        });
    }

    public Task Handle(HardwareAllocated message, IMessageHandlerContext context)
    {
        if (Data.HasGovernanceApproval == null || !Data.HasGovernanceApproval.Value)
        {
            _log.Warn($"Workflow {message.WorkflowId} not yet approved for hardware allocation step");
            return Task.CompletedTask;
        }

        _log.Info($"Hardware has been allocated for workflow {message.WorkflowId}");

        Data.IsHardwareAllocated = true;

        return ProcessHardwareAndNetworking(context);
    }

    public Task Handle(NetworkingConfigured message, IMessageHandlerContext context)
    {
        if (Data.HasGovernanceApproval == null || !Data.HasGovernanceApproval.Value)
        {
            _log.Warn($"Workflow {message.WorkflowId} not yet approved for network configuration step");
            return Task.CompletedTask;
        }

        _log.Info($"Network has been configured for workflow {message.WorkflowId}");

        Data.IsNetworkConfigured = true;

        return ProcessHardwareAndNetworking(context);
    }

    public Task Handle(DataCenterProcessed message, IMessageHandlerContext context)
    {
        if (!Data.IsHardwareAllocated || !Data.IsNetworkConfigured)
        {
            _log.Warn($"Workflow {message.WorkflowId} not yet ready for data center processing");
            return Task.CompletedTask;
        }

        if (Data.IsDataCenterProcessed)
        {
            _log.Warn($"Data center has already been processed for workflow {message.WorkflowId}");
            return Task.CompletedTask;
        }

        _log.Info($"Data Center has been processed for workflow {message.WorkflowId}");

        Data.IsDataCenterProcessed = true;
        Data.CompletedUtc = DateTime.UtcNow;

        var email = GetEmailCommand(NotificationType.WorkflowComplete);
        return context.Send(email);
    }
    #endregion

    #region Timeouts
    public Task Timeout(DataCenterSLAWarning timeout, IMessageHandlerContext context)
    {
        if (Data.IsDataCenterProcessed)
        {
            return Task.CompletedTask;
        }

        _log.Warn("Data Center SLA - Warning");
        var email = GetEmailCommand(NotificationType.DataCenterSLAWarning);
        return context.Send(email);
    }

    public Task Timeout(DataCenterSLAExpired timeout, IMessageHandlerContext context)
    {
        if (Data.IsDataCenterProcessed)
        {
            return Task.CompletedTask;
        }

        _log.Warn("Data Center SLA - Expired");
        var email = GetEmailCommand(NotificationType.DataCenterSLAExpired);
        return context.Send(email);
    }
    #endregion

    #region Private Methods
    private Task ProcessHardwareAndNetworking(IMessageHandlerContext context)
    {
        if (!Data.IsHardwareAllocated || !Data.IsNetworkConfigured)
        {
            return Task.CompletedTask;
        }

        // begin data center processing
        var email = GetEmailCommand(NotificationType.BeginDataCenter);
        var beginDataCenterProcessingTask = context.Send(email);

        // data center SLA prep
        var startUtc = DateTime.UtcNow;
        var dataCenterSLA = TimeSpan.FromMinutes(2); // would probably come from config
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
    {
        var to = type switch
        {
            NotificationType.WorkflowCreated => Data.UserEmail,
            NotificationType.GovernanceDenied => Data.UserEmail,
            NotificationType.WorkflowComplete => Data.UserEmail,
            NotificationType.BeginGovernance => "governance@example.com",
            NotificationType.BeginHardware => "hardware@example.com",
            NotificationType.BeginNetworking => "networking@example.com",
            NotificationType.BeginDataCenter => "datacenter@example.com",
            NotificationType.DataCenterSLAWarning => "datacenter@example.com",
            NotificationType.DataCenterSLAExpired => "datacenter@example.com",
            _ => throw new NotSupportedException(),
        };
        return new SendEmail
        {
            WorkflowId = Data.WorkflowId,
            Type = type,
            To = to,
            Subject = $"{type} - {Data.WorkflowId}",
        };
    }
    #endregion
}
