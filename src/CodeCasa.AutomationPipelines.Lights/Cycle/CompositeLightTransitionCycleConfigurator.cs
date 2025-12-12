using System.Reactive.Concurrency;
using AutomationPipelines;
using CodeCasa.AutomationPipelines.Lights.Context;
using CodeCasa.AutomationPipelines.Lights.Extensions;
using CodeCasa.AutomationPipelines.Lights.Nodes;
using CodeCasa.Lights;
using CodeCasa.Lights.Extensions;
using Microsoft.Extensions.DependencyInjection;

namespace CodeCasa.AutomationPipelines.Lights.Cycle;

internal class CompositeLightTransitionCycleConfigurator(
    Dictionary<string, LightTransitionCycleConfigurator> activeConfigurators, 
    Dictionary<string, LightTransitionCycleConfigurator> inactiveConfigurators)
    : ILightTransitionCycleConfigurator
{
    public ILightTransitionCycleConfigurator AddOff()
    {
        var matchesNodeState = () => activeConfigurators.Values.All(c => c.LightEntity.IsOff());
        activeConfigurators.Values.ForEach(c => c.Add<TurnOffThenPassThroughNode>(_ => matchesNodeState()));
        inactiveConfigurators.Values.ForEach(c => c.AddPassThrough(_ => matchesNodeState()));
        return this;
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
        return Add(_ => lightTransition, _ => activeConfigurators.Values.All(c => c.LightEntity.SceneEquals(lightTransition.LightParameters)));
    }

    public ILightTransitionCycleConfigurator Add(Func<ILightPipelineContext, LightTransition?> lightTransitionFactory, Func<ILightPipelineContext, bool> matchesNodeState)
    {
        return Add(c => new StaticLightTransitionNode(lightTransitionFactory(c), c.ServiceProvider.GetRequiredService<IScheduler>()), matchesNodeState);
    }

    public ILightTransitionCycleConfigurator Add(Func<ILightPipelineContext, LightTransition?, LightTransition?> lightTransitionFactory, Func<ILightPipelineContext, bool> matchesNodeState)
    {
        return Add(c => new FactoryNode<LightTransition>(t => lightTransitionFactory(c, t)), matchesNodeState);
    }

    public ILightTransitionCycleConfigurator Add<TNode>(Func<ILightPipelineContext, bool> matchesNodeState) where TNode : IPipelineNode<LightTransition>
    {
        activeConfigurators.Values.ForEach(c => c.Add<TNode>(matchesNodeState));
        inactiveConfigurators.Values.ForEach(c => c.AddPassThrough(matchesNodeState));
        return this;
    }

    public ILightTransitionCycleConfigurator Add(Func<ILightPipelineContext, IPipelineNode<LightTransition>> nodeFactory, Func<ILightPipelineContext, bool> matchesNodeState)
    {
        activeConfigurators.Values.ForEach(c => c.Add(nodeFactory, matchesNodeState));
        inactiveConfigurators.Values.ForEach(c => c.AddPassThrough(matchesNodeState));
        return this;
    }

    public ILightTransitionCycleConfigurator AddPassThrough(Func<ILightPipelineContext, bool> matchesNodeState)
    {
        activeConfigurators.Values.ForEach(c => c.AddPassThrough(matchesNodeState));
        inactiveConfigurators.Values.ForEach(c => c.AddPassThrough(matchesNodeState));
        return this;
    }

    public ILightTransitionCycleConfigurator ForLight(string lightEntityId, Action<ILightTransitionCycleConfigurator> configure, ExcludedLightBehaviours excludedLightBehaviour = ExcludedLightBehaviours.None) => ForLights([lightEntityId], configure, excludedLightBehaviour);

    public ILightTransitionCycleConfigurator ForLight(ILight lightEntity, Action<ILightTransitionCycleConfigurator> configure, ExcludedLightBehaviours excludedLightBehaviour = ExcludedLightBehaviours.None) => ForLights([lightEntity], configure, excludedLightBehaviour);

    public ILightTransitionCycleConfigurator ForLights(IEnumerable<string> lightEntityIds,
        Action<ILightTransitionCycleConfigurator> configure,
        ExcludedLightBehaviours excludedLightBehaviour = ExcludedLightBehaviours.None)
    {
        var lightEntityIdsArray =
            CompositeHelper.ResolveAndValidateLightEntities(lightEntityIds, activeConfigurators.Keys);

        if (lightEntityIdsArray.Length == activeConfigurators.Count)
        {
            configure(this);
            return this;
        }

        if (excludedLightBehaviour == ExcludedLightBehaviours.None)
        {
            if (lightEntityIdsArray.Length == 1)
            {
                configure(activeConfigurators[lightEntityIdsArray.First()]);
                return this;
            }

            configure(new CompositeLightTransitionCycleConfigurator(
                activeConfigurators.Where(kvp => lightEntityIdsArray.Contains(kvp.Key))
                    .ToDictionary(kvp => kvp.Key, kvp => kvp.Value), []));
            return this;
        }

        configure(new CompositeLightTransitionCycleConfigurator(
            activeConfigurators.Where(kvp => lightEntityIdsArray.Contains(kvp.Key))
                .ToDictionary(kvp => kvp.Key, kvp => kvp.Value),
            activeConfigurators.Where(kvp => !lightEntityIdsArray.Contains(kvp.Key))
                .ToDictionary(kvp => kvp.Key, kvp => kvp.Value)));
        return this;
    }

    public ILightTransitionCycleConfigurator ForLights(IEnumerable<ILight> lightEntities, Action<ILightTransitionCycleConfigurator> configure, ExcludedLightBehaviours excludedLightBehaviour = ExcludedLightBehaviours.None) => ForLights(lightEntities.Select(e => e.Id), configure, excludedLightBehaviour);
}