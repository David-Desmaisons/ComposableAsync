using System;
using System.Threading.Tasks;

namespace EasyActor.Fiber
{
    public interface IStopableFiber : IFiber
    {
        Task Stop(Func<Task> cleanup);
    }
}
