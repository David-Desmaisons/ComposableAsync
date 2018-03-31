using System;

namespace Concurrent
{
    public interface IStopableFiber : IFiber, ICancellableDispatcher, IAsyncDisposable
    {
    }
}
