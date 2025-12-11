using Microsoft.Extensions.DependencyInjection;

namespace AutomationPipelines.Extensions;

/// <summary>
/// Extension methods for <see cref="IServiceCollection"/>.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Adds the automation pipelines to the service collection.
    /// </summary>
    public static IServiceCollection AddAutomationPipelines(this IServiceCollection serviceCollection)
    {
        return serviceCollection
            .AddTransient(typeof(IPipeline<>), typeof(ServiceProviderPipeline<>))
            .AddTransient(typeof(Pipeline<>), typeof(ServiceProviderPipeline<>))
            .AddTransient(typeof(ServiceProviderPipeline<>), typeof(ServiceProviderPipeline<>));
    }
}