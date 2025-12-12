namespace CodeCasa.Abstractions
{
    /// <summary>
    /// Provides an interface for dimmer devices that can dim or brighten light intensity.
    /// </summary>
    public interface IDimmer
    {
        /// <summary>
        /// Gets an observable that emits <c>true</c> when the dimmer is actively dimming, and <c>false</c> otherwise.
        /// </summary>
        public IObservable<bool> Dimming { get; }

        /// <summary>
        /// Gets an observable that emits <c>true</c> when the dimmer is actively brightening, and <c>false</c> otherwise.
        /// </summary>
        public IObservable<bool> Brightening { get; }
    }
}
