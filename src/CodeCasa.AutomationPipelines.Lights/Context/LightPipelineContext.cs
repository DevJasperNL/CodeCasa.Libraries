using CodeCasa.Lights;

namespace CodeCasa.AutomationPipelines.Lights.Context;

public class LightPipelineContext : ILightPipelineContext
{
    internal LightPipelineContext(IServiceProvider serviceProvider, ILight lightEntity)
    {
        ServiceProvider = serviceProvider;
        LightEntity = lightEntity;
    }

    public IServiceProvider ServiceProvider { get; }
    public ILight LightEntity { get; }
}