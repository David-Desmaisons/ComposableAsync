using System;
using System.Threading;
using System.Threading.Tasks;

namespace EasyActor
{
    public interface IStopableTaskQueue : ITaskQueue
    {
        void Stop();

        Task SetCleanUp(Func<Task> cleanup);
    }
}
