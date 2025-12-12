using CodeCasa.Lights;

namespace CodeCasa.AutomationPipelines.Lights.Context
{
    internal class LightPipelineContextProvider<TLight> where TLight : ILight
    {
        private ILightPipelineContext<TLight>? _lightPipelineContext;

        public ILightPipelineContext<TLight> GetLightPipelineContext()
        {
            return _lightPipelineContext ?? throw new InvalidOperationException("Current context not set.");
        }

        public void SetLightPipelineContext(ILightPipelineContext<TLight> context)
        {
            _lightPipelineContext = context;
        }

        public void ResetLightEntity()
        {
            _lightPipelineContext = null;
        }
    }
}
