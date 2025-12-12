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
            .AddTransient<LightPipelineFactory>()
            .AddTransient<ReactiveNodeFactory>()
            .AddSingleton<LightPipelineContextProvider>()
            .AddTransient(serviceProvider =>
                serviceProvider.GetRequiredService<LightPipelineContextProvider>().GetLightPipelineContext());
    }
}