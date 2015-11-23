using System;
using System.Threading;
using System.Threading.Tasks;

namespace EasyActor
{
    public interface ITaskQueue
    {
        Task<T> Enqueue<T>(Func<T> action);

        Task Enqueue(Func<Task> action);

        Task<T> Enqueue<T>(Func<Task<T>> action);

        SynchronizationContext SynchronizationContext { get; }

        TaskScheduler TaskScheduler { get; }
    }
}
