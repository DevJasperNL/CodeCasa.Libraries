using CodeCasa.Lights;

namespace CodeCasa.AutomationPipelines.Lights.Context;

public class LightPipelineContext<TLight> : ILightPipelineContext<TLight> where TLight : ILight
{
    internal LightPipelineContext(IServiceProvider serviceProvider, TLight lightEntity)
    {
        ServiceProvider = serviceProvider;
        LightEntity = lightEntity;
    }

    public IServiceProvider ServiceProvider { get; }
    public TLight LightEntity { get; }
}