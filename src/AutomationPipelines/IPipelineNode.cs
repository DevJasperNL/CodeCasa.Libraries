namespace AutomationPipelines;

/// <summary>
/// Represents a node in a pipeline.
/// </summary>
public interface IPipelineNode<TState>
{
    /// <summary>
    /// If set to true, the node will pass its input to the output without processing it.
    /// </summary>
    bool PassThrough { get; set; }

    /// <summary>
    /// Sets the input state of the node. This will trigger the processing of the input.
    /// </summary>
    TState? Input { get; set; }

    /// <summary>
    /// Gets the output state of the node.
    /// </summary>
    TState? Output { get; }

    /// <summary>
    /// Notifies when a new output is produced by the node.
    /// </summary>
    IObservable<TState?> OnNewOutput { get; }
}