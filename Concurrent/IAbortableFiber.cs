using System;
using System.Threading.Tasks;

namespace Concurrent
{
    public interface IAbortableFiber : IStopableFiber
    {
        Task Abort(Func<Task> cleanup);
    }
}
