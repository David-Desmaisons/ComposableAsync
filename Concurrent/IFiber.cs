using System.Threading;

namespace Concurrent
{
    /// <summary>
    /// Fiber abstraction
    /// </summary>
    public interface IFiber : ICancellableDispatcher
    {
        bool IsAlive { get; }

        SynchronizationContext SynchronizationContext { get; }
    }
}
