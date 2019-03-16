using System;
using System.Threading;
using System.Threading.Tasks;

namespace Concurrent
{
    /// <summary>
    /// Cancellable Fiber
    /// </summary>
    public interface ICancellableDispatcher : IDispatcher
    {
        /// <summary>
        /// Enqueue the task and return a task corresponding
        /// to the execution of the task
        /// </summary>
        /// <param name="action"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        Task Enqueue(Func<Task> action, CancellationToken cancellationToken);

        /// <summary>
        /// Enqueue the task and return a task corresponding
        /// to the execution of the original task
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="action"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        Task<T> Enqueue<T>(Func<Task<T>> action, CancellationToken cancellationToken);
    }
}
