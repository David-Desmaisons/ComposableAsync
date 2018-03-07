using System;
using System.Threading.Tasks;

namespace EasyActor.Queue
{
    public interface IAbortableTaskQueue : ITaskQueue
    {
        Task Abort(Func<Task> cleanup);
    }
}
