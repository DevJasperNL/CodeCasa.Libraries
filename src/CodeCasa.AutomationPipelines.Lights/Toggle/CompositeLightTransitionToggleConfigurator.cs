using System.Reactive.Concurrency;
using AutomationPipelines;
using CodeCasa.AutomationPipelines.Lights.Context;
using CodeCasa.AutomationPipelines.Lights.Extensions;
using CodeCasa.AutomationPipelines.Lights.Nodes;
using CodeCasa.Lights;
using Microsoft.Extensions.DependencyInjection;

namespace CodeCasa.AutomationPipelines.Lights.Toggle
{
    internal class CompositeLightTransitionToggleConfigurator(
        Dictionary<string, LightTransitionToggleConfigurator> activeConfigurators,
        Dictionary<string, LightTransitionToggleConfigurator> inactiveConfigurators) : ILightTransitionToggleConfigurator
    {
        public ILightTransitionToggleConfigurator SetToggleTimeout(TimeSpan timeout)
        {
            activeConfigurators.Values.ForEach(c => c.SetToggleTimeout(timeout));
            inactiveConfigurators.Values.ForEach(c => c.SetToggleTimeout(timeout));
            return this;
        }

        public ILightTransitionToggleConfigurator IncludeOffInToggleCycle()
        {
            activeConfigurators.Values.ForEach(c => c.IncludeOffInToggleCycle());
            inactiveConfigurators.Values.ForEach(c => c.IncludeOffInToggleCycle());
            return this;
        }

        public ILightTransitionToggleConfigurator ExcludeOffFromToggleCycle()
        {
            activeConfigurators.Values.ForEach(c => c.ExcludeOffFromToggleCycle());
            inactiveConfigurators.Values.ForEach(c => c.ExcludeOffFromToggleCycle());
            return this;
        }

        public ILightTransitionToggleConfigurator AddOff()
        {
            return Add<TurnOffThenPassThroughNode>();
        }

        public ILightTransitionToggleConfigurator AddOn()
        {
            return Add(LightTransition.On());
        }

        public ILightTransitionToggleConfigurator Add(LightParameters lightParameters)
        {
            return Add(lightParameters.AsTransition());
        }

        public ILightTransitionToggleConfigurator Add(Func<ILightPipelineContext, LightParameters?> lightParametersFactory)
        {
            return Add(c => lightParametersFactory(c)?.AsTransition());
        }

        public ILightTransitionToggleConfigurator Add(Func<ILightPipelineContext, LightTransition?, LightParameters?> lightParametersFactory)
        {
            return Add((c, t) => lightParametersFactory(c, t)?.AsTransition());
        }

        public ILightTransitionToggleConfigurator Add(LightTransition lightTransition)
        {
            return Add(_ => lightTransition);
        }

        public ILightTransitionToggleConfigurator Add(Func<ILightPipelineContext, LightTransition?> lightTransitionFactory)
        {
            return Add(c => new StaticLightTransitionNode(lightTransitionFactory(c), c.ServiceProvider.GetRequiredService<IScheduler>()));
        }

        public ILightTransitionToggleConfigurator Add(Func<ILightPipelineContext, LightTransition?, LightTransition?> lightTransitionFactory)
        {
            return Add(c => new FactoryNode<LightTransition>(t => lightTransitionFactory(c, t)));
        }

        public ILightTransitionToggleConfigurator Add<TNode>() where TNode : IPipelineNode<LightTransition>
        {
            activeConfigurators.Values.ForEach(c => c.Add<TNode>());
            inactiveConfigurators.Values.ForEach(c => c.AddPassThrough());
            return this;
        }

        public ILightTransitionToggleConfigurator Add(Func<ILightPipelineContext, IPipelineNode<LightTransition>> nodeFactory)
        {
            activeConfigurators.Values.ForEach(c => c.Add(nodeFactory));
            inactiveConfigurators.Values.ForEach(c => c.AddPassThrough());
            return this;
        }

        public ILightTransitionToggleConfigurator AddPassThrough()
        {
            activeConfigurators.Values.ForEach(c => c.AddPassThrough());
            inactiveConfigurators.Values.ForEach(c => c.AddPassThrough());
            return this;
        }

        public ILightTransitionToggleConfigurator ForLight(string lightEntityId, Action<ILightTransitionToggleConfigurator> configure, ExcludedLightBehaviours excludedLightBehaviour = ExcludedLightBehaviours.None) => ForLights([lightEntityId], configure, excludedLightBehaviour);

        public ILightTransitionToggleConfigurator ForLight(ILight lightEntity, Action<ILightTransitionToggleConfigurator> configure, ExcludedLightBehaviours excludedLightBehaviour = ExcludedLightBehaviours.None) => ForLights([lightEntity], configure, excludedLightBehaviour);

        public ILightTransitionToggleConfigurator ForLights(IEnumerable<string> lightEntityIds,
            Action<ILightTransitionToggleConfigurator> configure,
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

                configure(new CompositeLightTransitionToggleConfigurator(
                    activeConfigurators.Where(kvp => lightEntityIdsArray.Contains(kvp.Key))
                        .ToDictionary(kvp => kvp.Key, kvp => kvp.Value), []));
                return this;
            }

            configure(new CompositeLightTransitionToggleConfigurator(
                activeConfigurators.Where(kvp => lightEntityIdsArray.Contains(kvp.Key))
                    .ToDictionary(kvp => kvp.Key, kvp => kvp.Value),
                activeConfigurators.Where(kvp => !lightEntityIdsArray.Contains(kvp.Key))
                    .ToDictionary(kvp => kvp.Key, kvp => kvp.Value)));
            return this;
        }

        public ILightTransitionToggleConfigurator ForLights(IEnumerable<ILight> lightEntities, Action<ILightTransitionToggleConfigurator> configure, ExcludedLightBehaviours excludedLightBehaviour = ExcludedLightBehaviours.None) => ForLights(lightEntities.Select(e => e.EntityId), configure, excludedLightBehaviour);

    }
}
