using System.Reflection;

namespace Xo.TaskFlow.Core;

/// <inheritdoc cref="IFunctitect"/>
public sealed class Functitect : IFunctitect
{
	private readonly IServiceProvider _serviceProvider;
	private static readonly Type _msgType = typeof(Msg<>);

	/// <summary>
	///   Initializes a new instance of <see cref="Functitect"/>.
	/// </summary>
	/// <param name="serviceProvider">Service provider used to retrived registered services by their type.</param>
	public Functitect(IServiceProvider serviceProvider)
		=> this._serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));

	/// <inheritdoc />
	public IFunctory Build<TService, TArg>(
		TArg arg,
		string? nextParamName = null
	)
		=> this.Build(typeof(TService), nextParamName: nextParamName, staticArgs: new object[] { arg! });

	public IFunctory Build<T>(string? nextParamName = null) => this.Build(typeof(T), nextParamName: nextParamName);

	/// <inheritdoc />
	public IFunctory Build(
		Type serviceType,
		string? methodName = null,
		string? nextParamName = null,
		object[]? staticArgs = null
	)
	{
		Func<IDictionary<string, IMsg>, Func<Task<IMsg?>>> functory = (args) => async () =>
			{
				var service = this.GetService(serviceType);

				// Validate that service has method to be invoked, represented by methodName...
				var methodInfo = GetMethodInfo(serviceType, methodName);

				// Get the parameter information for the method in question...
				var parameters = methodInfo.GetParameters();

				ValidateMethod(args, methodInfo, parameters, staticArgs);

				var arguments = GetArguments(args, parameters, staticArgs);

				object? result = null;

				if (TypeInspector.MethodHasReturnTypeOfTask(methodInfo))
				{
					var task = (Task)methodInfo.Invoke(service, arguments)!;
					await task;
					result = task.GetType().GetProperty("Result")?.GetValue(task);
				}
				else
				{
					result = methodInfo.Invoke(service, arguments);
				}

				return result is null ? null : CreateMsg(result, nextParamName);
			};

		return new AsyncFunctoryAdaptor(functory!).SetServiceType(serviceType);
	}


	public IAsyncFunctory BuildAsyncFunctory<T>(string? methodName = null)
	{
		Func<IDictionary<string, IMsg>, Func<Task<IMsg?>>> functory = (args) => async () =>
			{
				var serviceType = typeof(T);

				var service = this.GetService(serviceType);

				// Validate that service has method to be invoked, represented by methodName...
				var methodInfo = GetMethodInfo(serviceType, methodName);

				// Get the parameter information for the method in question...
				var parameters = methodInfo.GetParameters();

				ValidateMethod(args, methodInfo, parameters);

				var arguments = GetArguments(args, parameters);

				object? result = null;

				var task = (Task)methodInfo.Invoke(service, arguments)!;
				await task;
				result = task.GetType().GetProperty("Result")?.GetValue(task);

				return result == null ? null : CreateMsg(result, null);
			};

		// todo: clean this up...
		return new AsyncFunctoryAdaptor(functory!).SetServiceType(serviceType: typeof(T)).AsAsync();
	}

	public ISyncFunctory BuildSyncFunctory<T>(string? methodName = null)
	{
		Func<IDictionary<string, IMsg>, Func<IMsg?>> functory = (args) => () =>
			{
				var serviceType = typeof(T);

				var service = this.GetService(serviceType);

				// Validate that service has method to be invoked, represented by methodName...
				var methodInfo = GetMethodInfo(serviceType, methodName);

				// Get the parameter information for the method in question...
				var parameters = methodInfo.GetParameters();

				ValidateMethod(args, methodInfo, parameters);

				var arguments = GetArguments(args, parameters);

				object? result = methodInfo.Invoke(service, arguments);

				return result == null ? null : CreateMsg(result, null);
			};

		return new SyncFunctoryAdapter(functory!);
	}

	private object? GetService(Type serviceType)
		=> this._serviceProvider.GetService(serviceType) ?? throw new InvalidOperationException($"Service not found for service type {serviceType.Name}");

	// todo: this static arg business needs to go... it's a hack...	
	private static void ValidateMethod(
		in IDictionary<string, IMsg> arguments,
		in MethodInfo methodInfo,
		in IEnumerable<ParameterInfo> parameters,
		in object[]? staticArgs = null
	)
	{
		if (parameters.Count() != arguments.Count() && parameters.Count() != arguments.Count() + (staticArgs?.Count() ?? 0))
		{
			throw new ArgumentException(
				$"Invalid parameters for method {methodInfo.Name}. " +
				$"Arguments provided: {string.Join(",", arguments.Select(p => p.Key))}, " +
				$"Parameters expected: {string.Join(",", parameters.Select(p => p.Name))}"
			);
		}
	}

	public static MethodInfo GetMethodInfo(
		Type type,
		string? methodName = null
	)
		=> methodName switch
		{
			null => type.GetMethods().First(),
			_ => type.GetMethod(methodName!) ?? throw new InvalidOperationException($"{type.Name} does not have method of name {methodName}")
		};

	private static object[] GetArguments(
		IDictionary<string, IMsg> arguments,
		IEnumerable<ParameterInfo> parameters,
		object[]? staticArgs = null
	)
	{
		if (staticArgs is null) return parameters.Select(p => arguments[p.Name!].ObjectData).ToArray();

		object[] finalArgs = new object[parameters.Count()];
		foreach (var p in parameters)
		{
			if (arguments.TryGetValue(p.Name!, out IMsg? msg))
			{
				finalArgs[p.Position] = msg.ObjectData;
			}
			else
			{
				var staticArg = staticArgs.FirstOrDefault(a => a.GetType() == p.ParameterType);
				if (staticArg is not null)
				{
					finalArgs[p.Position] = staticArg;
					continue;
				}
				throw new ArgumentException($"No argument found for parameter {p.Name} of type {p.ParameterType.Name}");
			}
		}

		return finalArgs;
	}

	public static IMsg CreateMsg(object result, string? nextParamName)
	{
		var resultType = result.GetType();

		var constructedType = _msgType.MakeGenericType(resultType);

		var arguments = nextParamName == null
			? new object[] { result }
			: new object[] { result, nextParamName };

		object? instance = Activator.CreateInstance(constructedType, arguments);

		return (IMsg)instance!;
	}
}