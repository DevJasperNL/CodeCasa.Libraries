
namespace CodeCasa.AutomationPipelines.Lights
{
    public sealed class CompositeAsyncDisposable : IAsyncDisposable
    {
        private readonly List<IAsyncDisposable> _asyncDisposables = new();
        private readonly List<IDisposable> _disposables = new();
        private bool _disposed;

        public void Add(IAsyncDisposable asyncDisposable)
        {
            _asyncDisposables.Add(asyncDisposable);
        }

        public void Add(IDisposable disposable)
        {
            _disposables.Add(disposable);
        }

        public void AddRange(IEnumerable<IDisposable> disposables)
        {
            _disposables.AddRange(disposables);
        }

        public async ValueTask DisposeAsync()
        {
            if (_disposed)
            {
                return;
            }
            _disposed = true;

            foreach (var d in _asyncDisposables)
            {
                await d.DisposeAsync();
            }
            foreach (var d in _disposables)
            {
                d.Dispose();
            }
        }
    }
}
