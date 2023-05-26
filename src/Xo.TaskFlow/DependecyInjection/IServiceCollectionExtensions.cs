using Xo.TaskFlow.Abstractions;
using Xo.TaskFlow.Core;
using Xo.TaskFlow.Factories;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Xo.TaskFlow.DependencyInjection.Extensions;

/// <summary>
///   <see cref="IServiceCollection"/> extension methods.
/// </summary>
public static class IServiceCollectionExtensions
{
	/// <summary>
	///   Add all Xo.TaskFlow services to <see cref="IServiceCollection"/>.
	/// </summary>
	/// <returns><see cref="IServiceCollection"/></returns>
	public static IServiceCollection AddTaskWorkflowEngineServices(this IServiceCollection services)
	{
		services.TryAddSingleton<IFunctitect, Functitect>();
		services.TryAddSingleton<INodeBuilderFactory, NodeBuilderFactory>();
		services.TryAddSingleton<INodeFactory, NodeFactory>();
		services.TryAddSingleton<IMsgFactory, MsgFactory>();
		services.TryAddSingleton<IWorkflowContextFactory, WorkflowContextFactory>();

		return services;
	}
}
