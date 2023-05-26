namespace Xo.TaskFlow.Abstractions;

public interface IHashBranchNode : IBranchNode
{
	IHashBranchNode AddNext(string key, INode node);
	IHashBranchNode SetHash(IDictionary<string, INode> hash);
}