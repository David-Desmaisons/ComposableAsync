using System;
using System.Threading;

namespace Concurrent
{
    /// <summary>
    /// Mono threaded Fiber
    /// </summary>
    public interface IMonoThreadFiber : IStopableFiber
    {
        Thread Thread { get; }

        void Send(Action action);
    }
}
