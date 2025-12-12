using System.Reactive.Concurrency;
using AutomationPipelines;
using CodeCasa.AutomationPipelines.Lights.Context;
using CodeCasa.AutomationPipelines.Lights.Extensions;
using CodeCasa.AutomationPipelines.Lights.Pipeline;
using NetDaemon.Devices.Abstractions;


using NetDaemon.HassModel;
using NetDaemon.HassModel.Entities;
using NetDaemon.Lights;

namespace CodeCasa.AutomationPipelines.Lights.ReactiveNode;

public partial class CompositeLightTransitionReactiveNodeConfigurator(
    IServiceProvider serviceProvider,
    IHaContext haContext,
    LightPipelineFactory lightPipelineFactory,
    ReactiveNodeFactory reactiveNodeFactory,
    Dictionary<string, LightTransitionReactiveNodeConfigurator> configurators,
    IScheduler scheduler)
    : ILightTransitionReactiveNodeConfigurator
{
    public ILightTransitionReactiveNodeConfigurator<TLight> SetName(string name)
    {
        configurators.Values.ForEach(c => c.SetName(name));
        return this;
    }

    public ILightTransitionReactiveNodeConfigurator<TLight> AddReactiveDimmer(IDimmer dimmer)
    {
        foreach (var configurator in configurators)
        {
            configurator.Value.AddReactiveDimmer(dimmer);
        }
        return this;
    }

    public ILightTransitionReactiveNodeConfigurator<TLight> SetReactiveDimmerOptions(DimmerOptions dimmerOptions)
    {
        foreach (var configurator in configurators)
        {
            configurator.Value.SetReactiveDimmerOptions(dimmerOptions);
        }
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

        var dimPulses = dimmer.Dimming.ToPulsesWhenTrue(options.TimeBetweenSteps, scheduler);
        var brightenPulses = dimmer.Brightening.ToPulsesWhenTrue(options.TimeBetweenSteps, scheduler);

        var configuratorsWithLightEntity = options.ValidateAndOrderMultipleLightEntityTypes(configurators)
            .Select(kvp => (configurator: kvp.Value, lightEntity: new LightEntity(haContext, kvp.Key))).ToArray();
        var lightEntitiesInDimOrder = configuratorsWithLightEntity.Select(t => t.lightEntity).ToArray();
        foreach (var containerAndLight in configuratorsWithLightEntity)
        {
            // Note: this is not strictly required, but I think it's neater and might prevent issues.
            var lightEntitiesInDimOrderWithContainerInstance = lightEntitiesInDimOrder.Select(l => l.EntityId == containerAndLight.lightEntity.EntityId ? containerAndLight.lightEntity : l);
            containerAndLight.configurator.AddDimPulses(options, lightEntitiesInDimOrderWithContainerInstance, dimPulses, brightenPulses);
        }
        return this;
    }

    public ILightTransitionReactiveNodeConfigurator<TLight> AddNodeSource(IObservable<Func<ILightPipelineContext, IPipelineNode<LightTransition>?>> nodeFactorySource)
    {
        configurators.Values.ForEach(c => c.AddNodeSource(nodeFactorySource));
        return this;
    }

    public ILightTransitionReactiveNodeConfigurator<TLight> ForLight(string lightEntityId, Action<ILightTransitionReactiveNodeConfigurator> configure) => ForLights([lightEntityId], configure);

    public ILightTransitionReactiveNodeConfigurator<TLight> ForLight(ILightEntityCore lightEntity, Action<ILightTransitionReactiveNodeConfigurator> configure) => ForLights([lightEntity], configure);

    public ILightTransitionReactiveNodeConfigurator<TLight> ForLights(IEnumerable<string> lightEntityIds, Action<ILightTransitionReactiveNodeConfigurator> configure)
    {
        var lightEntityIdsArray =
            CompositeHelper.ResolveAndValidateLightEntities(haContext, lightEntityIds, configurators.Keys);

        if (lightEntityIdsArray.Length == configurators.Count)
        {
            configure(this);
            return this;
        }
        if (lightEntityIdsArray.Length == 1)
        {
            configure(configurators[lightEntityIdsArray.First()]);
            return this;
        }

        configure(new CompositeLightTransitionReactiveNodeConfigurator(
            serviceProvider, haContext,
            lightPipelineFactory,
            reactiveNodeFactory,
            configurators
            .Where(kvp => lightEntityIdsArray.Contains(kvp.Key)).ToDictionary(kvp => kvp.Key, kvp => kvp.Value), scheduler));
        return this;
    }

    public ILightTransitionReactiveNodeConfigurator<TLight> ForLights(IEnumerable<ILightEntityCore> lightEntities, Action<ILightTransitionReactiveNodeConfigurator> configure) => ForLights(lightEntities.Select(e => e.EntityId), configure);

    private record LightEntity(IHaContext HaContext, string EntityId) : ILightEntityCore;
}