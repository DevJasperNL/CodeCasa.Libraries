using System.Reactive.Linq;
using CodeCasa.Abstractions;


namespace CodeCasa.AutomationPipelines.Lights.Nodes
{
    public class CompositeDimmer : IDimmer
    {
        public CompositeDimmer(IEnumerable<IDimmer> dimmers)
        {
            dimmers = dimmers.ToArray();
            if (dimmers == null || !dimmers.Any())
                throw new ArgumentException("At least one dimmer must be provided.", nameof(dimmers));

            Dimming = dimmers
                .Select(d => d.Dimming)
                .CombineLatest(x => x.Any())
                .DistinctUntilChanged();

            Brightening = dimmers
                .Select(d => d.Brightening)
                .CombineLatest(x => x.Any())
                .DistinctUntilChanged();
        }

        public IObservable<bool> Dimming { get; }

        public IObservable<bool> Brightening { get; }
    }
}
