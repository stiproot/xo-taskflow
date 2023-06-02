namespace Xo.TaskFlow.Abstractions;

internal static class FlowBuilderExtensions
{
    public static void Build()
    {
        IFlowBuilder builder = null!;

        var flow = builder
            .FromRoot<IService>()
                .If<IService>().With(c => c.RequireResult())
                .Then<IService>()
                .Else<IService>();

        var flow2 = builder
            .FromRoot<IService>()
                .If<IService>(b => b.Then<IService>().Else<IService>())
                .Then<IService>()
                .Else<IService>();
    }
}

internal interface IFlowBuilder : ICoreFlowBuilder
{
    IFlowBuilder FromRoot<T>();
    IFlowBuilder If<T>();
    IFlowBuilder With(Action<INodeConfigurationBuilder> config);
    IFlowBuilder If<T>(Action<IFlowBuilder> builder);
    IFlowBuilder If<T>(Action<IFlowBuilder> then, Action<IFlowBuilder> @else);
    IFlowBuilder Then<T>();
    IFlowBuilder Else<T>();
}

internal interface INodeConfigurationBuilder
{
    INodeConfigurationBuilder RequireResult();
}

internal interface INodeConfiguration
{
    bool RequiresResult { get; init; }
}

internal interface ICoreFlowBuilder { }

internal interface IService
{
    Task<long> GetLongAsync(string query);
}