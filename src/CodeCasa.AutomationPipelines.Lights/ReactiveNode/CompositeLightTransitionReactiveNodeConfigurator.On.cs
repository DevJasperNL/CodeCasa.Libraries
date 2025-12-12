using System.Reactive.Concurrency;
using AutomationPipelines;
using CodeCasa.AutomationPipelines.Lights.Context;
using CodeCasa.AutomationPipelines.Lights.Extensions;
using CodeCasa.AutomationPipelines.Lights.Nodes;
using CodeCasa.AutomationPipelines.Lights.Pipeline;
using Microsoft.Extensions.DependencyInjection;
using NetDaemon.Lights;
using NetDaemon.Lights.Extensions;
using NetDaemon.Lights.Scenes;

namespace CodeCasa.AutomationPipelines.Lights.ReactiveNode;

public partial class CompositeLightTransitionReactiveNodeConfigurator
{
    public ILightTransitionReactiveNodeConfigurator<TLight> On<T>(IObservable<T> triggerObservable, LightSceneTemplate sceneTemplate)
    {
        configurators.Values.ForEach(b => b.On(triggerObservable, sceneTemplate));
        return this;
    }

    public ILightTransitionReactiveNodeConfigurator<TLight> On<T>(IObservable<T> triggerObservable, Func<LightSceneTemplate> sceneTemplateFactory)
    {
        configurators.Values.ForEach(b => b.On(triggerObservable, sceneTemplateFactory));
        return this;
    }

    public ILightTransitionReactiveNodeConfigurator<TLight> On<T>(IObservable<T> triggerObservable, LightParameters lightParameters)
        => On(triggerObservable, lightParameters.AsTransition());

    public ILightTransitionReactiveNodeConfigurator<TLight> On<T>(IObservable<T> triggerObservable, Func<ILightPipelineContext, LightParameters> lightParametersFactory)
        => On(triggerObservable, c => lightParametersFactory(c).AsTransition());

    public ILightTransitionReactiveNodeConfigurator<TLight> On<T>(IObservable<T> triggerObservable, LightTransition lightTransition)
        => On(triggerObservable, c => new StaticLightTransitionNode(lightTransition, c.ServiceProvider.GetRequiredService<IScheduler>()));

    public ILightTransitionReactiveNodeConfigurator<TLight> On<T>(IObservable<T> triggerObservable, Func<ILightPipelineContext, LightTransition> lightTransitionFactory)
        => On(triggerObservable, c => new StaticLightTransitionNode(lightTransitionFactory(c), c.ServiceProvider.GetRequiredService<IScheduler>()));

    public ILightTransitionReactiveNodeConfigurator<TLight> On<T, TNode>(IObservable<T> triggerObservable) where TNode : IPipelineNode<LightTransition>
    {
        configurators.Values.ForEach(c => c.On<T, TNode>(triggerObservable));
        return this;
    }

    public ILightTransitionReactiveNodeConfigurator<TLight> On<T>(IObservable<T> triggerObservable, Func<ILightPipelineContext, IPipelineNode<LightTransition>> nodeFactory)
    {
        configurators.Values.ForEach(c => c.On(triggerObservable, nodeFactory));
        return this;
    }

    public ILightTransitionReactiveNodeConfigurator<TLight> On<T>(IObservable<T> triggerObservable, Action<ILightTransitionPipelineConfigurator> pipelineConfigurator)
    {
        // Note: we create the pipeline in composite context so all configuration is also applied in that context.
        var pipelines = lightPipelineFactory.CreateLightPipelines(configurators.Values.Select(c => c.LightEntity),
            pipelineConfigurator);
        configurators.Values.ForEach(c => c.On(triggerObservable, ctx => pipelines[ctx.LightEntity.EntityId]));
        return this;
    }

    public ILightTransitionReactiveNodeConfigurator<TLight> On<T>(IObservable<T> triggerObservable, Action<ILightTransitionReactiveNodeConfigurator> configure)
    {
        // Note: we create the pipeline in composite context so all configuration is also applied in that context.
        var nodes = reactiveNodeFactory.CreateReactiveNodes(configurators.Values.Select(c => c.LightEntity),
            configure);
        configurators.Values.ForEach(c => c.On(triggerObservable, ctx => nodes[ctx.LightEntity.EntityId]));
        return this;
    }

    public ILightTransitionReactiveNodeConfigurator<TLight> PassThroughOn<T>(IObservable<T> triggerObservable)
    {
        configurators.Values.ForEach(c => c.PassThroughOn(triggerObservable));
        return this;
    }

    public ILightTransitionReactiveNodeConfigurator<TLight> TurnOffWhen<T>(IObservable<T> triggerObservable)
    {
        configurators.Values.ForEach(c => c.TurnOffWhen(triggerObservable));
        return this;
    }

    public ILightTransitionReactiveNodeConfigurator<TLight> TurnOnWhen<T>(IObservable<T> triggerObservable)
    {
        return On(triggerObservable, LightTransition.On());
    }
}