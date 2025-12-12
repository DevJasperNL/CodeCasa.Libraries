using AutomationPipelines;
using CodeCasa.AutomationPipelines.Lights.Context;
using CodeCasa.AutomationPipelines.Lights.Cycle;
using CodeCasa.AutomationPipelines.Lights.Extensions;
using CodeCasa.AutomationPipelines.Lights.Nodes;
using Microsoft.Extensions.DependencyInjection;


using NetDaemon.Lights;
using NetDaemon.Lights.Scenes;

namespace CodeCasa.AutomationPipelines.Lights.ReactiveNode;

public partial class LightTransitionReactiveNodeConfigurator
{
    public ILightTransitionReactiveNodeConfigurator<TLight> AddCycle<T>(IObservable<T> triggerObservable,
            IEnumerable<LightSceneTemplate> scenes)
            => AddCycle(triggerObservable, scenes.ToArray());

    public ILightTransitionReactiveNodeConfigurator<TLight> AddCycle<T>(IObservable<T> triggerObservable,
        params LightSceneTemplate[] scenes)
    {
        return AddCycle(triggerObservable, configure =>
        {
            foreach (var lightScene in scenes)
            {
                configure.Add(lightScene);
            }
        });
    }

    public ILightTransitionReactiveNodeConfigurator<TLight> AddCycle<T>(IObservable<T> triggerObservable, IEnumerable<LightParameters> lightParameters)
        => AddCycle(triggerObservable, lightParameters.ToArray());

    public ILightTransitionReactiveNodeConfigurator<TLight> AddCycle<T>(IObservable<T> triggerObservable,
        params LightParameters[] lightParameters)
    {
        return AddCycle(triggerObservable, configure =>
        {
            foreach (var lp in lightParameters)
            {
                configure.Add(lp);
            }
        });
    }

    public ILightTransitionReactiveNodeConfigurator<TLight> AddCycle<T>(IObservable<T> triggerObservable, IEnumerable<LightTransition> lightTransitions)
        => AddCycle(triggerObservable, lightTransitions.ToArray());

    public ILightTransitionReactiveNodeConfigurator<TLight> AddCycle<T>(IObservable<T> triggerObservable,
        params LightTransition[] lightTransitions)
    {
        return AddCycle(triggerObservable, configure =>
        {
            foreach (var lt in lightTransitions)
            {
                configure.Add(lt);
            }
        });
    }

    public ILightTransitionReactiveNodeConfigurator<TLight> AddCycle<T>(IObservable<T> triggerObservable,
        Action<ILightTransitionCycleConfigurator> configure)
    {
        var cycleConfigurator = new LightTransitionCycleConfigurator(LightEntity, scheduler);
        configure(cycleConfigurator);
        AddNodeSource(triggerObservable.ToCycleObservable(cycleConfigurator.CycleNodeFactories.Select(tuple =>
        {
            var serviceScope = serviceProvider.CreateScope();
            var context = new LightPipelineContext(serviceScope.ServiceProvider, LightEntity);
            var factory = new Func<IPipelineNode<LightTransition>>(() => new ScopedNode<LightTransition>(serviceScope, tuple.nodeFactory(context)));
            var valueIsActiveFunc = () => tuple.matchesNodeState(context);
            return (factory, valueIsActiveFunc);
        })));
        return this;
    }
}