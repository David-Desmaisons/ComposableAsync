using System.Threading;

namespace Concurrent
{
    public interface IFiber : IDispatcher
    {
        SynchronizationContext SynchronizationContext { get; }
    }
}
