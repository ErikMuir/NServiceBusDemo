using NServiceBus;
using NServiceBus.Logging;
using Demo.Workflow.Messages.Commands;

namespace Demo.Workflow.Handlers;

public class BeginDataCenterProcessingHandler : IHandleMessages<BeginDataCenterProcessing>
{
    static readonly ILog log = LogManager.GetLogger<BeginDataCenterProcessing>();

    public Task Handle(BeginDataCenterProcessing message, IMessageHandlerContext context)
    {
        log.Info($"Notifying Data Center team that workflow {message.WorkflowId} is ready for processing...");
        return Task.CompletedTask;
    }
}
