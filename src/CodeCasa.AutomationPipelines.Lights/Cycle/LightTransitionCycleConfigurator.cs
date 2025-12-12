using System.Reactive.Concurrency;
using AutomationPipelines;
using CodeCasa.AutomationPipelines.Lights.Context;
using CodeCasa.AutomationPipelines.Lights.Extensions;
using CodeCasa.AutomationPipelines.Lights.Nodes;
using CodeCasa.Lights;
using CodeCasa.Lights.Extensions;

namespace CodeCasa.AutomationPipelines.Lights.Cycle;

internal class LightTransitionCycleConfigurator<TLight>(TLight lightEntity, IScheduler scheduler) : ILightTransitionCycleConfigurator<TLight> where TLight : ILight
{
    public TLight LightEntity { get; } = lightEntity;

    internal List<(Func<ILightPipelineContext<TLight>, IPipelineNode<LightTransition>> nodeFactory, Func<ILightPipelineContext<TLight>, bool> matchesNodeState)> CycleNodeFactories
    {
        get;
    } = [];

    public ILightTransitionCycleConfigurator<TLight> AddOff()
    {
        return Add<TurnOffThenPassThroughNode>(_ => LightEntity.IsOff());
    }

    public ILightTransitionCycleConfigurator<TLight> AddOn()
    {
        return Add(LightTransition.On());
    }

    public ILightTransitionCycleConfigurator<TLight> Add(LightParameters lightParameters)
    {
        return Add(lightParameters.AsTransition());
    }

    public ILightTransitionCycleConfigurator<TLight> Add(Func<ILightPipelineContext<TLight>, LightParameters?> lightParametersFactory, Func<ILightPipelineContext<TLight>, bool> matchesNodeState)
    {
        return Add(c => lightParametersFactory(c)?.AsTransition(), matchesNodeState);
    }

    public ILightTransitionCycleConfigurator<TLight> Add(Func<ILightPipelineContext<TLight>, LightTransition?, LightParameters?> lightParametersFactory, Func<ILightPipelineContext<TLight>, bool> matchesNodeState)
    {
        return Add((c, t) => lightParametersFactory(c, t)?.AsTransition(), matchesNodeState);
    }

    public ILightTransitionCycleConfigurator<TLight> Add(LightTransition lightTransition)
    {
        return Add(new StaticLightTransitionNode(lightTransition, scheduler), _ => LightEntity.SceneEquals(lightTransition.LightParameters));
    }

    public ILightTransitionCycleConfigurator<TLight> Add(Func<ILightPipelineContext<TLight>, LightTransition?> lightTransitionFactory, Func<ILightPipelineContext<TLight>, bool> matchesNodeState)
    {
        return Add(c => new StaticLightTransitionNode(lightTransitionFactory(c), scheduler), matchesNodeState);
    }

    public ILightTransitionCycleConfigurator<TLight> Add(Func<ILightPipelineContext<TLight>, LightTransition?, LightTransition?> lightTransitionFactory, Func<ILightPipelineContext<TLight>, bool> matchesNodeState)
    {
        return Add(c => new FactoryNode<LightTransition>(t => lightTransitionFactory(c, t)), matchesNodeState);
    }

    public ILightTransitionCycleConfigurator<TLight> Add<TNode>(Func<ILightPipelineContext<TLight>, bool> matchesNodeState) where TNode : IPipelineNode<LightTransition>
    {
        return Add(c => c.ServiceProvider.CreateInstanceWithinContext<TNode, TLight>(c), matchesNodeState);
    }

    public ILightTransitionCycleConfigurator<TLight> Add(IPipelineNode<LightTransition> node, Func<ILightPipelineContext<TLight>, bool> matchesNodeState)
    {
        return Add(_ => node, matchesNodeState);
    }

    public ILightTransitionCycleConfigurator<TLight> Add(Func<ILightPipelineContext<TLight>, IPipelineNode<LightTransition>> nodeFactory, Func<ILightPipelineContext<TLight>, bool> matchesNodeState)
    {
        CycleNodeFactories.Add((nodeFactory, matchesNodeState));
        return this;
    }

    public ILightTransitionCycleConfigurator<TLight> AddPassThrough(Func<ILightPipelineContext<TLight>, bool> matchesNodeState)
    {
        return Add(new PassThroughNode<LightTransition>(), matchesNodeState);
    }

    public ILightTransitionCycleConfigurator<TLight> ForLight(string lightEntityId, Action<ILightTransitionCycleConfigurator<TLight>> configure, ExcludedLightBehaviours excludedLightBehaviour = ExcludedLightBehaviours.None) => ForLights([lightEntityId], configure, excludedLightBehaviour);

    public ILightTransitionCycleConfigurator<TLight> ForLight(TLight lightEntity, Action<ILightTransitionCycleConfigurator<TLight>> configure, ExcludedLightBehaviours excludedLightBehaviour = ExcludedLightBehaviours.None) => ForLights([lightEntity], configure, excludedLightBehaviour);

    public ILightTransitionCycleConfigurator<TLight> ForLights(IEnumerable<string> lightEntityIds, Action<ILightTransitionCycleConfigurator<TLight>> configure, ExcludedLightBehaviours excludedLightBehaviour = ExcludedLightBehaviours.None)
    {
        CompositeHelper.ValidateLightEntities(lightEntityIds, LightEntity.Id);
        return this;
    }

    public ILightTransitionCycleConfigurator<TLight> ForLights(IEnumerable<TLight> lightEntities, Action<ILightTransitionCycleConfigurator<TLight>> configure, ExcludedLightBehaviours excludedLightBehaviour = ExcludedLightBehaviours.None)
    {
        CompositeHelper.ValidateLightEntities(lightEntities, LightEntity.Id);
        return this;
    }
}