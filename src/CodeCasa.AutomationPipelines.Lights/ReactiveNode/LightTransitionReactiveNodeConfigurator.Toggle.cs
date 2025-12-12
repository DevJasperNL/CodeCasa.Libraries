using AutomationPipelines;
using CodeCasa.AutomationPipelines.Lights.Context;
using CodeCasa.AutomationPipelines.Lights.Extensions;
using CodeCasa.AutomationPipelines.Lights.Nodes;
using CodeCasa.AutomationPipelines.Lights.Toggle;
using CodeCasa.Lights;
using Microsoft.Extensions.DependencyInjection;
using NetDaemon.Lights;
using NetDaemon.Lights.Extensions;
using NetDaemon.Lights.Scenes;

namespace CodeCasa.AutomationPipelines.Lights.ReactiveNode;

public partial class LightTransitionReactiveNodeConfigurator
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
        var toggleConfigurator = new LightTransitionToggleConfigurator(LightEntity, scheduler);
        configure(toggleConfigurator);
        AddNodeSource(triggerObservable.ToToggleObservable(
            LightEntity.IsOn,
            () => new TurnOffThenPassThroughNode(),
            toggleConfigurator.NodeFactories.Select(fact =>
            {
                return new Func<IPipelineNode<LightTransition>>(() =>
                {
                    var serviceScope = serviceProvider.CreateScope();
                    var context = new LightPipelineContext(serviceScope.ServiceProvider, LightEntity);
                    return new ScopedNode<LightTransition>(serviceScope, fact(context));
                });
            }),
            toggleConfigurator.ToggleTimeout ?? TimeSpan.FromMilliseconds(1000),
            toggleConfigurator.IncludeOffValue));
        return this;
    }
}