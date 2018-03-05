using System;
using System.Threading.Tasks;

namespace EasyActor
{
    public interface ITaskQueue
    {
        Task Enqueue(Action action);

        Task<T> Enqueue<T>(Func<T> action);

        Task Enqueue(Func<Task> action);

        Task<T> Enqueue<T>(Func<Task<T>> action);

        TaskScheduler TaskScheduler { get; }
    }
}
