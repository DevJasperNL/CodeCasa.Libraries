using System.Reactive;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using AutomationPipelines;
using CodeCasa.Abstractions;
using CodeCasa.AutomationPipelines.Lights.Context;
using CodeCasa.AutomationPipelines.Lights.Extensions;
using CodeCasa.AutomationPipelines.Lights.Nodes;
using CodeCasa.AutomationPipelines.Lights.Pipeline;
using CodeCasa.Lights;

namespace CodeCasa.AutomationPipelines.Lights.ReactiveNode;

public partial class LightTransitionReactiveNodeConfigurator(
    IServiceProvider serviceProvider,
    LightPipelineFactory lightPipelineFactory,
    ReactiveNodeFactory reactiveNodeFactory,
    ILight lightEntity, 
    IScheduler scheduler) : ILightTransitionReactiveNodeConfigurator
{
    public ILight LightEntity { get; } = lightEntity;

    internal string? Name { get; private set; }
    internal List<IObservable<IPipelineNode<LightTransition>?>> NodeObservables { get; } = new();
    internal List<IDimmer> Dimmers { get; } = new();
    internal DimmerOptions DimmerOptions { get; private set; } = new ();
    
    public ILightTransitionReactiveNodeConfigurator SetName(string name)
    {
        Name = name;
        return this;
    }

    public ILightTransitionReactiveNodeConfigurator AddReactiveDimmer(IDimmer dimmer)
    {
        Dimmers.Add(dimmer);
        return this;
    }

    public ILightTransitionReactiveNodeConfigurator SetReactiveDimmerOptions(DimmerOptions dimmerOptions)
    {
        DimmerOptions = dimmerOptions;
        return this;
    }

    public ILightTransitionReactiveNodeConfigurator AddUncoupledDimmer(IDimmer dimmer)
    {
        return AddUncoupledDimmer(dimmer, _ => { });
    }

    public ILightTransitionReactiveNodeConfigurator AddUncoupledDimmer(IDimmer dimmer, Action<DimmerOptions> dimOptions)
    {
        var options = new DimmerOptions();
        dimOptions(options);
        options.ValidateSingleLightEntity(LightEntity.Id);

        var dimPulses = dimmer.Dimming.ToPulsesWhenTrue(options.TimeBetweenSteps, scheduler);
        var brightenPulses = dimmer.Brightening.ToPulsesWhenTrue(options.TimeBetweenSteps, scheduler);

        AddDimPulses(options, [LightEntity], dimPulses, brightenPulses);
        return this;
    }

    internal void AddDimPulses(DimmerOptions options, IEnumerable<ILight> lightsInDimOrder, IObservable<Unit> dimPulses, IObservable<Unit> brightenPulses)
    {
        var dimHelper = new DimHelper(LightEntity, lightsInDimOrder, options.MinBrightness, options.BrightnessStep);
        AddNodeSource(dimPulses
            .Select(_ => dimHelper.DimStep())
            .Where(t => t != null)
            .Select(t => (IPipelineNode<LightTransition>)(t == LightTransition.Off() ? new TurnOffThenPassThroughNode() : new StaticLightTransitionNode(t, scheduler))));
        AddNodeSource(brightenPulses
            .Select(_ => dimHelper.BrightenStep())
            .Where(t => t != null)
            .Select(t => (IPipelineNode<LightTransition>)(t == LightTransition.Off() ? new TurnOffThenPassThroughNode() : new StaticLightTransitionNode(t, scheduler))));
    }

    public ILightTransitionReactiveNodeConfigurator AddNodeSource(IObservable<IPipelineNode<LightTransition>?> nodeSource)
    {
        NodeObservables.Add(nodeSource);
        return this;
    }

    public ILightTransitionReactiveNodeConfigurator AddNodeSource(IObservable<Func<ILightPipelineContext, IPipelineNode<LightTransition>?>> nodeFactorySource)
    {
        return AddNodeSource(nodeFactorySource.Select(f => f(new LightPipelineContext(serviceProvider, LightEntity))));
    }

    public ILightTransitionReactiveNodeConfigurator ForLight(string lightEntityId,
        Action<ILightTransitionReactiveNodeConfigurator> configure) => ForLights([lightEntityId], configure);

    public ILightTransitionReactiveNodeConfigurator ForLight(ILight lightEntity,
        Action<ILightTransitionReactiveNodeConfigurator> configure) => ForLights([lightEntity], configure);

    public ILightTransitionReactiveNodeConfigurator ForLights(IEnumerable<string> lightEntityIds,
        Action<ILightTransitionReactiveNodeConfigurator> configure)
    {
        CompositeHelper.ValidateLightEntities(lightEntityIds, LightEntity.Id);
        return this;
    }

    public ILightTransitionReactiveNodeConfigurator ForLights(IEnumerable<ILight> lightEntities,
        Action<ILightTransitionReactiveNodeConfigurator> configure)
    {
        CompositeHelper.ValidateLightEntities(lightEntities, LightEntity.Id);
        return this;
    }
}