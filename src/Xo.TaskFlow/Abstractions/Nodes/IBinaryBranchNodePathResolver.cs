namespace Xo.TaskFlow.Abstractions;

public interface IBinaryBranchNodePathResolver
{
	bool Resolve(IMsg? msg);
}