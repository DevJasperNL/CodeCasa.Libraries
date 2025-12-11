using CodeCasa.Lights.Extensions;

namespace CodeCasa.Lights;

/// <summary>
/// Represents a light transition with specific light parameters and an optional transition duration.
/// </summary>
public record LightTransition
{
    /// <summary>
    /// Gets the light parameters that describe the target state of the light.
    /// </summary>
    public LightParameters LightParameters { get; init; } = null!;

    /// <summary>
    /// Gets the duration over which the light should transition to the target parameters.
    /// If null, the transition is immediate.
    /// </summary>
    public TimeSpan? TransitionTime { get; init; }

    /// <summary>
    /// Creates a <see cref="LightTransition"/> with an off state and no transition time.
    /// </summary>
    /// <returns>A <see cref="LightTransition"/> object representing an immediate off transition.</returns>
    public static LightTransition Off()
    {
        return LightParameters.Off().AsTransition();
    }

    /// <summary>
    /// Creates a <see cref="LightTransition"/> with an on state and no transition time.
    /// This is intended for binary lights that only support on/off states.
    /// </summary>
    /// <returns>A <see cref="LightTransition"/> object representing an immediate on transition.</returns>
    public static LightTransition On()
    {
        return LightParameters.On().AsTransition();
    }

    /// <inheritdoc/>
    public override string ToString()
    {
        return TransitionTime == null ? LightParameters.ToString() : $"{LightParameters} in {TransitionTime}";
    }
}