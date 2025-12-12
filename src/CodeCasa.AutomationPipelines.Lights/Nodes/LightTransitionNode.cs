using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using AutomationPipelines;
using CodeCasa.AutomationPipelines.Lights.Extensions;
using CodeCasa.Lights;

namespace CodeCasa.AutomationPipelines.Lights.Nodes
{
    public abstract class LightTransitionNode(IScheduler scheduler) : IPipelineNode<LightTransition>
    {
        private readonly Subject<LightTransition?> _newOutputSubject = new();
        private LightTransition? _input;
        private LightParameters? _inputLightDestinationParameters;
        private DateTime? _inputStartOfTransition;
        private DateTime? _inputEndOfTransition;
        private LightTransition? _output;
        private bool _passThroughNextInput;
        private bool _passThrough;
        private IDisposable? _scheduledAction;

        protected LightParameters? InputLightSourceParameters { get; private set; }

        /// <inheritdoc />
        public IObservable<LightTransition?> OnNewOutput => _newOutputSubject.AsObservable();

        /// <inheritdoc />
        public LightTransition? Input
        {
            get => _input;
            set
            {
                _scheduledAction?.Dispose(); // Always cancel scheduled actions when the input changes.
                // We save additional information on the light transition that we can later use to continue the transition if it would be interrupted.
                InputLightSourceParameters = _inputLightDestinationParameters;
                _input = value;
                _inputLightDestinationParameters = value?.LightParameters;
                var transitionTime = value?.TransitionTime;
                _inputStartOfTransition = DateTime.UtcNow;
                _inputEndOfTransition = transitionTime == null ? null : _inputStartOfTransition + transitionTime;

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
        protected virtual void InputReceived(LightTransition? input)
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
        public LightTransition? Output
        {
            get => _output;
            protected set
            {
                _scheduledAction?.Dispose(); // Always cancel scheduled actions when the output is changed directly.
                PassThrough = false;

                SetOutputInternal(value);
            }
        }

        protected void ScheduleInterpolatedLightTransitionUsingInputTransitionTime(LightParameters? sourceLightParameters, LightParameters? desiredLightParameters)
        {
            PassThrough = false;
            _scheduledAction = scheduler.ScheduleInterpolatedLightTransition(sourceLightParameters,
                desiredLightParameters, _inputStartOfTransition, _inputEndOfTransition, SetOutputInternal);
        }

        public bool PassThrough
        {
            get => _passThrough;
            set
            {
                // Always reset _passThroughNextInput when PassThrough is explicitly called.
                _passThroughNextInput = false;

                if (_passThrough == value)
                {
                    return;
                }

                _scheduledAction?.Dispose(); // Always cancel scheduled actions when the pass through value changes.

                _passThrough = value;
                if (_passThrough)
                {
                    _scheduledAction = scheduler.ScheduleInterpolatedLightTransition(InputLightSourceParameters,
                        _inputLightDestinationParameters, _inputStartOfTransition, _inputEndOfTransition, SetOutputInternal);
                }
            }
        }

        /// <summary>
        /// Changes the output state of the node and enables pass-through mode after the next input.
        /// This can be useful for nodes that should influence pipeline behavior once. For example a light switch or a motion sensor detection.
        /// </summary>
        protected void ChangeOutputAndTurnOnPassThroughOnNextInput(LightTransition? output)
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

        private void SetOutputInternal(LightTransition? output)
        {
            _output = output;
            _newOutputSubject.OnNext(output);
        }

        /// <inheritdoc />
        public override string ToString() => GetType().Name;
    }
}
