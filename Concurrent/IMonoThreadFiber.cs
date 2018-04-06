using System;
using System.Threading;

namespace Concurrent
{
    public interface IMonoThreadFiber : IStopableFiber
    {
        Thread Thread { get; }

        void Send(Action action);
    }
}
