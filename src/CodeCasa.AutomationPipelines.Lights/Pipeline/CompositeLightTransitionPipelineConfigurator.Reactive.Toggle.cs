using AutomationPipelines;
using CodeCasa.AutomationPipelines.Lights.Context;
using CodeCasa.AutomationPipelines.Lights.Toggle;


using NetDaemon.Lights;
using NetDaemon.Lights.Scenes;

namespace CodeCasa.AutomationPipelines.Lights.Pipeline;

public partial class CompositeLightTransitionPipelineConfigurator
{
    public ILightTransitionPipelineConfigurator AddToggle<T>(IObservable<T> triggerObservable,
        IEnumerable<LightSceneTemplate> scenes)
    {
        return AddReactiveNode(c => c.AddToggle(triggerObservable, scenes));
    }

    public ILightTransitionPipelineConfigurator AddToggle<T>(IObservable<T> triggerObservable,
        params LightSceneTemplate[] scenes)
    {
        return AddReactiveNode(c => c.AddToggle(triggerObservable, scenes));
    }

    public ILightTransitionPipelineConfigurator AddToggle<T>(IObservable<T> triggerObservable,
        IEnumerable<LightParameters> lightParameters)
    {
        return AddReactiveNode(c => c.AddToggle(triggerObservable, lightParameters));
    }

    public ILightTransitionPipelineConfigurator AddToggle<T>(IObservable<T> triggerObservable,
        params LightParameters[] lightParameters)
    {
        return AddReactiveNode(c => c.AddToggle(triggerObservable, lightParameters));
    }

    public ILightTransitionPipelineConfigurator AddToggle<T>(IObservable<T> triggerObservable,
        IEnumerable<LightTransition> lightTransitions)
    {
        return AddReactiveNode(c => c.AddToggle(triggerObservable, lightTransitions));
    }

    public ILightTransitionPipelineConfigurator AddToggle<T>(IObservable<T> triggerObservable,
        params LightTransition[] lightTransitions)
    {
        return AddReactiveNode(c => c.AddToggle(triggerObservable, lightTransitions));
    }

    public ILightTransitionPipelineConfigurator AddToggle<T>(IObservable<T> triggerObservable,
        IEnumerable<Func<ILightPipelineContext, IPipelineNode<LightTransition>>> nodeFactories)
    {
        return AddReactiveNode(c => c.AddToggle(triggerObservable, nodeFactories));
    }

    public ILightTransitionPipelineConfigurator AddToggle<T>(IObservable<T> triggerObservable,
        params Func<ILightPipelineContext, IPipelineNode<LightTransition>>[] nodeFactories)
    {
        return AddReactiveNode(c => c.AddToggle(triggerObservable, nodeFactories));
    }

    public ILightTransitionPipelineConfigurator AddToggle<T>(IObservable<T> triggerObservable,
        Action<ILightTransitionToggleConfigurator> configure)
    {
        return AddReactiveNode(c => c.AddToggle(triggerObservable, configure));
    }
}