using CodeCasa.AutomationPipelines.Lights.Context;
using CodeCasa.Lights;
using Microsoft.Extensions.DependencyInjection;

namespace CodeCasa.AutomationPipelines.Lights.Extensions;

internal static class ServiceProviderExtensions
{
    public static TInstance
        CreateInstanceWithinContext<TInstance, TLight>(this IServiceProvider serviceProvider, TLight lightEntity) where TLight : ILight =>
        serviceProvider.CreateInstanceWithinContext<TInstance, TLight>(new LightPipelineContext<TLight>(serviceProvider, lightEntity));

    public static TInstance
        CreateInstanceWithinContext<TInstance, TLight>(this IServiceProvider serviceProvider, ILightPipelineContext<TLight> context) where TLight : ILight =>
        (TInstance)serviceProvider.CreateInstanceWithinContext(typeof(TInstance), context);

    public static object CreateInstanceWithinContext<TLight>(this IServiceProvider serviceProvider, Type instanceType,
        ILightPipelineContext<TLight> context) where TLight : ILight
    {
        var contextProvider = serviceProvider.GetRequiredService<LightPipelineContextProvider<TLight>>();
        contextProvider.SetLightPipelineContext(context);
        var instance = ActivatorUtilities.CreateInstance(serviceProvider, instanceType);
        contextProvider.ResetLightEntity();
        return instance;
    }
}