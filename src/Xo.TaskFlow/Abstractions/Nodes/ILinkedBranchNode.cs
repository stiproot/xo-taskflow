namespace Xo.TaskFlow.Abstractions;

public interface ILinkedBranchNode : IBranchNode
{
	ILinkedBranchNode SetNext(INode node);
	// ILinkedBranchNode SetNext<T>(bool requiresResult = true);
}