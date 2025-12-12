using CodeCasa.AutomationPipelines.Lights.Cycle;


using NetDaemon.Lights;
using NetDaemon.Lights.Scenes;

namespace CodeCasa.AutomationPipelines.Lights.Pipeline;

public partial class LightTransitionPipelineConfigurator
{
    public ILightTransitionPipelineConfigurator AddCycle<T>(IObservable<T> triggerObservable, IEnumerable<LightSceneTemplate> scenes)
    {
        return AddReactiveNode(c => c.AddCycle(triggerObservable, scenes));
    }

    public ILightTransitionPipelineConfigurator AddCycle<T>(IObservable<T> triggerObservable, params LightSceneTemplate[] scenes)
    {
        return AddReactiveNode(c => c.AddCycle(triggerObservable, scenes));
    }

    public ILightTransitionPipelineConfigurator AddCycle<T>(IObservable<T> triggerObservable, IEnumerable<LightParameters> lightParameters)
    {
        return AddReactiveNode(c => c.AddCycle(triggerObservable, lightParameters));
    }

    public ILightTransitionPipelineConfigurator AddCycle<T>(IObservable<T> triggerObservable,
        params LightParameters[] lightParameters)
    {
        return AddReactiveNode(c => c.AddCycle(triggerObservable, lightParameters));
    }

    public ILightTransitionPipelineConfigurator AddCycle<T>(IObservable<T> triggerObservable, IEnumerable<LightTransition> lightTransitions)
    {
        return AddReactiveNode(c => c.AddCycle(triggerObservable, lightTransitions));
    }

    public ILightTransitionPipelineConfigurator AddCycle<T>(IObservable<T> triggerObservable,
        params LightTransition[] lightTransitions)
    {
        return AddReactiveNode(c => c.AddCycle(triggerObservable, lightTransitions));
    }

    public ILightTransitionPipelineConfigurator AddCycle<T>(IObservable<T> triggerObservable, Action<ILightTransitionCycleConfigurator> configure)
    {
        return AddReactiveNode(c => c.AddCycle(triggerObservable, configure));
    }
}