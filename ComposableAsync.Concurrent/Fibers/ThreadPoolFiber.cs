using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using ComposableAsync.Concurrent.Collections;
using ComposableAsync.Concurrent.SynchronizationContexts;
using ComposableAsync.Concurrent.WorkItems;

namespace ComposableAsync.Concurrent.Fibers
{
    [DebuggerNonUserCode]
    internal sealed class ThreadPoolFiber : IMonoThreadFiber
    {
        public bool IsAlive => !_EndFiber.Task.IsCompleted;
        public SynchronizationContext SynchronizationContext =>
            _SynchronizationContext ?? (_SynchronizationContext = new MonoThreadedFiberSynchronizationContext(this));
        public Thread Thread { get; private set; }

        private SynchronizationContext _SynchronizationContext;
        private readonly IMpScQueue<IWorkItem> _TaskQueue;
        private readonly TaskCompletionSource<int> _EndFiber = new TaskCompletionSource<int>();

        public ThreadPoolFiber(IMpScQueue<IWorkItem> queue = null)
        {
            _TaskQueue = queue ?? new BlockingMpscQueue<IWorkItem>();
            ThreadPool.QueueUserWorkItem(_ => Consume());
        }

        public void Send(Action action)
        {
            if (Thread.CurrentThread == Thread)
            {
                action();
                return;
            }

            Enqueue(action).Wait();
        }

        private Task PrivateEnqueue(ITraceableWorkItem workItem)
        {
            try
            {
                _TaskQueue.Enqueue(workItem);
                return workItem.Task;
            }
            catch (OperationCanceledException operationCanceledException)
            {
                return Task.FromCanceled(operationCanceledException.CancellationToken);
            }
        }

        private Task<T> PrivateEnqueue<T>(ITraceableWorkItem<T> workItem)
        {
            try
            {
                _TaskQueue.Enqueue(workItem);
                return workItem.Task;
            }
            catch (OperationCanceledException operationCanceledException)
            {
                return Task.FromCanceled<T>(operationCanceledException.CancellationToken);
            }
        }   

        public Task<T> Enqueue<T>(Func<T> action)
        {
            return PrivateEnqueue(new WorkItem<T>(action));
        }

        public void Dispatch(Action action)
        {
            try
            {
                var workItem = new DispatchItem(action);
                _TaskQueue.Enqueue(workItem);
            }
            catch (Exception)
            {
                // ignored
            }
        }

        public Task Enqueue(Action action)
        {
            return PrivateEnqueue(new ActionWorkItem(action));
        }

        public Task Enqueue(Func<Task> action)
        {
            return PrivateEnqueue(new AsyncActionWorkItem(action));
        }

        public Task<T> Enqueue<T>(Func<Task<T>> action)
        {
            return PrivateEnqueue(new AsyncWorkItem<T>(action));
        }

        private void Consume() 
        {
            using (new SynchronizationContextSwapper(SynchronizationContext)) 
            {
                ConsumeInContext();
            }
        }

        private void ConsumeInContext()
        {
            Thread = Thread.CurrentThread;

            try
            {
                _TaskQueue.OnElements(action => action.Do());
            }
            catch (OperationCanceledException)
            {          
            }

            foreach (var action in _TaskQueue.GetUnsafeQueue())
            {
                action.Cancel();
            }

            _TaskQueue.Dispose();
            _EndFiber.TrySetResult(0);
        }

        private void StopQueueing()
        {
            _TaskQueue.CompleteAdding();
        }

        public Task DisposeAsync()
        {
            GC.SuppressFinalize(this);
            try
            {
                StopQueueing();
                return _EndFiber.Task;
            }
            catch
            {
                return Task.CompletedTask;
            }
        }

        ~ThreadPoolFiber()
        {
            StopQueueing();
        }

        public Task Enqueue(Func<Task> action, CancellationToken cancellationToken)
        {
            return PrivateEnqueue(new AsyncActionCancellableWorkItem(action, cancellationToken));
        }

        public Task<T> Enqueue<T>(Func<Task<T>> action, CancellationToken cancellationToken)
        {
            return PrivateEnqueue(new AsyncCancellableWorkItem<T>(action, cancellationToken));
        }

        public Task<T> Enqueue<T>(Func<T> action, CancellationToken cancellationToken)
        {
            return PrivateEnqueue(new CancellableWorkItem<T>(action, cancellationToken));
        }

        public Task Enqueue(Action action, CancellationToken cancellationToken)
        {
            return PrivateEnqueue(new ActionCancellableWorkItem(action, cancellationToken));
        }
    }
}
