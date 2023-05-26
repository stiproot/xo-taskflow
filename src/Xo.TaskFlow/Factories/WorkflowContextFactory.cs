namespace Xo.TaskFlow.Factories;

/// <inheritdoc cref="IWorkflowContextFactory"/>
public class WorkflowContextFactory : IWorkflowContextFactory
{
	/// <inheritdoc />
	public IWorkflowContext Create() => new WorkflowContext();
}
