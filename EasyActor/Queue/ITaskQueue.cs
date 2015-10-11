using System;
using System.Threading.Tasks;

namespace EasyActor
{
    internal interface ITaskQueue
    {
        Task Enqueue(Func<Task> action);

        Task<T> Enqueue<T>(Func<Task<T>> action);
    }
}
