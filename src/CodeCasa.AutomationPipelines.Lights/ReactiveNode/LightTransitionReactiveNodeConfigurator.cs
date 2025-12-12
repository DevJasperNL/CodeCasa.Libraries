using System.Reactive;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using AutomationPipelines;
using CodeCasa.AutomationPipelines.Lights.Context;
using CodeCasa.AutomationPipelines.Lights.Extensions;
using CodeCasa.AutomationPipelines.Lights.Nodes;
using CodeCasa.AutomationPipelines.Lights.Pipeline;
using NetDaemon.Devices.Abstractions;


using NetDaemon.HassModel.Entities;
using NetDaemon.Lights;

namespace CodeCasa.AutomationPipelines.Lights.ReactiveNode;

public partial class LightTransitionReactiveNodeConfigurator(
    IServiceProvider serviceProvider,
    LightPipelineFactory lightPipelineFactory,
    ReactiveNodeFactory reactiveNodeFactory,
    ILightEntityCore lightEntity, 
    IScheduler scheduler) : ILightTransitionReactiveNodeConfigurator
{
    public ILightEntityCore LightEntity { get; } = lightEntity;

    internal string? Name { get; private set; }
    internal List<IObservable<IPipelineNode<LightTransition>?>> NodeObservables { get; } = new();
    internal List<IDimmer> Dimmers { get; } = new();
    internal DimmerOptions DimmerOptions { get; private set; } = new ();
    
    public ILightTransitionReactiveNodeConfigurator<TLight> SetName(string name)
    {
        Name = name;
        return this;
    }

    public ILightTransitionReactiveNodeConfigurator<TLight> AddReactiveDimmer(IDimmer dimmer)
    {
        Dimmers.Add(dimmer);
        return this;
    }

    public ILightTransitionReactiveNodeConfigurator<TLight> SetReactiveDimmerOptions(DimmerOptions dimmerOptions)
    {
        DimmerOptions = dimmerOptions;
        return this;
    }

    public ILightTransitionReactiveNodeConfigurator<TLight> AddUncoupledDimmer(IDimmer dimmer)
    {
        return AddUncoupledDimmer(dimmer, _ => { });
    }

    public ILightTransitionReactiveNodeConfigurator<TLight> AddUncoupledDimmer(IDimmer dimmer, Action<DimmerOptions> dimOptions)
    {
        var options = new DimmerOptions();
        dimOptions(options);
        options.ValidateSingleLightEntity(LightEntity.EntityId);

        var dimPulses = dimmer.Dimming.ToPulsesWhenTrue(options.TimeBetweenSteps, scheduler);
        var brightenPulses = dimmer.Brightening.ToPulsesWhenTrue(options.TimeBetweenSteps, scheduler);

        AddDimPulses(options, [LightEntity], dimPulses, brightenPulses);
        return this;
    }

    internal void AddDimPulses(DimmerOptions options, IEnumerable<ILightEntityCore> lightsInDimOrder, IObservable<Unit> dimPulses, IObservable<Unit> brightenPulses)
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

    public ILightTransitionReactiveNodeConfigurator<TLight> AddNodeSource(IObservable<IPipelineNode<LightTransition>?> nodeSource)
    {
        NodeObservables.Add(nodeSource);
        return this;
    }

    public ILightTransitionReactiveNodeConfigurator<TLight> AddNodeSource(IObservable<Func<ILightPipelineContext, IPipelineNode<LightTransition>?>> nodeFactorySource)
    {
        return AddNodeSource(nodeFactorySource.Select(f => f(new LightPipelineContext(serviceProvider, LightEntity))));
    }

    public ILightTransitionReactiveNodeConfigurator<TLight> ForLight(string lightEntityId,
        Action<ILightTransitionReactiveNodeConfigurator> configure) => ForLights([lightEntityId], configure);

    public ILightTransitionReactiveNodeConfigurator<TLight> ForLight(ILightEntityCore lightEntity,
        Action<ILightTransitionReactiveNodeConfigurator> configure) => ForLights([lightEntity], configure);

    public ILightTransitionReactiveNodeConfigurator<TLight> ForLights(IEnumerable<string> lightEntityIds,
        Action<ILightTransitionReactiveNodeConfigurator> configure)
    {
        CompositeHelper.ValidateLightEntities(LightEntity.HaContext, lightEntityIds, LightEntity.EntityId);
        return this;
    }

    public ILightTransitionReactiveNodeConfigurator<TLight> ForLights(IEnumerable<ILightEntityCore> lightEntities,
        Action<ILightTransitionReactiveNodeConfigurator> configure) => ForLights(lightEntities.Select(e => e.EntityId), configure);
}