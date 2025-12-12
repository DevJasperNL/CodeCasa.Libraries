using AutomationPipelines;

namespace CodeCasa.AutomationPipelines.Lights.Nodes;

public class PassThroughNode<TState> : PipelineNode<TState>
{
    public PassThroughNode()
    {
        PassThrough = true;
    }
}