using AutomationPipelines;

namespace CodeCasa.AutomationPipelines.Lights.Nodes;

public class FactoryNode<TState>(Func<TState?, TState?> lightTransitionFactory)
    : PipelineNode<TState>
{
    protected override void InputReceived(TState? input)
    {
        Output = lightTransitionFactory(input);
    }
}