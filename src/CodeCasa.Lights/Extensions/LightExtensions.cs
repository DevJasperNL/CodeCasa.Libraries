namespace CodeCasa.Lights.Extensions;

/// <summary>
/// Provides extension methods for <see cref="ILight"/> instances.
/// </summary>
public static class LightExtensions
{
    public static bool IsOn(this ILight light)
    {
        return (light.GetParameters().Brightness ?? 0) != 0;
    }
    public static bool IsOff(this ILight light)
    {
        return (light.GetParameters().Brightness ?? 0) == 0;
    }

    /// <summary>
    /// Flattens a light hierarchy into a single-dimensional array of leaf lights.
    /// </summary>
    /// <remarks>
    /// This method recursively traverses the light hierarchy and collects all leaf lights
    /// (lights without children). Circular references are handled by tracking visited lights.
    /// </remarks>
    /// <param name="light">The light to flatten.</param>
    /// <returns>An array of leaf lights from the light hierarchy.</returns>
    public static ILight[] Flatten(this ILight light)
    {
        var visitedLights = new HashSet<ILight>();
        var result = new List<ILight>();
        FlattenRecursive(light, visitedLights, result);
        return result.ToArray();
    }

    private static void FlattenRecursive(ILight light, HashSet<ILight> visitedLights, List<ILight> result)
    {
        if (!visitedLights.Add(light))
        {
            return;
        }

        var children = light.GetChildren();
        if (!children.Any())
        {
            result.Add(light);
            return;
        }
        foreach (var child in children)
        {
            FlattenRecursive(child, visitedLights, result);
        }
    }
}