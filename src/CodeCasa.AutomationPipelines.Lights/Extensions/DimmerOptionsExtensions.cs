namespace CodeCasa.AutomationPipelines.Lights.Extensions
{
    internal static class DimmerOptionsExtensions
    {
        public static void ValidateSingleLightEntity(this DimmerOptions dimmerOptions, string lightEntityId)
        {
            var dimOrderLightEntitiesArray = dimmerOptions.DimOrderLightEntities?.ToArray();
            if (dimOrderLightEntitiesArray != null && dimOrderLightEntitiesArray.Any())
            {
                var extraEntities = dimOrderLightEntitiesArray.Where(l => l != lightEntityId).ToArray();
                if (extraEntities.Any())
                {
                    throw new InvalidOperationException(
                        $"Builder only supports entity {lightEntityId}. Please remove extra entities {string.Join(", ", extraEntities)}.");
                }
            }
        }

        public static OrderedDictionary<string, T> ValidateAndOrderMultipleLightEntityTypes<T>(this DimmerOptions dimmerOptions, Dictionary<string, T> typesByLightEntityIds)
        {
            var dimOrderLightEntitiesArray = dimmerOptions.DimOrderLightEntities?.ToArray();
            if (dimOrderLightEntitiesArray != null && dimOrderLightEntitiesArray.Any())
            {
                var missingEntities = typesByLightEntityIds.Keys.Except(dimOrderLightEntitiesArray).ToArray();
                if (missingEntities.Any())
                {
                    throw new InvalidOperationException(
                        $"When providing dim order, all entities should be provided. The following entities are missing: {string.Join(", ", missingEntities)}. Make sure to provide low level entities.");
                }

                var extraEntities = dimOrderLightEntitiesArray.Except(typesByLightEntityIds.Keys).ToArray();
                if (extraEntities.Any())
                {
                    throw new InvalidOperationException(
                        $"Pipeline does not contain the following entities: {string.Join(", ", extraEntities)}. Make sure to provide low level entities.");
                }

                return new OrderedDictionary<string, T>(dimOrderLightEntitiesArray
                    .Select(e => new KeyValuePair<string, T>(e, typesByLightEntityIds[e])));
            }

            return new OrderedDictionary<string, T>(typesByLightEntityIds);
        }
    }
}
