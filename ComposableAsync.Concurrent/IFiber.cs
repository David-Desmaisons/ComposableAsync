using System.Threading;

namespace ComposableAsync.Concurrent
{
    /// <summary>
    /// Fiber abstraction
    /// </summary>
    public interface IFiber : IDispatcher
    {
        /// <summary>
        /// True if the fiber is active
        /// </summary>
        bool IsAlive { get; }

        /// <summary>
        /// Corresponding synchronization context
        /// </summary>
        SynchronizationContext SynchronizationContext { get; }
    }
}
