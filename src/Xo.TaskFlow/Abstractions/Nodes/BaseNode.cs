namespace Xo.TaskFlow.Abstractions;

/// <inheritdoc cref="INode"/>
public abstract class BaseNode : INode
{
	protected readonly IList<IMsg> _Params = new List<IMsg>();
	protected readonly List<INode> _PromisedParams = new List<INode>();
	protected readonly IList<Func<IWorkflowContext, IMsg>> _ContextParams = new List<Func<IWorkflowContext, IMsg>>();
	protected INodevaluator _Nodevaluator = new ParallelNodeEvaluator();
	protected readonly IMsgFactory _MsgFactory;
	protected IAsyncFunctory? _AsyncFunctory;
	protected ISyncFunctory? _SyncFunctory;
	protected ILogger? _Logger;
	protected IWorkflowContext? _Context;
	protected Func<Exception, Task>? _ExceptionHandlerAsync;
	protected Action<Exception>? _ExceptionHandler;

	/// <inheritdoc />
	public string Id { get; internal set; } = $"{Guid.NewGuid()}";

	/// <inheritdoc />
	public bool HasParam(string paramName) => this._Params.Any(p => p.ParamName == paramName);

	/// <inheritdoc />
	public bool RequiresResult { get; internal set; }

	/// <inheritdoc />
	public IFunctory Functory => this._AsyncFunctory is not null ? (IFunctory)this._AsyncFunctory! : (IFunctory)this._SyncFunctory!;

	/// <inheritdoc />
	public bool IsSync => this._SyncFunctory != null;

	/// <inheritdoc />
	public INode SetNodevaluator(INodevaluator nodevaluator)
	{
		this._Nodevaluator = nodevaluator ?? throw new ArgumentNullException(nameof(nodevaluator));
		return this;
	}

	/// <inheritdoc />
	public INode RunNodesInLoop()
	{
		this.SetNodevaluator(new LoopNodeEvaluator());
		return this;
	}

	/// <inheritdoc />
	public INode SetFunctory(IAsyncFunctory functory)
	{
		this._AsyncFunctory = functory ?? throw new ArgumentNullException(nameof(functory));
		return this;
	}

	/// <inheritdoc />
	public INode SetFunctory(Func<IDictionary<string, IMsg>, Func<Task<IMsg?>>> fn)
	{
		this._AsyncFunctory = new AsyncFunctoryAdaptor(fn);
		return this;
	}

	/// <inheritdoc />
	public INode SetFunctory(ISyncFunctory functory)
	{
		this._SyncFunctory = functory ?? throw new ArgumentNullException(nameof(functory));
		return this;
	}

	/// <inheritdoc />
	public INode SetFunctory(Func<IDictionary<string, IMsg>, Func<IMsg?>> fn)
	{
		this._SyncFunctory = new SyncFunctoryAdapter(fn);
		return this;
	}

	/// <inheritdoc />
	public INode SetFunctory(Func<IWorkflowContext, Func<IMsg>> fn)
	{
		this._SyncFunctory = new SyncFunctoryAdapter(fn);
		return this;
	}

	/// <inheritdoc />
	public INode SetContext(IWorkflowContext? context)
	{
		this._Context = context;
		return this;
	}

	/// <inheritdoc />
	public INode SetId(string id)
	{
		this.Id = id;
		return this;
	}

	/// <inheritdoc />
	public INode SetLogger(ILogger logger)
	{
		this._Logger = logger ?? throw new ArgumentNullException(nameof(logger));
		return this;
	}

	/// <inheritdoc />
	public INode AddArg(params INode[] nodes)
	{
		foreach (var h in nodes)
		{
			this._PromisedParams.Add(h);
		}
		return this;
	}

	/// <inheritdoc />
	public INode AddArg(params IMsg[] msgs)
	{
		foreach (var m in msgs)
		{
			this._Params.Add(m);
		}
		return this;
	}

	/// <inheritdoc />
	public INode AddArg<T>(
		T data,
		string paramName
	)
	{
		if (data is null || paramName is null) throw new InvalidOperationException("Null values cannot be passed into AddArg<T>...");
		this._Params.Add(this._MsgFactory.Create<T>(data, paramName));
		return this;
	}

	/// <inheritdoc />
	public INode AddArg(params Func<IWorkflowContext, IMsg>[] contextArgs)
	{
		foreach (var p in contextArgs)
		{
			this._ContextParams.Add(p);
		}
		return this;
	}

	/// <inheritdoc />
	public INode SetExceptionHandler(Func<Exception, Task> handler)
	{
		this._ExceptionHandlerAsync = handler;
		return this;
	}

	/// <inheritdoc />
	public INode SetExceptionHandler(Action<Exception> handler)
	{
		this._ExceptionHandler = handler;
		return this;
	}

	/// <inheritdoc />
	public virtual async Task<IMsg?> Run(CancellationToken cancellationToken)
	{
		this._Logger?.LogTrace($"Node.Run - start.");

		cancellationToken.ThrowIfCancellationRequested();

		this.Validate();

		await this.ResolvePromisedParams(cancellationToken);

		this.AddContextParamResultsToParams();

		try
		{
			return await this.ResolveFunctory(cancellationToken);
		}
		catch (Exception ex)
		{
			await HandleException(ex);
			throw;
		}
	}

	/// <inheritdoc />
	public virtual void Validate()
	{
		this._Logger?.LogTrace($"Node.Validate - start.");

		if (this._AsyncFunctory is null && this._SyncFunctory is null)
		{
			this._Logger?.LogError($"Node validation failed.");
			throw new InvalidOperationException("Strategy factory has not been set.");
		}

		this._Logger?.LogError($"Node.Validate - end.");
	}

	/// <inheritdoc />
	public virtual async Task ResolvePromisedParams(CancellationToken cancellationToken)
	{
		this._Logger?.LogTrace($"Node.ResolvePromisedParams - running param nodes.");

		// Are there any async operations, in the form of INodes, that need to run in order to provide params to our functory?
		if (!this._PromisedParams.Any()) return;

		var results = await this._Nodevaluator.RunAsync(this._PromisedParams, cancellationToken);

		// We are only interested in adding non-null results to our params 
		// If the result (IMsg) is null the Task was void.
		IEnumerable<IMsg> nonNullResults = results.Where(p => p is not null && p.HasParam).ToList()!;

		// Let's add the results to our list of params, for our functory.
		foreach (var r in nonNullResults)
		{
			this._Params.Add(r);
		}
	}

	/// <inheritdoc />
	public virtual void AddContextParamResultsToParams()
	{
		this._Logger?.LogTrace($"Node.AddContextParamResultsToParams - start.");

		// Are there any params that need to be extracted from the shared context?
		if (!this._ContextParams.Any())
		{
			return;
		}

		if (this._Context == null)
		{
			throw new InvalidOperationException("Context has not been provided");
		}

		foreach (var f in this._ContextParams)
		{
			this._Params.Add(f(this._Context));
		}

		this._Logger?.LogTrace($"Node.AddContextParamResultsToParams - end.");
	}

	/// <inheritdoc />
	public virtual async Task<IMsg?> ResolveFunctory(CancellationToken cancellationToken)
	{
		this._Logger?.LogTrace($"BaseNode.ResolveFunctory - starting...");

		var paramDic = this._Params.ToDictionary(p => p.ParamName!);

		var result = this.IsSync
			? this._SyncFunctory!.CreateFunc(paramDic, this._Context)()
			: await this._AsyncFunctory!.CreateFunc(paramDic, this._Context)();

		if (result is not null && this._Context is not null)
		{
			this._Context.AddMsg(this.Id, result);
		}

		return result;
	}

	/// <inheritdoc />
	public virtual async Task HandleException(Exception ex)
	{
		if (this._ExceptionHandlerAsync != null)
		{
			await this._ExceptionHandlerAsync(ex);
		}

		if (this._ExceptionHandler != null)
		{
			this._ExceptionHandler(ex);
		}
	}

	/// <inheritdoc />
	public virtual INode RequireResult(bool requiresResult = true)
	{
		this.RequiresResult = requiresResult;
		return this;
	}

	/// <summary>
	///   Initializes a new instance of <see cref="Node"/>. 
	/// </summary>
	public BaseNode(
		IMsgFactory msgFactory,
		ILogger? logger = null,
		string? id = null,
		IWorkflowContext? context = null
	)
	{
		this._MsgFactory = msgFactory ?? throw new ArgumentNullException(nameof(msgFactory));
		this._Logger = logger;

		if (id is not null) this.Id = id;
		this._Context = context;
	}
}
