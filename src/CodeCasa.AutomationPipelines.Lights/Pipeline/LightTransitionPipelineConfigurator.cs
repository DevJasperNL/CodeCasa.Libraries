using AutomationPipelines;
using CodeCasa.AutomationPipelines.Lights.Context;
using CodeCasa.AutomationPipelines.Lights.Extensions;
using CodeCasa.AutomationPipelines.Lights.ReactiveNode;
using System.Reactive.Concurrency;
using CodeCasa.Abstractions;
using CodeCasa.Lights;

namespace CodeCasa.AutomationPipelines.Lights.Pipeline
{
    public partial class LightTransitionPipelineConfigurator(
        IServiceProvider serviceProvider,
        LightPipelineFactory lightPipelineFactory,
        ReactiveNodeFactory reactiveNodeFactory,
        ILight lightEntity,
        IScheduler scheduler)
        : ILightTransitionPipelineConfigurator
    {
        private readonly List<IPipelineNode<LightTransition>> _nodes = new();

        internal ILight LightEntity { get; } = lightEntity;
        internal string? Name { get; private set; }

        public IReadOnlyCollection<IPipelineNode<LightTransition>> Nodes => _nodes.AsReadOnly();

        public ILightTransitionPipelineConfigurator
            AddConditional(IObservable<bool> observable, Action<ILightTransitionPipelineConfigurator> trueConfigure, Action<ILightTransitionPipelineConfigurator> falseConfigure)
        {
            throw new NotImplementedException();
        }

        public ILightTransitionPipelineConfigurator SetName(string name)
        {
            Name = name;
            return this;
        }

        public ILightTransitionPipelineConfigurator AddNode<TNode>() where TNode : IPipelineNode<LightTransition>
        {
            _nodes.Add(serviceProvider.CreateInstanceWithinContext<TNode>(LightEntity));
            return this;
        }

        public ILightTransitionPipelineConfigurator AddNode(IPipelineNode<LightTransition> node)
        {
            _nodes.Add(node);
            return this;
        }

        public ILightTransitionPipelineConfigurator AddNode(Func<ILightPipelineContext, IPipelineNode<LightTransition>> nodeFactory)
        {
            _nodes.Add(nodeFactory(new LightPipelineContext(serviceProvider, LightEntity)));
            return this;
        }

        public ILightTransitionPipelineConfigurator AddReactiveNode(
            Action<ILightTransitionReactiveNodeConfigurator> configure)
        {
            return AddNode(reactiveNodeFactory.CreateReactiveNode(LightEntity, configure));
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
            Action<ILightTransitionPipelineConfigurator> compositeNodeBuilder) =>
            ForLights([lightEntityId], compositeNodeBuilder);

        public ILightTransitionPipelineConfigurator ForLight(ILight lightEntity,
            Action<ILightTransitionPipelineConfigurator> compositeNodeBuilder) => ForLights([lightEntity], compositeNodeBuilder);

        public ILightTransitionPipelineConfigurator ForLights(IEnumerable<string> lightEntityIds,
            Action<ILightTransitionPipelineConfigurator> compositeNodeBuilder)
        {
            CompositeHelper.ValidateLightEntities(LightEntity.HaContext, lightEntityIds, LightEntity.EntityId);
            return this;
        }

        public ILightTransitionPipelineConfigurator ForLights(IEnumerable<ILight> lightEntities,
            Action<ILightTransitionPipelineConfigurator> compositeNodeBuilder) => ForLights(lightEntities.Select(e => e.EntityId), compositeNodeBuilder);

        public ILightTransitionPipelineConfigurator AddPipeline(Action<ILightTransitionPipelineConfigurator> pipelineNodeOptions) => AddNode(lightPipelineFactory.CreateLightPipeline(LightEntity, pipelineNodeOptions));
    }
}
