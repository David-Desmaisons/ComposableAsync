using System;
using System.Threading.Tasks;

namespace EasyActor.Fiber
{
    public interface IAbortableFiber : IStopableFiber
    {
        Task Abort(Func<Task> cleanup);
    }
}
