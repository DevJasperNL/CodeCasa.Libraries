using CodeCasa.Lights;

namespace CodeCasa.AutomationPipelines.Lights.Context;

public interface ILightPipelineContext<out TLight> where TLight : ILight
{
    IServiceProvider ServiceProvider { get; }
    TLight LightEntity { get; }
}