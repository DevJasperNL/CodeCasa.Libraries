using System.Reactive.Concurrency;
using AutomationPipelines;
using CodeCasa.AutomationPipelines.Lights.Context;
using CodeCasa.AutomationPipelines.Lights.Extensions;
using CodeCasa.AutomationPipelines.Lights.Nodes;
using CodeCasa.Lights;
using CodeCasa.Lights.Extensions;

namespace CodeCasa.AutomationPipelines.Lights.Cycle;

internal class LightTransitionCycleConfigurator(ILight lightEntity, IScheduler scheduler) : ILightTransitionCycleConfigurator
{
    public ILight LightEntity { get; } = lightEntity;

    internal List<(Func<ILightPipelineContext, IPipelineNode<LightTransition>> nodeFactory, Func<ILightPipelineContext, bool> matchesNodeState)> CycleNodeFactories
    {
        get;
    } = [];

    public ILightTransitionCycleConfigurator AddOff()
    {
        return Add<TurnOffThenPassThroughNode>(_ => LightEntity.IsOff());
    }

    public ILightTransitionCycleConfigurator AddOn()
    {
        return Add(LightTransition.On());
    }

    public ILightTransitionCycleConfigurator Add(LightParameters lightParameters)
    {
        return Add(lightParameters.AsTransition());
    }

    public ILightTransitionCycleConfigurator Add(Func<ILightPipelineContext, LightParameters?> lightParametersFactory, Func<ILightPipelineContext, bool> matchesNodeState)
    {
        return Add(c => lightParametersFactory(c)?.AsTransition(), matchesNodeState);
    }

    public ILightTransitionCycleConfigurator Add(Func<ILightPipelineContext, LightTransition?, LightParameters?> lightParametersFactory, Func<ILightPipelineContext, bool> matchesNodeState)
    {
        return Add((c, t) => lightParametersFactory(c, t)?.AsTransition(), matchesNodeState);
    }

    public ILightTransitionCycleConfigurator Add(LightTransition lightTransition)
    {
        return Add(new StaticLightTransitionNode(lightTransition, scheduler), _ => LightEntity.SceneEquals(lightTransition.LightParameters));
    }

    public ILightTransitionCycleConfigurator Add(Func<ILightPipelineContext, LightTransition?> lightTransitionFactory, Func<ILightPipelineContext, bool> matchesNodeState)
    {
        return Add(c => new StaticLightTransitionNode(lightTransitionFactory(c), scheduler), matchesNodeState);
    }

    public ILightTransitionCycleConfigurator Add(Func<ILightPipelineContext, LightTransition?, LightTransition?> lightTransitionFactory, Func<ILightPipelineContext, bool> matchesNodeState)
    {
        return Add(c => new FactoryNode<LightTransition>(t => lightTransitionFactory(c, t)), matchesNodeState);
    }

    public ILightTransitionCycleConfigurator Add<TNode>(Func<ILightPipelineContext, bool> matchesNodeState) where TNode : IPipelineNode<LightTransition>
    {
        return Add(c => c.ServiceProvider.CreateInstanceWithinContext<TNode>(c), matchesNodeState);
    }

    public ILightTransitionCycleConfigurator Add(IPipelineNode<LightTransition> node, Func<ILightPipelineContext, bool> matchesNodeState)
    {
        return Add(_ => node, matchesNodeState);
    }

    public ILightTransitionCycleConfigurator Add(Func<ILightPipelineContext, IPipelineNode<LightTransition>> nodeFactory, Func<ILightPipelineContext, bool> matchesNodeState)
    {
        CycleNodeFactories.Add((nodeFactory, matchesNodeState));
        return this;
    }

    public ILightTransitionCycleConfigurator AddPassThrough(Func<ILightPipelineContext, bool> matchesNodeState)
    {
        return Add(new PassThroughNode<LightTransition>(), matchesNodeState);
    }

    public ILightTransitionCycleConfigurator ForLight(string lightEntityId, Action<ILightTransitionCycleConfigurator> configure, ExcludedLightBehaviours excludedLightBehaviour = ExcludedLightBehaviours.None) => ForLights([lightEntityId], configure, excludedLightBehaviour);

    public ILightTransitionCycleConfigurator ForLight(ILight lightEntity, Action<ILightTransitionCycleConfigurator> configure, ExcludedLightBehaviours excludedLightBehaviour = ExcludedLightBehaviours.None) => ForLights([lightEntity], configure, excludedLightBehaviour);

    public ILightTransitionCycleConfigurator ForLights(IEnumerable<string> lightEntityIds, Action<ILightTransitionCycleConfigurator> configure, ExcludedLightBehaviours excludedLightBehaviour = ExcludedLightBehaviours.None)
    {
        CompositeHelper.ValidateLightEntities(lightEntityIds, LightEntity.Id);
        return this;
    }

    public ILightTransitionCycleConfigurator ForLights(IEnumerable<ILight> lightEntities, Action<ILightTransitionCycleConfigurator> configure, ExcludedLightBehaviours excludedLightBehaviour = ExcludedLightBehaviours.None) => ForLights(lightEntities.Select(e => e.EntityId), configure, excludedLightBehaviour);
}