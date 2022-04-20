using NServiceBus;
using NServiceBus.Logging;
using Demo.Workflow.Messages.Commands;
using Demo.Workflow.Messages.Events;
using Demo.Workflow.Messages.Timeouts;

namespace Demo.Workflow.Sagas;

public class WorkflowSaga :
    Saga<WorkflowSagaData>,
    IAmStartedByMessages<BeginWorkflow>,
    IHandleMessages<QuestionnaireSubmitted>,
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
            .ToMessage<QuestionnaireSubmitted>(msg => msg.WorkflowId)
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
        Data.CreatedBy = message.User;
        Data.CreatedUtc = DateTime.UtcNow;

        var workflowCreated = new WorkflowCreated(message.WorkflowId, message.User);
        return context.Publish(workflowCreated);
    }

    public Task Handle(QuestionnaireSubmitted message, IMessageHandlerContext context)
    {
        _log.Info($"Questionnaire submitted for workflow {message.WorkflowId}");

        Data.IsQuestionnaireSubmitted = true;
        Data.HasGovernanceApproval = null;

        var beginGovernanceReview = new BeginGovernanceReview(message.WorkflowId);
        return context.SendLocal(beginGovernanceReview);
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

        var notifyGovernanceDenial = new NotifyGovernanceDenial(message.WorkflowId, Data.CreatedBy);
        return context.SendLocal(notifyGovernanceDenial);
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

        var beginEngagementReview = new BeginHardwareAndNetworking(message.WorkflowId);
        return context.SendLocal(beginEngagementReview);
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

        var workflowComplete = new WorkflowCompleted(message.WorkflowId, Data.CompletedUtc, Data.CreatedBy);
        return context.Publish(workflowComplete);
    }
    #endregion

    #region Timeouts
    public Task Timeout(DataCenterSLAWarning timeout, IMessageHandlerContext context)
    {
        if (!Data.IsDataCenterProcessed)
        {
            _log.Warn($"Alert!! The Data Center's SLA for workflow {timeout.WorkflowId} will expire {timeout.ExpiresUtc}");
        }

        return Task.CompletedTask;
    }

    public Task Timeout(DataCenterSLAExpired timeout, IMessageHandlerContext context)
    {
        if (!Data.IsDataCenterProcessed)
        {
            _log.Warn($"Alert!! The Governance team's SLA for workflow {timeout.WorkflowId} expired {timeout.ExpiresUtc}");
        }

        return Task.CompletedTask;
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
        var beginDataCenterProcessing = new BeginDataCenterProcessing(Data.WorkflowId);
        var beginDataCenterProcessingTask = context.SendLocal(beginDataCenterProcessing);

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
    #endregion
}
