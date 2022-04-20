using NServiceBus;
using NServiceBus.Logging;
using Demo.Workflow.Messages.Commands;

namespace Demo.Workflow.Handlers;

public class BeginGovernanceReviewHandler : IHandleMessages<BeginGovernanceReview>
{
    static readonly ILog log = LogManager.GetLogger<BeginGovernanceReview>();

    public Task Handle(BeginGovernanceReview message, IMessageHandlerContext context)
    {
        log.Info($"Notifying Governance Review team that workflow {message.WorkflowId} is ready for review...");
        return Task.CompletedTask;
    }
}
