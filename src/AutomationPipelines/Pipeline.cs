using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace AutomationPipelines;

/// <summary>
/// Represents a pipeline of nodes.
/// </summary>
public class Pipeline<TState> : PipelineNode<TState>, IPipeline<TState>
{
    private readonly List<IPipelineNode<TState>> _nodes = new();
    private ILogger<Pipeline<TState>>? _logger;

    private bool _callActionDistinct = true;
    private Action<TState>? _action;
    private IDisposable? _subscription;
    private readonly IServiceProvider _serviceProvider;

    /// <inheritdoc />
    public Pipeline(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    /// <inheritdoc />
    public Pipeline(IServiceProvider serviceProvider, ILogger<Pipeline<TState>>? logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    /// <inheritdoc />
    public IPipeline<TState> SetDefault(TState state)
    {
        Input = state;
        return this;
    }

    /// <inheritdoc />
    public IPipeline<TState> RegisterNode<TNode>() where TNode : IPipelineNode<TState>
    {
        return RegisterNode(ActivatorUtilities.CreateInstance<TNode>(_serviceProvider));
    }

    /// <inheritdoc />
    public IPipeline<TState> RegisterNode<TNode>(TNode node) where TNode : IPipelineNode<TState>
    {
        _subscription?.Dispose(); // Dispose old subscription if any.
        _subscription = node.OnNewOutput.Subscribe(SetOutputAndCallActionWhenApplicable);

        if (_nodes.Any())
        {
            var previousNode = _nodes.Last();
            previousNode.OnNewOutput.Subscribe(output =>
            {
                node.Input = output;
                _logger?.LogTrace($"Node {previousNode} passed value {output} to node {node}.");
            });

            node.Input = previousNode.Output;
        }

        _nodes.Add(node);

        _logger?.LogDebug($"Node {node} registered.");

        if (_nodes.Count == 1)
        {
            node.Input = Input;
        }

        SetOutputAndCallActionWhenApplicable(node.Output);

        return this;
    }

    /// <inheritdoc />
    public IPipeline<TState> SetOutputHandler(Action<TState> action, bool callActionDistinct = true)
    {
        _logger?.LogDebug(callActionDistinct
            ? "Setting output handler."
            : "Setting output handler. Action calls with duplicate values are allowed.");

        _callActionDistinct = callActionDistinct;
        _action = action;
        if (Output != null)
        {
            _action(Output);
        }

        return this;
    }

    /// <inheritdoc />
    protected override void InputReceived(TState? state)
    {
        _logger?.LogDebug($"Input set to {state}.");
        if (!_nodes.Any())
        {
            _logger?.LogTrace($"No nodes, calling action immediately with {state}.");
            SetOutputAndCallActionWhenApplicable(Input);
            return;
        }

        var firstNode = _nodes.First();
        _logger?.LogTrace($"Passing input {state} to first node {firstNode}.");
        firstNode.Input = Input;
    }

    private void SetOutputAndCallActionWhenApplicable(TState? output)
    {
        var outputChanged = !EqualityComparer<TState>.Default.Equals(Output, output);

        Output = output;
        _logger?.LogDebug($"Pipeline output set to {output}.");

        if (output == null)
        {
            _logger?.LogTrace("No action set to execute.");
            return;
        }

        if (_callActionDistinct && !outputChanged)
        {
            _logger?.LogTrace("No action executed as output has not changed.");
            return;
        }

        // Note that _action will be called AFTER OnNewOutput.
        _action?.Invoke(output);
        _logger?.LogTrace($"Action executed with value {output}.");
    }
}