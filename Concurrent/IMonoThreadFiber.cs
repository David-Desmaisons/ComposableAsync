using System;

namespace Concurrent
{
    public interface IMonoThreadFiber : IStopableFiber
    {
        void Send(Action action);
    }
}
