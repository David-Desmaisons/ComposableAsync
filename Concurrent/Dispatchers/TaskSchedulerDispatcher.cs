using System;
using System.Threading;
using System.Threading.Tasks;
using Concurrent.Tasks;
using Concurrent.WorkItems;

namespace Concurrent.Dispatchers
{
    /// <summary>
    /// Dispatcher using a <see cref="TaskScheduler"/>
    /// </summary>
    internal class TaskSchedulerDispatcher : ICancellableDispatcher
    {
        internal TaskScheduler TaskScheduler { get; }

        private readonly TaskFactory _TaskFactory;

        public TaskSchedulerDispatcher(TaskScheduler taskScheduler)
        {
            TaskScheduler = taskScheduler;
            _TaskFactory = new TaskFactory(taskScheduler);
        }

        private static Task<T> Safe<T>(Func<Task<T>> compute)
        {
            try
            {
                return compute();
            }
            catch
            {
                return TaskBuilder<T>.Cancelled;
            }
        }

        private static Task Safe(Func<Task> compute)
        {
            try
            {
                return compute();
            }
            catch
            {
                return TaskBuilder.Cancelled;
            }
        }

        public void Dispatch(Action action)
        {
            Safe(() => _TaskFactory.StartNew(action));
        }

        public Task Enqueue(Action action)
        {
            return Safe(() => _TaskFactory.StartNew(action));
        }

        public Task<T> Enqueue<T>(Func<T> action)
        {
            return Safe(() => _TaskFactory.StartNew(action));
        }

        private Task Enqueue(AsyncActionWorkItem workItem)
        {
            return Safe(() =>
            {
                _TaskFactory.StartNew(workItem.Do);
                return workItem.Task;
            });
        }

        private Task<T> Enqueue<T>(AsyncWorkItem<T> workItem)
        {
            return Safe(() =>
            {
                _TaskFactory.StartNew(workItem.Do);
                return workItem.Task;
            });
        }

        private Task Enqueue(AsyncActionWorkItem workItem, CancellationToken cancellationToken)
        {
            return Safe(() =>
            {
                _TaskFactory.StartNew(workItem.Do, cancellationToken);
                return workItem.Task;
            });
        }

        private Task<T> Enqueue<T>(AsyncWorkItem<T> workItem, CancellationToken cancellationToken)
        {
            return Safe(() =>
            {
                _TaskFactory.StartNew(workItem.Do, cancellationToken);
                return workItem.Task;
            });
        }

        public Task Enqueue(Func<Task> action)
        {
            return Enqueue(new AsyncActionWorkItem(action));
        }

        public Task<T> Enqueue<T>(Func<Task<T>> action)
        {
            return Enqueue(new AsyncWorkItem<T>(action));
        }

        public Task Enqueue(Func<Task> action, CancellationToken cancellationToken)
        {
            return Enqueue(new AsyncActionWorkItem(action, cancellationToken), cancellationToken);
        }

        public Task<T> Enqueue<T>(Func<Task<T>> action, CancellationToken cancellationToken)
        {
            return Enqueue(new AsyncWorkItem<T>(action, cancellationToken), cancellationToken);
        }
    }
}
