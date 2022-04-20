namespace Demo.Workflow.Messages.Events;

public class QuestionnaireSubmitted : WorkflowEvent
{
    public QuestionnaireSubmitted(Guid workflowId) : base(workflowId) { }
}
