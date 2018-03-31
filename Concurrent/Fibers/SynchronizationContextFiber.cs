using System;
using System.Threading;
using System.Threading.Tasks;
using Concurrent.WorkItems;

namespace Concurrent.Fibers
{
    internal sealed class SynchronizationContextFiber : IFiber, ICancellableDispatcher
    {
        public bool IsAlive => true;
        public SynchronizationContext SynchronizationContext => _Context;

        private readonly SynchronizationContext _Context;
        public SynchronizationContextFiber(SynchronizationContext synchronizationContext)
        {
            _Context = synchronizationContext;
        }

        public Task<T> Enqueue<T>(Func<T> action)
        {
            var workitem = new WorkItem<T>(action);
            _Context.Post(_SendOrPostWorkItem, workitem);
            return workitem.Task;
        }

        public Task Enqueue(Func<Task> action)
        {
            var workitem = new AsyncActionWorkItem(action);
            _Context.Post(_SendOrPostWorkItem, workitem);
            return workitem.Task;
        }

        public Task<T> Enqueue<T>(Func<Task<T>> action)
        {
            var workitem = new AsyncWorkItem<T>(action);
            _Context.Post(_SendOrPostWorkItem, workitem);
            return workitem.Task;
        }

        public Task Enqueue(Func<Task> action, CancellationToken cancellationToken)
        {
            var workitem = new AsyncActionWorkItem(action, cancellationToken);
            _Context.Post(_SendOrPostWorkItem, workitem);
            return workitem.Task;
        }

        public Task<T> Enqueue<T>(Func<Task<T>> action, CancellationToken cancellationToken)
        {
            var workitem = new AsyncWorkItem<T>(action, cancellationToken);
            _Context.Post(_SendOrPostWorkItem, workitem);
            return workitem.Task;
        }

        public void Dispatch(Action action)
        {
            _Context.Post(_SendOrPostAction, action);
        }

        public Task Enqueue(Action action) 
        {
            var workitem = new ActionWorkItem(action);
            _Context.Post(_SendOrPostWorkItem, workitem);
            return workitem.Task;
        }

        private static readonly SendOrPostCallback _SendOrPostWorkItem = DoWorkItem;
        private static readonly SendOrPostCallback _SendOrPostAction = RunAction;

        private static void DoWorkItem(object state) => ((IWorkItem) state).Do();
        private static void RunAction(object state) => ((Action) state)();
    }
}
