using System;

namespace Concurrent
{
    public interface IMonoThreadFiber : IAbortableFiber, IDisposable
    {
        void Send(Action action);
    }
}
