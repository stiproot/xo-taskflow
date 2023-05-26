namespace Xo.TaskFlow.Core;

public class NotNullBinaryBranchNodePathResolver : IBinaryBranchNodePathResolver
{
	public bool Resolve(IMsg? msg) => msg is not null;
}