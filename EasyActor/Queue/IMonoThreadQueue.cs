using System;

namespace EasyActor.Queue
{
    public interface IMonoThreadQueue : ITaskQueue, IDisposable
    {
        void Send(Action action);
    }
}
