using System;

namespace EasyActor.Fiber
{
    public interface IMonoThreadFiber : IFiber, IDisposable
    {
        void Send(Action action);
    }
}
