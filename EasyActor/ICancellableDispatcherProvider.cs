using Concurrent;

namespace EasyActor
{
    /// <summary>
    /// Returns the fiber associated with an actor
    /// </summary>
    public interface ICancellableDispatcherProvider
    {
        /// <summary>
        /// Returns the corresponding <see cref="ICancellableDispatcher"/>
        /// </summary>
        ICancellableDispatcher Dispatcher { get; }
    }
}
