using System.Reactive.Concurrency;
using CodeCasa.AutomationPipelines.Lights.Nodes;
using CodeCasa.Lights;

namespace CodeCasa.AutomationPipelines.Lights.ReactiveNode
{
    // todo: investigate why off is sent twice in a row
    public class DimmingContext(
        (string entityId, LightParameters? parametersAfterDim)[] dimmerNodeOutputParametersInOrder)
    {
        public (string entityId, LightParameters? parametersAfterDim)[] DimmerNodeOutputParametersInOrder { get; } = dimmerNodeOutputParametersInOrder;
    }

    internal class ReactiveDimmerNode : LightTransitionNode
    {
        private readonly int _minBrightness;
        private readonly int _brightnessStep;
        private int _dimSteps; // negative is dimming, positive is brightening.

        public ReactiveDimmerNode(
            ReactiveNode reactiveNode,
            string lightEntityId,
            int minBrightness, 
            int brightnessStep, 
            IScheduler scheduler) : base(scheduler)
        {
            reactiveNode.NodeChanged.Subscribe(_ => Reset());
            PassThrough = true;

            LightEntityId = lightEntityId;
            _minBrightness = minBrightness;
            _brightnessStep = brightnessStep;
        }

        public string LightEntityId { get; }

        public void Reset()
        {
            PassThrough = true;
            _dimSteps = 0;
        }

        protected override void InputReceived(LightTransition? input)
        {
            if (input == null)
            {
                Output = null;
                return;
            }

            var newBrightness = CalculateBrightness(input.LightParameters.Brightness ?? 0);
            Output = input with { LightParameters = input.LightParameters with { Brightness = newBrightness } };
        }

        public void DimStep(DimmingContext context)
        {
            if (!ShouldDim(context))
            {
                return;
            }
            _dimSteps--;
            if (_dimSteps == 0)
            {
                PassThrough = true;
                return;
            }

            ScheduleInterpolatedLightTransition();
        }

        public void BrightenStep(DimmingContext context)
        {
            if (!ShouldBrighten(context))
            {
                return;
            }
            _dimSteps++;
            if (_dimSteps == 0)
            {
                PassThrough = true;
                return;
            }

            ScheduleInterpolatedLightTransition();
        }

        private bool ShouldDim(DimmingContext context)
        {
            var subjectParameters = context.DimmerNodeOutputParametersInOrder.Single(x => x.entityId == LightEntityId).parametersAfterDim;
            var subjectBrightness = subjectParameters?.Brightness ?? 0;
            if (subjectBrightness > _minBrightness)
            {
                // If we are brighter than minimum brightness, we have to dim anyway.
                return true;
            }
            if (subjectBrightness <= 0)
            {
                return false;
            }
            // At this point we are at min brightness and have to check if any other light is going to dim. If so, we don't have to.
            string? lightToTurnOff = null;
            foreach (var (entityId, parametersAfterDim) in context.DimmerNodeOutputParametersInOrder)
            {
                var brightness = parametersAfterDim?.Brightness ?? 0;
                if (brightness == 0)
                {
                    continue;
                }
                if (brightness > _minBrightness)
                {
                    // If any light is brighter than MinBrightness, we let them dim first.
                    return false;
                }

                lightToTurnOff ??= entityId;
            }

            return lightToTurnOff == LightEntityId;
        }

        private bool ShouldBrighten(DimmingContext context)
        {
            var subjectParameters = context.DimmerNodeOutputParametersInOrder.Single(x => x.entityId == LightEntityId).parametersAfterDim;
            var subjectBrightness = subjectParameters?.Brightness ?? 0;
            if (subjectBrightness > _minBrightness)
            {
                // If we are brighter than minimum brightness, we have to brighten anyway.
                return subjectBrightness < byte.MaxValue;
            }
            // At this point we are either off or at min brightness and have to check if any other light is going to turn on. If so, we don't have to turn on or brighten.
            string? lightToTurnOn = null;
            foreach (var (entityId, parametersAfterDim) in context.DimmerNodeOutputParametersInOrder.Reverse())
            {
                var brightness = parametersAfterDim?.Brightness ?? 0;
                if (brightness >= _minBrightness) // On
                {
                    if (lightToTurnOn != null || brightness > _minBrightness)
                    {
                        return false;
                    }
                    continue;
                }

                lightToTurnOn ??= entityId;
            }

            return lightToTurnOn == null || lightToTurnOn == LightEntityId;
        }

        private void ScheduleInterpolatedLightTransition()
        {
            if (Input == null)
            {
                Output = new LightParameters { Brightness = CalculateBrightness(0) }.AsTransition();
            }
            else
            {
                ScheduleInterpolatedLightTransitionUsingInputTransitionTime(
                    InputLightSourceParameters == null
                        ? null
                        : InputLightSourceParameters with
                        {
                            Brightness = CalculateBrightness(InputLightSourceParameters.Brightness ?? 0)
                        },
                    Input.LightParameters with
                    {
                        Brightness = CalculateBrightness(Input.LightParameters.Brightness ?? 0)
                    });
            }
        }

        private double? CalculateBrightness(double inputBrightness)
        {
            var calculatedBrightness = inputBrightness + _brightnessStep * _dimSteps;
            if (_dimSteps < 0)
            {
                // Make sure we always show minimum brightness before turning off.
                if (calculatedBrightness <= _minBrightness && calculatedBrightness + _brightnessStep > _minBrightness)
                {
                    return _minBrightness;
                }
            }
            if (_dimSteps > 0)
            {
                // Make sure we always show minimum brightness after turning on.
                if (calculatedBrightness > _minBrightness && calculatedBrightness - _brightnessStep <= _minBrightness)
                {
                    return _minBrightness;
                }
            }
            return Math.Min(byte.MaxValue, Math.Max(0, calculatedBrightness));
        }
    }
}
