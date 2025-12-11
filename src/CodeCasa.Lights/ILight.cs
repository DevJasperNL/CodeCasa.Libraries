namespace CodeCasa.Lights;

/// <summary>
/// Represents a single light or group of lights.
/// </summary>
public interface ILight
{
    LightParameters Parameters { get; }
    void ApplyTransition(LightTransition transition);
    ILight[] GetChildren();
}