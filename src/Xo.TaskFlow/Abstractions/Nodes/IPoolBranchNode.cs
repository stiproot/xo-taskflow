namespace Xo.TaskFlow.Abstractions;

public interface IPoolBranchNode : IBranchNode
{
	IPoolBranchNode AddNext(INode node);
	IPoolBranchNode AddNext(params INode[] node);
}