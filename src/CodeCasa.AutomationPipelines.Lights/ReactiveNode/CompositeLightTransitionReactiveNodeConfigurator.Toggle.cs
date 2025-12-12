using AutomationPipelines;
using CodeCasa.AutomationPipelines.Lights.Context;
using CodeCasa.AutomationPipelines.Lights.Extensions;
using CodeCasa.AutomationPipelines.Lights.Nodes;
using CodeCasa.AutomationPipelines.Lights.Toggle;
using Microsoft.Extensions.DependencyInjection;
using NetDaemon.Lights;
using NetDaemon.Lights.Extensions;
using NetDaemon.Lights.Scenes;

namespace CodeCasa.AutomationPipelines.Lights.ReactiveNode;

public partial class CompositeLightTransitionReactiveNodeConfigurator
{
    public ILightTransitionReactiveNodeConfigurator<TLight> AddToggle<T>(IObservable<T> triggerObservable, IEnumerable<LightSceneTemplate> scenes)
        => AddToggle(triggerObservable, scenes.ToArray());

    public ILightTransitionReactiveNodeConfigurator<TLight> AddToggle<T>(IObservable<T> triggerObservable, params LightSceneTemplate[] scenes)
    {
        return AddToggle(triggerObservable, configure =>
        {
            foreach (var lightScene in scenes)
            {
                configure.Add(lightScene);
            }
        });
    }

    public ILightTransitionReactiveNodeConfigurator<TLight> AddToggle<T>(IObservable<T> triggerObservable, IEnumerable<LightParameters> lightParameters)
        => AddToggle(triggerObservable, lightParameters.ToArray());

    public ILightTransitionReactiveNodeConfigurator<TLight> AddToggle<T>(IObservable<T> triggerObservable,
        params LightParameters[] lightParameters)
    {
        return AddToggle(triggerObservable, configure =>
        {
            foreach (var lightParameter in lightParameters)
            {
                configure.Add(lightParameter);
            }
        });
    }

    public ILightTransitionReactiveNodeConfigurator<TLight> AddToggle<T>(IObservable<T> triggerObservable, IEnumerable<LightTransition> lightTransitions)
        => AddToggle(triggerObservable, lightTransitions.ToArray());

    public ILightTransitionReactiveNodeConfigurator<TLight> AddToggle<T>(IObservable<T> triggerObservable,
        params LightTransition[] lightTransitions)
    {
        return AddToggle(triggerObservable, configure =>
        {
            foreach (var lightTransition in lightTransitions)
            {
                configure.Add(lightTransition);
            }
        });
    }

    public ILightTransitionReactiveNodeConfigurator<TLight> AddToggle<T>(IObservable<T> triggerObservable, IEnumerable<Func<ILightPipelineContext, IPipelineNode<LightTransition>>> nodeFactories)
        => AddToggle(triggerObservable, nodeFactories.ToArray());

    public ILightTransitionReactiveNodeConfigurator<TLight> AddToggle<T>(IObservable<T> triggerObservable, params Func<ILightPipelineContext, IPipelineNode<LightTransition>>[] nodeFactories)
    {
        return AddToggle(triggerObservable, configure =>
        {
            foreach (var fact in nodeFactories)
            {
                configure.Add(fact);
            }
        });
    }

    public ILightTransitionReactiveNodeConfigurator<TLight> AddToggle<T>(IObservable<T> triggerObservable, Action<ILightTransitionToggleConfigurator> configure)
    {
        var toggleConfigurators = configurators.ToDictionary(kvp => kvp.Key,
            kvp => new LightTransitionToggleConfigurator(kvp.Value.LightEntity, scheduler));
        var compositeCycleConfigurator = new CompositeLightTransitionToggleConfigurator(haContext, toggleConfigurators, []);
        configure(compositeCycleConfigurator);
        configurators.ForEach(kvp => kvp.Value.AddNodeSource(triggerObservable.ToToggleObservable(
            () => configurators.Values.Any(c => c.LightEntity.IsOn()),
            () => new TurnOffThenPassThroughNode(),
            toggleConfigurators[kvp.Key].NodeFactories.Select(fact =>
            {
                return new Func<IPipelineNode<LightTransition>>(() =>
                {
                    var serviceScope = serviceProvider.CreateScope();
                    var context = new LightPipelineContext(serviceScope.ServiceProvider, kvp.Value.LightEntity);
                    return new ScopedNode<LightTransition>(serviceScope, fact(context));
                });
            }),
            toggleConfigurators[kvp.Key].ToggleTimeout ?? TimeSpan.FromMilliseconds(1000),
            toggleConfigurators[kvp.Key].IncludeOffValue)));
        return this;
    }
}