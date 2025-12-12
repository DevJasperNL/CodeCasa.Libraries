using System.Reactive.Concurrency;
using AutomationPipelines;
using CodeCasa.AutomationPipelines.Lights.Context;
using CodeCasa.AutomationPipelines.Lights.Extensions;
using CodeCasa.AutomationPipelines.Lights.ReactiveNode;
using NetDaemon.Devices.Abstractions;
using NetDaemon.HassModel;
using NetDaemon.HassModel.Entities;
using NetDaemon.Lights;

namespace CodeCasa.AutomationPipelines.Lights.Pipeline
{
    public partial class CompositeLightTransitionPipelineConfigurator(
        IServiceProvider serviceProvider,
        IHaContext haContext,
        LightPipelineFactory lightPipelineFactory,
        ReactiveNodeFactory reactiveNodeFactory,
        Dictionary<string, LightTransitionPipelineConfigurator> nodeContainers,
        IScheduler scheduler)
        : ILightTransitionPipelineConfigurator
    {
        public Dictionary<string, LightTransitionPipelineConfigurator> NodeContainers { get; } = nodeContainers;

        public ILightTransitionPipelineConfigurator SetName(string name)
        {
            NodeContainers.Values.ForEach(b => b.SetName(name));
            return this;
        }

        public ILightTransitionPipelineConfigurator AddNode<TNode>() where TNode : IPipelineNode<LightTransition>
        {
            NodeContainers.Values.ForEach(b => b.AddNode<TNode>());
            return this;
        }

        public ILightTransitionPipelineConfigurator AddNode(Func<ILightPipelineContext, IPipelineNode<LightTransition>> nodeFactory)
        {
            NodeContainers.Values.ForEach(b => b.AddNode(nodeFactory));
            return this;
        }

        public ILightTransitionPipelineConfigurator AddReactiveNode(
            Action<ILightTransitionReactiveNodeConfigurator> configure)
        {
            var nodes = reactiveNodeFactory.CreateReactiveNodes(NodeContainers.Select(nc => nc.Value.LightEntity),
                configure);
            NodeContainers.ForEach(kvp => kvp.Value.AddNode(nodes[kvp.Key]));
            return this;
        }

        public ILightTransitionPipelineConfigurator AddPipeline(Action<ILightTransitionPipelineConfigurator> pipelineNodeOptions)
        {
            var pipelines = lightPipelineFactory.CreateLightPipelines(NodeContainers.Select(c => c.Value.LightEntity),
                pipelineNodeOptions);
            NodeContainers.ForEach(kvp => kvp.Value.AddNode(pipelines[kvp.Key]));
            return this;
        }

        public ILightTransitionPipelineConfigurator AddDimmer(IDimmer dimmer)
        {
            return AddDimmer(dimmer, _ => { });
        }

        public ILightTransitionPipelineConfigurator AddDimmer(IDimmer dimmer, Action<DimmerOptions> dimOptions)
        {
            return AddReactiveNode(c =>
            {
                c.AddUncoupledDimmer(dimmer, dimOptions);
            });
        }

        public ILightTransitionPipelineConfigurator ForLight(string lightEntityId,
            Action<ILightTransitionPipelineConfigurator> compositeNodeBuilder) => ForLights([lightEntityId], compositeNodeBuilder);

        public ILightTransitionPipelineConfigurator ForLight(ILightEntityCore lightEntity,
            Action<ILightTransitionPipelineConfigurator> compositeNodeBuilder) => ForLights([lightEntity], compositeNodeBuilder);

        public ILightTransitionPipelineConfigurator ForLights(IEnumerable<string> lightEntityIds,
            Action<ILightTransitionPipelineConfigurator> compositeNodeBuilder)
        {
            var lightEntityIdsArray =
                CompositeHelper.ResolveAndValidateLightEntities(haContext, lightEntityIds, NodeContainers.Keys);

            if (lightEntityIdsArray.Length == NodeContainers.Count)
            {
                compositeNodeBuilder(this);
                return this;
            }
            if (lightEntityIdsArray.Length == 1)
            {
                compositeNodeBuilder(NodeContainers[lightEntityIdsArray.First()]);
                return this;
            }

            compositeNodeBuilder(new CompositeLightTransitionPipelineConfigurator(serviceProvider, haContext, lightPipelineFactory, reactiveNodeFactory, NodeContainers
                    .Where(kvp => lightEntityIdsArray.Contains(kvp.Key)).ToDictionary(kvp => kvp.Key, kvp => kvp.Value),
                scheduler));
            return this;
        }

        public ILightTransitionPipelineConfigurator ForLights(IEnumerable<ILightEntityCore> lightEntities,
            Action<ILightTransitionPipelineConfigurator> compositeNodeBuilder) => ForLights(lightEntities.Select(e => e.EntityId), compositeNodeBuilder);
    }
}
