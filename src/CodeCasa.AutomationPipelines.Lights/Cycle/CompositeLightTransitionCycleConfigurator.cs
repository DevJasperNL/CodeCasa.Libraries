using System.Reactive.Concurrency;
using AutomationPipelines;
using CodeCasa.AutomationPipelines.Lights.Context;
using CodeCasa.AutomationPipelines.Lights.Extensions;
using CodeCasa.AutomationPipelines.Lights.Nodes;
using CodeCasa.Lights;
using CodeCasa.Lights.Extensions;
using Microsoft.Extensions.DependencyInjection;

namespace CodeCasa.AutomationPipelines.Lights.Cycle;

internal class CompositeLightTransitionCycleConfigurator<TLight>(
    Dictionary<string, LightTransitionCycleConfigurator<TLight>> activeConfigurators, 
    Dictionary<string, LightTransitionCycleConfigurator<TLight>> inactiveConfigurators)
    : ILightTransitionCycleConfigurator<TLight> where TLight : ILight
{
    public ILightTransitionCycleConfigurator<TLight> AddOff()
    {
        var matchesNodeState = () => activeConfigurators.Values.All(c => c.LightEntity.IsOff());
        activeConfigurators.Values.ForEach(c => c.Add<TurnOffThenPassThroughNode>(_ => matchesNodeState()));
        inactiveConfigurators.Values.ForEach(c => c.AddPassThrough(_ => matchesNodeState()));
        return this;
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
        return Add(_ => lightTransition, _ => activeConfigurators.Values.All(c => c.LightEntity.SceneEquals(lightTransition.LightParameters)));
    }

    public ILightTransitionCycleConfigurator<TLight> Add(Func<ILightPipelineContext<TLight>, LightTransition?> lightTransitionFactory, Func<ILightPipelineContext<TLight>, bool> matchesNodeState)
    {
        return Add(c => new StaticLightTransitionNode(lightTransitionFactory(c), c.ServiceProvider.GetRequiredService<IScheduler>()), matchesNodeState);
    }

    public ILightTransitionCycleConfigurator<TLight> Add(Func<ILightPipelineContext<TLight>, LightTransition?, LightTransition?> lightTransitionFactory, Func<ILightPipelineContext<TLight>, bool> matchesNodeState)
    {
        return Add(c => new FactoryNode<LightTransition>(t => lightTransitionFactory(c, t)), matchesNodeState);
    }

    public ILightTransitionCycleConfigurator<TLight> Add<TNode>(Func<ILightPipelineContext<TLight>, bool> matchesNodeState) where TNode : IPipelineNode<LightTransition>
    {
        activeConfigurators.Values.ForEach(c => c.Add<TNode>(matchesNodeState));
        inactiveConfigurators.Values.ForEach(c => c.AddPassThrough(matchesNodeState));
        return this;
    }

    public ILightTransitionCycleConfigurator<TLight> Add(Func<ILightPipelineContext<TLight>, IPipelineNode<LightTransition>> nodeFactory, Func<ILightPipelineContext<TLight>, bool> matchesNodeState)
    {
        activeConfigurators.Values.ForEach(c => c.Add(nodeFactory, matchesNodeState));
        inactiveConfigurators.Values.ForEach(c => c.AddPassThrough(matchesNodeState));
        return this;
    }

    public ILightTransitionCycleConfigurator<TLight> AddPassThrough(Func<ILightPipelineContext<TLight>, bool> matchesNodeState)
    {
        activeConfigurators.Values.ForEach(c => c.AddPassThrough(matchesNodeState));
        inactiveConfigurators.Values.ForEach(c => c.AddPassThrough(matchesNodeState));
        return this;
    }

    public ILightTransitionCycleConfigurator<TLight> ForLight(string lightEntityId, Action<ILightTransitionCycleConfigurator> configure, ExcludedLightBehaviours excludedLightBehaviour = ExcludedLightBehaviours.None) => ForLights([lightEntityId], configure, excludedLightBehaviour);

    public ILightTransitionCycleConfigurator<TLight> ForLight(ILightEntityCore lightEntity, Action<ILightTransitionCycleConfigurator> configure, ExcludedLightBehaviours excludedLightBehaviour = ExcludedLightBehaviours.None) => ForLights([lightEntity], configure, excludedLightBehaviour);

    public ILightTransitionCycleConfigurator<TLight> ForLights(IEnumerable<string> lightEntityIds,
        Action<ILightTransitionCycleConfigurator> configure,
        ExcludedLightBehaviours excludedLightBehaviour = ExcludedLightBehaviours.None)
    {
        var lightEntityIdsArray =
            CompositeHelper.ResolveAndValidateLightEntities(haContext, lightEntityIds, activeConfigurators.Keys);

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

            configure(new CompositeLightTransitionCycleConfigurator(haContext,
                activeConfigurators.Where(kvp => lightEntityIdsArray.Contains(kvp.Key))
                    .ToDictionary(kvp => kvp.Key, kvp => kvp.Value), []));
            return this;
        }

        configure(new CompositeLightTransitionCycleConfigurator(haContext,
            activeConfigurators.Where(kvp => lightEntityIdsArray.Contains(kvp.Key))
                .ToDictionary(kvp => kvp.Key, kvp => kvp.Value),
            activeConfigurators.Where(kvp => !lightEntityIdsArray.Contains(kvp.Key))
                .ToDictionary(kvp => kvp.Key, kvp => kvp.Value)));
        return this;
    }

    public ILightTransitionCycleConfigurator<TLight> ForLights(IEnumerable<ILightEntityCore> lightEntities, Action<ILightTransitionCycleConfigurator> configure, ExcludedLightBehaviours excludedLightBehaviour = ExcludedLightBehaviours.None) => ForLights(lightEntities.Select(e => e.EntityId), configure, excludedLightBehaviour);
}