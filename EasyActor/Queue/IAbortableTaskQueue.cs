using System;
using System.Threading.Tasks;

namespace EasyActor
{
    public interface IAbortableTaskQueue : IStopableTaskQueue, IDisposable
    {
        Task Abort(Func<Task> cleanup);
    }
}
