using ComposableAsync.Concurrent.WorkItems;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace ComposableAsync.Concurrent.Fibers
{
    internal sealed class SynchronizationContextFiber : IFiber
    {
        public bool IsAlive => true;
        public SynchronizationContext SynchronizationContext { get; }

        public SynchronizationContextFiber(SynchronizationContext synchronizationContext)
        {
            SynchronizationContext = synchronizationContext;
        }

        public Task<T> Enqueue<T>(Func<T> action)
        {
            return PrivateEnqueue(new WorkItem<T>(action));
        }

        public Task<T> Enqueue<T>(Func<T> action, CancellationToken cancellationToken)
        {
            return PrivateEnqueue(new CancellableWorkItem<T>(action, cancellationToken));
        }

        public Task Enqueue(Action action, CancellationToken cancellationToken)
        {
            return PrivateEnqueue(new ActionCancellableWorkItem(action, cancellationToken));
        }

        public Task Enqueue(Func<Task> action)
        {
            return PrivateEnqueue(new AsyncActionWorkItem(action));
        }

        public Task<T> Enqueue<T>(Func<Task<T>> action)
        {
            return PrivateEnqueue(new AsyncWorkItem<T>(action));
        }

        public IDispatcher Clone() => this;

        public Task Enqueue(Func<Task> action, CancellationToken cancellationToken)
        {
            return PrivateEnqueue(new AsyncActionCancellableWorkItem(action, cancellationToken));
        }

        public Task<T> Enqueue<T>(Func<Task<T>> action, CancellationToken cancellationToken)
        {
            return PrivateEnqueue(new AsyncCancellableWorkItem<T>(action, cancellationToken));
        }

        public void Dispatch(Action action)
        {
            SynchronizationContext.Post(_SendOrPostAction, action);
        }

        public Task Enqueue(Action action)
        {
            return PrivateEnqueue(new ActionWorkItem(action));
        }

        private Task<T> PrivateEnqueue<T>(ITraceableWorkItem<T> workItem)
        {
            SynchronizationContext.Post(_SendOrPostWorkItem, workItem);
            return workItem.Task;
        }

        private static readonly SendOrPostCallback _SendOrPostWorkItem = DoWorkItem;
        private static readonly SendOrPostCallback _SendOrPostAction = RunAction;

        private static void DoWorkItem(object state) => ((IWorkItem)state).Do();
        private static void RunAction(object state) => ((Action)state)();
    }
}
