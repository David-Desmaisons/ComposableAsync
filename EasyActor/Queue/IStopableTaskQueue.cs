using System;
using System.Threading.Tasks;

namespace EasyActor
{
    public interface IStopableTaskQueue : ITaskQueue
    {
        Task Stop(Func<Task> cleanup);
    }
}
