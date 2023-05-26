namespace Xo.TaskFlow.Abstractions;

public interface ILinkedBranchNodeBuilder : IBranchNodeBuilder
{
	ILinkedBranchNodeBuilder SetNext<T>(bool requiresResult = true);
	ILinkedBranchNodeBuilder SetNext(INode node);
}