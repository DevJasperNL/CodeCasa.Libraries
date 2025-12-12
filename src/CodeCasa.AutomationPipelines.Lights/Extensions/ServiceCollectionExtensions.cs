using CodeCasa.AutomationPipelines.Lights.Context;
using CodeCasa.AutomationPipelines.Lights.Pipeline;
using CodeCasa.AutomationPipelines.Lights.ReactiveNode;
using Microsoft.Extensions.DependencyInjection;

namespace CodeCasa.AutomationPipelines.Lights.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddLightPipelines(this IServiceCollection serviceCollection)
    {
        return serviceCollection
            .AddTransient(typeof(LightPipelineFactory<>))
            .AddTransient<ReactiveNodeFactory>()
            .AddSingleton(typeof(LightPipelineContextProvider<>))
            .AddTransient(serviceProvider =>
                serviceProvider.GetRequiredService<LightPipelineContextProvider>().GetLightPipelineContext());

        serviceCollection.AddTransient(typeof(ILightPipelineContext<>), serviceProvider =>
        {
            // 1. Get the ServiceDescriptor for the current request
            // We use a custom extension method (shown below) to retrieve the Type 
            // of the requested concrete service (e.g., ILightPipelineContext<StreetLight>)
            var serviceType = serviceProvider.GetRequiredService<TLightServiceTypeAccessor>().ServiceType;

            // 2. Extract the generic argument (TLight, e.g., StreetLight)
            var specificTLight = serviceType.GetGenericArguments()[0];

            // 3. Construct the specific closed Provider type
            // e.g., LightPipelineContextProvider<StreetLight>
            var specificProviderType = typeof(LightPipelineContextProvider<>).MakeGenericType(specificTLight);

            // 4. Resolve the specific Provider singleton instance
            var providerInstance = serviceProvider.GetRequiredService(specificProviderType);

            // 5. Use reflection to call the GetLightPipelineContext() method
            var getContextMethod = specificProviderType.GetMethod("GetLightPipelineContext");

            // 6. Invoke the method
            var context = getContextMethod?.Invoke(providerInstance, null);

            return context ?? throw new InvalidOperationException($"Context not found for {specificTLight.Name}");
        });
    }
}