using System;
using System.Threading.Tasks;

namespace EasyActor.Queue
{
    public interface IStopableTaskQueue : ITaskQueue
    {
        Task Stop(Func<Task> cleanup);
    }
}
