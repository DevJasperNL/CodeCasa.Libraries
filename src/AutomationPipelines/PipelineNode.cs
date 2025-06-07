using System.Reactive.Linq;
using System.Reactive.Subjects;

namespace AutomationPipelines;

/// <summary>
/// Implementation of <see cref="IPipelineNode{TState}"/> meant to manage its own output.
/// Has convenient protected methods to control the output and pass-through behavior.
/// </summary>
public abstract class PipelineNode<TState> : IPipelineNode<TState>
{
    private readonly Subject<TState?> _newOutputSubject = new();
    private TState? _input;
    private TState? _output;
    private bool _passThroughNextInput;
    private bool _passThrough;

    /// <inheritdoc />
    public IObservable<TState?> OnNewOutput => _newOutputSubject.AsObservable();

    /// <inheritdoc />
    public TState? Input
    {
        get => _input;
        set
        {
            _input = value;
            if (_passThroughNextInput)
            {
                PassThrough = true;
                return;
            }
            if (PassThrough)
            {
                SetOutputInternal(_input);
                return;
            }
            InputReceived(_input);
        }
    }

    /// <summary>
    /// Called when the input is received.
    /// </summary>
    protected virtual void InputReceived(TState? input)
    {
        // Ignore input by default.
    }

    /// <summary>
    /// Turns on pass-through mode for the node, meaning it will pass the input directly to the output without processing it.
    /// </summary>
    protected void PassInputThrough()
    {
        PassThrough = true;
    }

    /// <summary>
    /// Sets the output state of the node. This will trigger the processing of the input.
    /// If the node is disabled, it will be enabled when setting an output value.
    /// </summary>
    public TState? Output
    {
        get => _output;
        protected set
        {
            PassThrough = false;
            _passThroughNextInput = false;

            SetOutputInternal(value);
        }
    }

    /// <inheritdoc />
    public bool PassThrough
    {
        get => _passThrough;
        set
        {
            // Always reset _disableOnNextInput when Enabled is explicitly called.
            _passThroughNextInput = false;

            if (_passThrough == value)
            {
                return;
            }
            
            _passThrough = value;
            if (_passThrough)
            {
                SetOutputInternal(_input);
            }
        }
    }

    /// <summary>
    /// Changes the output state of the node and enables pass-through mode after the next input.
    /// This can be useful for nodes that should influence pipeline behavior once. For example a light switch or a motion sensor detection.
    /// </summary>
    protected void ChangeOutputAndTurnOnPassThroughOnNextInput(TState? output)
    {
        Output = output;
        TurnOnPassThroughOnNextInput();
    }

    /// <summary>
    /// Keeps the current output but enables pass-through mode after receiving the next input.
    /// This can be useful for nodes that  should influence pipeline behavior once. For example a light switch or a motion sensor detection.
    /// </summary>
    protected void TurnOnPassThroughOnNextInput()
    {
        if (PassThrough)
        {
            return;
        }

        _passThroughNextInput = true;
    }

    private void SetOutputInternal(TState? output)
    {
        _output = output;
        _newOutputSubject.OnNext(output);
    }
}