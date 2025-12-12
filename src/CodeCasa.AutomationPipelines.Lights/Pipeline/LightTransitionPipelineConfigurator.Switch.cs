using AutomationPipelines;
using CodeCasa.AutomationPipelines.Lights.Context;
using CodeCasa.AutomationPipelines.Lights.Extensions;
using CodeCasa.AutomationPipelines.Lights.Nodes;
using CodeCasa.AutomationPipelines.Lights.ReactiveNode;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using Microsoft.Extensions.DependencyInjection;
using NetDaemon.Lights;
using NetDaemon.Lights.Extensions;
using NetDaemon.Lights.Scenes;

namespace CodeCasa.AutomationPipelines.Lights.Pipeline;

public partial class LightTransitionPipelineConfigurator
{
    public ILightTransitionPipelineConfigurator Switch<TObservable>(LightSceneTemplate trueLightSceneTemplate,
        LightSceneTemplate falseLightSceneTemplate) where TObservable : IObservable<bool>
    {
        return Switch<TObservable>(trueLightSceneTemplate(LightEntity), falseLightSceneTemplate(LightEntity));
    }

    public ILightTransitionPipelineConfigurator Switch(IObservable<bool> observable, LightSceneTemplate trueLightSceneTemplate,
        LightSceneTemplate falseLightSceneTemplate)
    {
        return Switch(observable, trueLightSceneTemplate(LightEntity), falseLightSceneTemplate(LightEntity));
    }

    public ILightTransitionPipelineConfigurator Switch<TObservable>(Func<LightSceneTemplate> trueLightSceneTemplateFactory,
        Func<LightSceneTemplate> falseLightSceneTemplateFactory) where TObservable : IObservable<bool>
    {
        return Switch<TObservable>(
            new Func<ILightPipelineContext, LightParameters>(_ => trueLightSceneTemplateFactory()(LightEntity)),
            _ => falseLightSceneTemplateFactory()(LightEntity));
    }

    public ILightTransitionPipelineConfigurator Switch(IObservable<bool> observable, Func<LightSceneTemplate> trueLightSceneTemplateFactory,
        Func<LightSceneTemplate> falseLightSceneTemplateFactory)
    {
        return Switch(observable,
            new Func<ILightPipelineContext, LightParameters>(_ => trueLightSceneTemplateFactory()(LightEntity)),
            _ => falseLightSceneTemplateFactory()(LightEntity));
    }

    public ILightTransitionPipelineConfigurator Switch<TObservable>(LightParameters trueLightParameters,
        LightParameters falseLightParameters) where TObservable : IObservable<bool>
    {
        return Switch<TObservable>(trueLightParameters.AsTransition(), falseLightParameters.AsTransition());
    }

    public ILightTransitionPipelineConfigurator Switch(IObservable<bool> observable, LightParameters trueLightParameters,
        LightParameters falseLightParameters)
    {
        return Switch(observable, trueLightParameters.AsTransition(), falseLightParameters.AsTransition());
    }

    public ILightTransitionPipelineConfigurator Switch<TObservable>(Func<ILightPipelineContext, LightParameters> trueLightParametersFactory,
        Func<ILightPipelineContext, LightParameters> falseLightParametersFactory) where TObservable : IObservable<bool>
    {
        return Switch<TObservable>(c => falseLightParametersFactory(c).AsTransition(), c => trueLightParametersFactory(c).AsTransition());
    }

    public ILightTransitionPipelineConfigurator Switch(IObservable<bool> observable, Func<ILightPipelineContext, LightParameters> trueLightParametersFactory,
        Func<ILightPipelineContext, LightParameters> falseLightParametersFactory)
    {
        return Switch(observable, c => trueLightParametersFactory(c).AsTransition(), c => falseLightParametersFactory(c).AsTransition());
    }

    public ILightTransitionPipelineConfigurator Switch<TObservable>(LightTransition trueLightTransition,
        LightTransition falseLightTransition) where TObservable : IObservable<bool>
    {
        return Switch<TObservable>(trueLightTransition, falseLightTransition);
    }

    public ILightTransitionPipelineConfigurator Switch(IObservable<bool> observable, LightTransition trueLightTransition,
        LightTransition falseLightTransition)
    {
        return Switch(observable, trueLightTransition, falseLightTransition);
    }

    public ILightTransitionPipelineConfigurator Switch<TObservable>(Func<ILightPipelineContext, LightTransition> trueLightTransitionFactory,
        Func<ILightPipelineContext, LightTransition> falseLightTransitionFactory) where TObservable : IObservable<bool>
    {
        return Switch<TObservable>(
            c => new StaticLightTransitionNode(trueLightTransitionFactory(c), c.ServiceProvider.GetRequiredService<IScheduler>()), 
            c => new StaticLightTransitionNode(falseLightTransitionFactory(c), c.ServiceProvider.GetRequiredService<IScheduler>()));
    }

    public ILightTransitionPipelineConfigurator Switch(IObservable<bool> observable, Func<ILightPipelineContext, LightTransition> trueLightTransitionFactory,
        Func<ILightPipelineContext, LightTransition> falseLightTransitionFactory)
    {
        return Switch(
            observable,
            c => new StaticLightTransitionNode(trueLightTransitionFactory(c), c.ServiceProvider.GetRequiredService<IScheduler>()),
            c => new StaticLightTransitionNode(falseLightTransitionFactory(c), c.ServiceProvider.GetRequiredService<IScheduler>()));
    }

    public ILightTransitionPipelineConfigurator Switch<TObservable>(Func<ILightPipelineContext, IPipelineNode<LightTransition>> trueNodeFactory, Func<ILightPipelineContext, IPipelineNode<LightTransition>> falseNodeFactory) where TObservable : IObservable<bool>
    {
        var observable = serviceProvider.CreateInstanceWithinContext<TObservable>(LightEntity);
        return Switch(observable, trueNodeFactory, falseNodeFactory);
    }

    public ILightTransitionPipelineConfigurator Switch(IObservable<bool> observable, Func<ILightPipelineContext, IPipelineNode<LightTransition>> trueNodeFactory,
        Func<ILightPipelineContext, IPipelineNode<LightTransition>> falseNodeFactory)
    {
        return AddReactiveNode(c => c
            .On(observable.Where(x => x), trueNodeFactory)
            .On(observable.Where(x => !x), falseNodeFactory));
    }

    public ILightTransitionPipelineConfigurator Switch<TObservable, TTrueNode, TFalseNode>() where TObservable : IObservable<bool> where TTrueNode : IPipelineNode<LightTransition> where TFalseNode : IPipelineNode<LightTransition>
    {
        var observable = serviceProvider.CreateInstanceWithinContext<TObservable>(LightEntity);
        return Switch<TTrueNode, TFalseNode>(observable);
    }

    public ILightTransitionPipelineConfigurator Switch<TTrueNode, TFalseNode>(IObservable<bool> observable) where TTrueNode : IPipelineNode<LightTransition> where TFalseNode : IPipelineNode<LightTransition>
    {
        return AddReactiveNode(c => c
            .On<bool, TTrueNode>(observable.Where(x => x))
            .On<bool, TFalseNode>(observable.Where(x => !x)));
    }

    public ILightTransitionPipelineConfigurator AddReactiveNodeSwitch<TObservable>(Action<ILightTransitionReactiveNodeConfigurator> trueConfigure, Action<ILightTransitionReactiveNodeConfigurator> falseConfigure) where TObservable : IObservable<bool>
    {
        var observable = serviceProvider.CreateInstanceWithinContext<TObservable>(LightEntity);
        return AddReactiveNodeSwitch(observable, trueConfigure, falseConfigure);
    }

    public ILightTransitionPipelineConfigurator AddReactiveNodeSwitch(IObservable<bool> observable, Action<ILightTransitionReactiveNodeConfigurator> trueConfigure,
        Action<ILightTransitionReactiveNodeConfigurator> falseConfigure)
    {
        return AddReactiveNode(c => c
            .On(observable.Where(x => x), trueConfigure)
            .On(observable.Where(x => !x), falseConfigure));
    }

    public ILightTransitionPipelineConfigurator AddPipelineSwitch<TObservable>(Action<ILightTransitionPipelineConfigurator> trueConfigure, Action<ILightTransitionPipelineConfigurator> falseConfigure) where TObservable : IObservable<bool>
    {
        return Switch<TObservable>(c => lightPipelineFactory.CreateLightPipeline(c.LightEntity, trueConfigure), c => lightPipelineFactory.CreateLightPipeline(c.LightEntity, falseConfigure));
    }

    public ILightTransitionPipelineConfigurator AddPipelineSwitch(IObservable<bool> observable, Action<ILightTransitionPipelineConfigurator> trueConfigure,
        Action<ILightTransitionPipelineConfigurator> falseConfigure)
    {
        return Switch(observable, c => lightPipelineFactory.CreateLightPipeline(c.LightEntity, trueConfigure), c => lightPipelineFactory.CreateLightPipeline(c.LightEntity, falseConfigure));
    }

    public ILightTransitionPipelineConfigurator TurnOnOff<TObservable>() where TObservable : IObservable<bool>
    {
        return Switch<TObservable>(LightTransition.On(), LightTransition.Off());
    }

    public ILightTransitionPipelineConfigurator TurnOnOff(IObservable<bool> observable)
    {
        return Switch(observable, LightTransition.On(), LightTransition.Off());
    }
}
