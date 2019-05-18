using System;
using System.Threading;
using System.Threading.Tasks;
using ComposableAsync.Concurrent.WorkItems;

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
            var workItem = new WorkItem<T>(action);
            SynchronizationContext.Post(_SendOrPostWorkItem, workItem);
            return workItem.Task;
        }

        public Task Enqueue(Func<Task> action)
        {
            var workItem = new AsyncActionWorkItem(action);
            SynchronizationContext.Post(_SendOrPostWorkItem, workItem);
            return workItem.Task;
        }

        public Task<T> Enqueue<T>(Func<Task<T>> action)
        {
            var workItem = new AsyncWorkItem<T>(action);
            SynchronizationContext.Post(_SendOrPostWorkItem, workItem);
            return workItem.Task;
        }

        public Task Enqueue(Func<Task> action, CancellationToken cancellationToken)
        {
            var workItem = new AsyncActionWorkItem(action, cancellationToken);
            SynchronizationContext.Post(_SendOrPostWorkItem, workItem);
            return workItem.Task;
        }

        public Task<T> Enqueue<T>(Func<Task<T>> action, CancellationToken cancellationToken)
        {
            var workItem = new AsyncWorkItem<T>(action, cancellationToken);
            SynchronizationContext.Post(_SendOrPostWorkItem, workItem);
            return workItem.Task;
        }

        public void Dispatch(Action action)
        {
            SynchronizationContext.Post(_SendOrPostAction, action);
        }

        public Task Enqueue(Action action) 
        {
            var workItem = new ActionWorkItem(action);
            SynchronizationContext.Post(_SendOrPostWorkItem, workItem);
            return workItem.Task;
        }

        private static readonly SendOrPostCallback _SendOrPostWorkItem = DoWorkItem;
        private static readonly SendOrPostCallback _SendOrPostAction = RunAction;

        private static void DoWorkItem(object state) => ((IWorkItem) state).Do();
        private static void RunAction(object state) => ((Action) state)();
    }
}
