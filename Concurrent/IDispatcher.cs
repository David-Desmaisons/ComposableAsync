using System;
using System.Threading.Tasks;

namespace Concurrent
{
    public interface IDispatcher
    {
        void Dispatch(Action action);

        Task Enqueue(Action action);

        Task<T> Enqueue<T>(Func<T> action);

        Task Enqueue(Func<Task> action);

        Task<T> Enqueue<T>(Func<Task<T>> action);
    }
}
