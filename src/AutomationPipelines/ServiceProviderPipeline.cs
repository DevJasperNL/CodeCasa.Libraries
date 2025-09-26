using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace AutomationPipelines;

/// <summary>
/// Represents a pipeline of nodes.
/// This pipeline implementation can use the service provider to resolve nodes.
/// </summary>
public class ServiceProviderPipeline<TState> : Pipeline<TState>
{
    private readonly IServiceProvider _serviceProvider;

    /// <inheritdoc />
    public ServiceProviderPipeline(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    /// <inheritdoc />
    public ServiceProviderPipeline(IServiceProvider serviceProvider, IEnumerable<IPipelineNode<TState>> nodes)
        : base(nodes)
    {
        _serviceProvider = serviceProvider;
    }

    /// <inheritdoc />
    public ServiceProviderPipeline(
        IServiceProvider serviceProvider,
        TState defaultState,
        IEnumerable<IPipelineNode<TState>> nodes,
        Action<TState> outputHandlerAction)
        : base(defaultState, nodes, outputHandlerAction)
    {
        _serviceProvider = serviceProvider;
    }

    /// <inheritdoc />
    public ServiceProviderPipeline(IServiceProvider serviceProvider, params IPipelineNode<TState>[] nodes)
        : base(nodes)
    {
        _serviceProvider = serviceProvider;
    }

    /// <inheritdoc />
    public ServiceProviderPipeline(IServiceProvider serviceProvider, TState defaultState, params IPipelineNode<TState>[] nodes)
        : base(defaultState, nodes)
    {
        _serviceProvider = serviceProvider;
    }

    /// <inheritdoc />
    public ServiceProviderPipeline(IServiceProvider serviceProvider, ILogger<Pipeline<TState>>? logger) : base(logger)
    {
        _serviceProvider = serviceProvider;
    }

    /// <inheritdoc />
    public ServiceProviderPipeline(IServiceProvider serviceProvider, IEnumerable<IPipelineNode<TState>> nodes, ILogger<Pipeline<TState>>? logger)
        : base(nodes, logger)
    {
        _serviceProvider = serviceProvider;
    }

    /// <inheritdoc />
    public ServiceProviderPipeline(
        IServiceProvider serviceProvider,
        TState defaultState,
        IEnumerable<IPipelineNode<TState>> nodes,
        Action<TState> outputHandlerAction, ILogger<Pipeline<TState>>? logger)
        : base(defaultState, nodes, outputHandlerAction, logger)
    {
        _serviceProvider = serviceProvider;
    }

    /// <summary>
    /// Registers a new node in the pipeline. The node will be created using the service provider.
    /// </summary>
    public override IPipeline<TState> RegisterNode<TNode>()
    {
        return RegisterNode(ActivatorUtilities.CreateInstance<TNode>(_serviceProvider));
    }
}