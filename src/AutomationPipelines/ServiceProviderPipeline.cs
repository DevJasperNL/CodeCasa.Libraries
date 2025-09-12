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
    public ServiceProviderPipeline(IServiceProvider serviceProvider, ILogger<Pipeline<TState>>? logger) : base(logger)
    {
        _serviceProvider = serviceProvider;
    }

    /// <summary>
    /// Registers a new node in the pipeline. The node will be created using the service provider.
    /// </summary>
    public new IPipeline<TState> RegisterNode<TNode>() where TNode : IPipelineNode<TState>
    {
        return RegisterNode(ActivatorUtilities.CreateInstance<TNode>(_serviceProvider));
    }
}