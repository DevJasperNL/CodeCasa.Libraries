using Microsoft.Extensions.Logging;
using System.Xml.Linq;

namespace AutomationPipelines;

/// <summary>
/// Represents a pipeline of nodes.
/// </summary>
public class Pipeline<TState> : PipelineNode<TState>, IPipeline<TState>
{
    private readonly List<IPipelineNode<TState>> _nodes = new();
    private readonly ILogger<Pipeline<TState>>? _logger;

    private bool _callActionDistinct = true;
    private Action<TState>? _action;
    private IDisposable? _subscription;

    /// <summary>
    /// Initializes a new, empty pipeline with no nodes.
    /// </summary>
    public Pipeline()
    {
    }

    /// <summary>
    /// Initializes a new pipeline with the specified nodes.
    /// </summary>
    public Pipeline(IEnumerable<IPipelineNode<TState>> nodes)
        : this(nodes, null)
    {
    }

    /// <summary>
    /// Initializes a new pipeline with the specified default state, nodes, and output handler.
    /// </summary>
    public Pipeline(TState defaultState, IEnumerable<IPipelineNode<TState>> nodes, Action<TState> outputHandlerAction)
        : this(defaultState, nodes, outputHandlerAction, null)
    {
    }

    /// <summary>
    /// Initializes a new pipeline with the specified nodes.
    /// </summary>
    public Pipeline(params IPipelineNode<TState>[] nodes)
    {
        foreach (var node in nodes)
        {
            RegisterNode(node);
        }
    }

    /// <summary>
    /// Initializes a new pipeline with the specified default state and nodes.
    /// </summary>
    public Pipeline(TState defaultState, params IPipelineNode<TState>[] nodes)
    {
        foreach (var node in nodes)
        {
            RegisterNode(node);
        }

        SetDefault(defaultState);
    }
    
    /// <summary>
    /// Initializes a new, empty pipeline with an optional logger.
    /// </summary>
    public Pipeline(ILogger<Pipeline<TState>>? logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// Initializes a new pipeline with the specified nodes and an optional logger.
    /// </summary>
    public Pipeline(IEnumerable<IPipelineNode<TState>> nodes, ILogger<Pipeline<TState>>? logger)
    {
        _logger = logger;
        foreach (var node in nodes)
        {
            RegisterNode(node);
        }
    }

    /// <summary>
    /// Initializes a new pipeline with the specified default state, nodes, output handler, and an optional logger.
    /// </summary>
    public Pipeline(TState defaultState, IEnumerable<IPipelineNode<TState>> nodes, Action<TState> outputHandlerAction, ILogger<Pipeline<TState>>? logger)
    {
        _logger = logger;
        foreach (var node in nodes)
        {
            RegisterNode(node);
        }

        SetDefault(defaultState);
        SetOutputHandler(outputHandlerAction);
    }

    /// <inheritdoc />
    public IPipeline<TState> SetDefault(TState state)
    {
        Input = state;
        return this;
    }

    /// <summary>
    /// Registers a new node in the pipeline. The node will be created using the type's parameterless constructor.
    /// </summary>
    public virtual IPipeline<TState> RegisterNode<TNode>() where TNode : IPipelineNode<TState>
    {
        return RegisterNode((TNode)Activator.CreateInstance(typeof(TNode))!);
    }

    /// <inheritdoc />
    public IPipeline<TState> RegisterNode(IPipelineNode<TState> node)
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