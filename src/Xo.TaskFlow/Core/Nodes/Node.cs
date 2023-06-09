namespace Xo.TaskFlow.Core;

/// <inheritdoc cref="INode"/>
public sealed class Node : BaseNode
{
	/// <summary>
	///   Initializes a new instance of <see cref="Node"/>. 
	/// </summary>
	public Node(
		IMsgFactory msgFactory,
		ILogger? logger = null,
		string? id = null,
		IWorkflowContext? context = null
	) : base(msgFactory, logger, id, context) { }
}
