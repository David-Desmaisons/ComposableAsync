using System;

namespace Concurrent
{
    public interface IMonoThreadFiber : IStopableFiber, ICancellableDispatcher
    {
        void Send(Action action);
    }
}
