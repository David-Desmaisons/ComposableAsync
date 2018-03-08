using System;
using System.Threading.Tasks;

namespace EasyActor.Fiber
{
    public interface IAbortableFiber : IFiber
    {
        Task Abort(Func<Task> cleanup);
    }
}
