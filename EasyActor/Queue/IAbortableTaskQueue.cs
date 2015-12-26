using System;
using System.Threading;
using System.Threading.Tasks;

namespace EasyActor
{
    public interface IAbortableTaskQueue : IStopableTaskQueue
    {
        Task Abort(Func<Task> cleanup);
    }
}
