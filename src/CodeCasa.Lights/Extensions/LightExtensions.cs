
namespace CodeCasa.Lights.Extensions;

public static class LightExtensions
{
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