using CodeCasa.Lights;
using CodeCasa.Lights.Extensions;

namespace CodeCasa.AutomationPipelines.Lights
{
    internal static class CompositeHelper
    {
        // todo: check and filter without errors.
        public static string[] ResolveAndValidateLightEntities(IEnumerable<ILight> lightEntityIds, IEnumerable<string> supportedLightIds)
        {
            var supportedLightIdsArray = supportedLightIds.ToArray();
            var lightLeafs = lightEntityIds.SelectMany(le => le.Flatten()).DistinctBy(l => l.Id).ToArray();
            if (!lightLeafs.Any())
            {
                throw new ArgumentException("At least one entity id should be provided.", nameof(lightLeafs));
            }

            var missingLights = lightLeafs
                .Where(id => !supportedLightIdsArray.Contains(id.Id))
                .ToArray();

            if (missingLights.Any())
            {
                throw new InvalidOperationException(
                    $"The following light entities are not supported: {string.Join(", ", missingLights)}.");
            }

            return lightLeafs.Select(l => l.Id).ToArray();
        }

        public static void ValidateLightEntities(IEnumerable<ILight> lights, string supportedLightId)
        {
            var lightLeafs = lights.SelectMany(le => le.Flatten()).DistinctBy(l => l.Id).ToArray();
            if (!lightLeafs.Any())
            {
                throw new ArgumentException("At least one entity id should be provided.", nameof(lightLeafs));
            }

            var missingLights = lightLeafs
                .Where(l => l.Id != supportedLightId)
                .ToArray();

            if (missingLights.Any())
            {
                throw new InvalidOperationException(
                    $"The following light entities are not supported: {string.Join(", ", missingLights)}.");
            }
        }

        public static void ValidateLightEntities(IEnumerable<string> lights, string supportedLightId)
        {
            var lightsArray = lights.ToArray();
            if (!lightsArray.Any())
            {
                throw new ArgumentException("At least one entity id should be provided.", nameof(lightsArray));
            }

            var missingLights = lightsArray
                .Where(id => id != supportedLightId)
                .ToArray();

            if (missingLights.Any())
            {
                throw new InvalidOperationException(
                    $"The following light entities are not supported: {string.Join(", ", missingLights)}.");
            }
        }
    }
}
