using System;

namespace EasyActor.Fiber
{
    public interface IMonoThreadFiber : IAbortableFiber, IDisposable
    {
        int EnqueuedTasksNumber { get; }

        void Send(Action action);
    }
}
