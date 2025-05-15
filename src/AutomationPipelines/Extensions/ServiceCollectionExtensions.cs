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
            .AddTransient(typeof(IPipeline<>), typeof(Pipeline<>))
            .AddTransient(typeof(Pipeline<>), typeof(Pipeline<>));
    }
}