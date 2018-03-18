using System.Threading;

namespace Concurrent
{
    public interface IFiber : IDispatcher
    {
        bool IsAlive { get; }

        SynchronizationContext SynchronizationContext { get; }
    }
}
