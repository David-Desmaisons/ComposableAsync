using System.Threading;

namespace Concurrent
{
    public interface IFiber : ICancellableDispatcher
    {
        bool IsAlive { get; }

        SynchronizationContext SynchronizationContext { get; }
    }
}
