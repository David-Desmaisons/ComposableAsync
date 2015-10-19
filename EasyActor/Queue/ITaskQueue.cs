using System;
using System.Threading;
using System.Threading.Tasks;

namespace EasyActor
{
    public interface ITaskQueue
    {
        Task Enqueue(Func<Task> action);

        Task<T> Enqueue<T>(Func<Task<T>> action);

        SynchronizationContext SynchronizationContext { get; }
    }
}
