using System;
using System.Threading;

namespace Concurrent
{
    /// <summary>
    /// Mono threaded fiber
    /// </summary>
    public interface IMonoThreadFiber : IStopableFiber
    {
        /// <summary>
        /// Thread Fiber
        /// </summary>
        Thread Thread { get; }

        /// <summary>
        /// Dispatch an action on the fiber
        /// </summary>
        /// <param name="action"></param>
        void Send(Action action);
    }
}
