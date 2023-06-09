namespace Xo.TaskFlow.Abstractions;

public interface IBinaryBranchNode : IBranchNode
{
	IBinaryBranchNode AddTrue(INode? node);
	IBinaryBranchNode AddFalse(INode? node);
	IBinaryBranchNode AddPathResolver(IBinaryBranchNodePathResolver? pathResolver);
}