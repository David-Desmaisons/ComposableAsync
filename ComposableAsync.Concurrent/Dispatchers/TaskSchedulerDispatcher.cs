using System;
using System.Threading;
using System.Threading.Tasks;
using ComposableAsync.Concurrent.WorkItems;

namespace ComposableAsync.Concurrent.Dispatchers
{
    /// <summary>
    /// Dispatcher using a <see cref="TaskScheduler"/>
    /// </summary>
    internal class TaskSchedulerDispatcher : IDispatcher
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
            catch (TaskSchedulerException)
            {
                return Task.FromCanceled<T>(new CancellationToken(true));
            }
        }

        private static Task Safe(Func<Task> compute)
        {
            try
            {
                return compute();
            }
            catch (TaskSchedulerException)
            {
                return Task.FromCanceled(new CancellationToken(true));
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

        private Task Enqueue(ITraceableWorkItem workItem)
        {
            return Safe(() =>
            {
                _TaskFactory.StartNew(workItem.Do);
                return workItem.Task;
            });
        }

        private Task<T> Enqueue<T>(ITraceableWorkItem<T> workItem)
        {
            return Safe(() =>
            {
                _TaskFactory.StartNew(workItem.Do);
                return workItem.Task;
            });
        }

        private Task Enqueue(ITraceableWorkItem workItem, CancellationToken cancellationToken)
        {
            return Safe(() =>
            {
                _TaskFactory.StartNew(workItem.Do, cancellationToken);
                return workItem.Task;
            });
        }

        private Task<T> Enqueue<T>(ITraceableWorkItem<T> workItem, CancellationToken cancellationToken)
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

        public IDispatcher Clone() => this;

        public Task Enqueue(Func<Task> action, CancellationToken cancellationToken)
        {
            return Enqueue(new AsyncActionCancellableWorkItem(action, cancellationToken), cancellationToken);
        }

        public Task<T> Enqueue<T>(Func<Task<T>> action, CancellationToken cancellationToken)
        {
            return Enqueue(new AsyncCancellableWorkItem<T>(action, cancellationToken), cancellationToken);
        }

        public Task<T> Enqueue<T>(Func<T> action, CancellationToken cancellationToken)
        {
            return Enqueue(new CancellableWorkItem<T>(action, cancellationToken), cancellationToken);
        }

        public Task Enqueue(Action action, CancellationToken cancellationToken)
        {
            return Enqueue(new ActionCancellableWorkItem(action, cancellationToken), cancellationToken);
        }
    }
}
