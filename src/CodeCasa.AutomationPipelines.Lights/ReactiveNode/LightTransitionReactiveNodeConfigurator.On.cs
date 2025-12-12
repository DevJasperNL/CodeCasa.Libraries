using AutomationPipelines;
using CodeCasa.AutomationPipelines.Lights.Context;
using CodeCasa.AutomationPipelines.Lights.Extensions;
using CodeCasa.AutomationPipelines.Lights.Nodes;
using CodeCasa.AutomationPipelines.Lights.Pipeline;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using CodeCasa.Lights;
using Microsoft.Extensions.DependencyInjection;

namespace CodeCasa.AutomationPipelines.Lights.ReactiveNode;

public partial class LightTransitionReactiveNodeConfigurator
{
    public ILightTransitionReactiveNodeConfigurator On<T>(IObservable<T> triggerObservable,
        LightParameters lightParameters) => On(triggerObservable, lightParameters.AsTransition());

    public ILightTransitionReactiveNodeConfigurator On<T>(IObservable<T> triggerObservable,
        Func<ILightPipelineContext, LightParameters> lightParametersFactory)
        => On(triggerObservable, c => lightParametersFactory(c).AsTransition());

    public ILightTransitionReactiveNodeConfigurator On<T>(IObservable<T> triggerObservable,
        LightTransition lightTransition) =>
        On(triggerObservable, c => new StaticLightTransitionNode(lightTransition, c.ServiceProvider.GetRequiredService<IScheduler>()));

    public ILightTransitionReactiveNodeConfigurator On<T>(IObservable<T> triggerObservable,
        Func<ILightPipelineContext, LightTransition> lightTransitionFactory)
        => On(triggerObservable, c => new StaticLightTransitionNode(lightTransitionFactory(c), c.ServiceProvider.GetRequiredService<IScheduler>()));

    public ILightTransitionReactiveNodeConfigurator On<T, TNode>(IObservable<T> triggerObservable)
        where TNode : IPipelineNode<LightTransition> =>
        AddNodeSource(triggerObservable.Select(_ =>
            new Func<ILightPipelineContext, IPipelineNode<LightTransition>?>(c =>
                c.ServiceProvider.CreateInstanceWithinContext<TNode>(c))));

    public ILightTransitionReactiveNodeConfigurator On<T>(IObservable<T> triggerObservable,
        Func<ILightPipelineContext, IPipelineNode<LightTransition>> nodeFactory) =>
        AddNodeSource(triggerObservable.Select(_ => nodeFactory));

    public ILightTransitionReactiveNodeConfigurator On<T>(IObservable<T> triggerObservable, Action<ILightTransitionPipelineConfigurator> pipelineConfigurator) => On(triggerObservable, c => lightPipelineFactory.CreateLightPipeline(c.LightEntity, pipelineConfigurator));

    public ILightTransitionReactiveNodeConfigurator On<T>(IObservable<T> triggerObservable, Action<ILightTransitionReactiveNodeConfigurator> configure) => On(triggerObservable, c => reactiveNodeFactory.CreateReactiveNode(c.LightEntity, configure));

    public ILightTransitionReactiveNodeConfigurator PassThroughOn<T>(IObservable<T> triggerObservable)
    {
        AddNodeSource(triggerObservable.Select(_ => new PassThroughNode<LightTransition>()));
        return this;
    }

    public ILightTransitionReactiveNodeConfigurator TurnOffWhen<T>(IObservable<T> triggerObservable)
    {
        AddNodeSource(triggerObservable.Select(_ => new TurnOffThenPassThroughNode()));
        return this;
    }

    public ILightTransitionReactiveNodeConfigurator TurnOnWhen<T>(IObservable<T> triggerObservable)
    {
        return On(triggerObservable, LightTransition.On());
    }
}