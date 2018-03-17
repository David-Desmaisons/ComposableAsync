using System;
using System.Threading.Tasks;

namespace Concurrent
{
    public interface IStopableFiber : IFiber
    {
        Task Stop(Func<Task> cleanup);
    }
}
