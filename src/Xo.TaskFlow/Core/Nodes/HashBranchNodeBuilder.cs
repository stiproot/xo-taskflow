namespace Xo.TaskFlow.Abstractions;

public class HashBranchNodeBuilder : BaseNodeBuilder, IHashBranchNodeBuilder
{
	protected readonly IDictionary<string, INode> _Hash = new Dictionary<string, INode>();

	public virtual IHashBranchNodeBuilder AddNext(string key, INode node)
	{
		this._Hash.Add(key, node ?? throw new ArgumentNullException(nameof(node)));
		return this;
	}

	public virtual IHashBranchNodeBuilder AddNext<T>(string key)
	{
		var n = this.Build(typeof(T));
		this._Hash.Add(key, n);
		return this;
	}

	public override INode Build()
	{
		var n = this.BuildBase() as IHashBranchNode;

		n!
			.SetHash(this._Hash);

		return n;
	}

	/// <summary>
	///   Initializes a new instance of <see cref="HashBranchNodeBuilder"/>. 
	/// </summary>
	public HashBranchNodeBuilder(
		IFunctitect functitect,
		INodeFactory nodeFactory,
		IMsgFactory msgFactory,
		ILogger? logger = null,
		string? id = null,
		IWorkflowContext? context = null
	) : base(functitect, nodeFactory, msgFactory, logger, id, context) => this._NodeType = NodeTypes.Hash;
}
