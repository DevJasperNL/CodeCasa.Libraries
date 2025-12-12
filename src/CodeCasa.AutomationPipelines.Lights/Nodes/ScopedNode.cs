
using AutomationPipelines;
using Microsoft.Extensions.DependencyInjection;

namespace CodeCasa.AutomationPipelines.Lights.Nodes
{
    internal class ScopedNode<TState>(IServiceScope serviceScope, IPipelineNode<TState> innerNode)
        : IPipelineNode<TState>, IAsyncDisposable
    {
        public void Dispose()
        {
            serviceScope.Dispose();
        }

        public async ValueTask DisposeAsync()
        {
            // todo: make this a helper method
            switch (serviceScope)
            {
                case IAsyncDisposable asyncDisposable:
                    await asyncDisposable.DisposeAsync();
                    break;
                case IDisposable disposable:
                    disposable.Dispose();
                    break;
            }

            switch (innerNode)
            {
                case IAsyncDisposable asyncDisposable:
                    await asyncDisposable.DisposeAsync();
                    break;
                case IDisposable disposable:
                    disposable.Dispose();
                    break;
            }
        }

        public TState? Input
        {
            get => innerNode.Input;
            set => innerNode.Input = value;
        }

        public TState? Output => innerNode.Output;
        public IObservable<TState?> OnNewOutput => innerNode.OnNewOutput;

        public override string? ToString() => $"{innerNode} (scoped)";
    }
}
