using CodeCasa.Lights;

namespace CodeCasa.AutomationPipelines.Lights.Context;

public interface ILightPipelineContext
{
    IServiceProvider ServiceProvider { get; }
    ILight LightEntity { get; }
}