using AutomationPipelines;
using Microsoft.Extensions.Logging;
using NetDaemon.HassModel;
using NetDaemon.HassModel.Entities;
using System.Reactive.Concurrency;
using CodeCasa.AutomationPipelines.Lights.ReactiveNode;
using CodeCasa.Lights;
using NetDaemon.Lights;
using NetDaemon.Lights.Extensions;

namespace CodeCasa.AutomationPipelines.Lights.Pipeline
{
    public class LightPipelineFactory(
        ILogger<Pipeline<LightTransition>> logger, IServiceProvider serviceProvider, ReactiveNodeFactory reactiveNodeFactory, IScheduler scheduler)
    {
        public IAsyncDisposable SetupLightPipeline(ILightEntityCore lightEntity,
            Action<ILightTransitionPipelineConfigurator> pipelineBuilder)
        {
            var disposables = new CompositeAsyncDisposable();
            var pipelines = CreateLightPipelines(lightEntity.Flatten(), pipelineBuilder);
            foreach (var pipeline in pipelines.Values)
            {
                disposables.Add(pipeline);
            }
            return disposables;
        }

        internal IPipeline<LightTransition> CreateLightPipeline(ILightEntityCore lightEntity, Action<ILightTransitionPipelineConfigurator> pipelineBuilder)
        {
            return CreateLightPipelines([lightEntity], pipelineBuilder)[lightEntity.EntityId];
        }

        internal Dictionary<string, IPipeline<LightTransition>> CreateLightPipelines(IEnumerable<ILightEntityCore> lightEntities, Action<ILightTransitionPipelineConfigurator> pipelineBuilder)
        {
            // todo: is this assumption correct? Make internal?
            // Note: we simply assume that these are not groups.
            var lightEntityArray = lightEntities.ToArray();
            if (!lightEntityArray.Any())
            {
                return new Dictionary<string, IPipeline<LightTransition>>();
            }

            var configurators = lightEntityArray.ToDictionary(l => l.EntityId, l => new LightTransitionPipelineConfigurator(serviceProvider, this, reactiveNodeFactory, l, scheduler));
            ILightTransitionPipelineConfigurator configurator = lightEntityArray.Length == 1
                ? configurators[lightEntityArray[0].EntityId]
                : new CompositeLightTransitionPipelineConfigurator(
                    serviceProvider,
                    haContext,
                    this,
                    reactiveNodeFactory,
                    configurators, scheduler);
            pipelineBuilder(configurator);

            return configurators.ToDictionary(kvp => kvp.Key, kvp =>
            {
                var conf = kvp.Value;
                return (IPipeline<LightTransition>)new Pipeline<LightTransition>(
                    conf.Name ?? conf.LightEntity.EntityId,
                    LightTransition.Off(),
                    conf.Nodes,
                    conf.LightEntity.ExecuteLightTransition,
                    logger);
            });
        }
    }
}
