using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Concurrent.Collections;
using Concurrent.SynchronizationContexts;
using Concurrent.Tasks;using Concurrent.WorkItems;

namespace Concurrent.Fibers
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

        private Task Enqueue(ActionWorkItem workItem)
        {
            try
            {
                _TaskQueue.Enqueue(workItem);
                return workItem.Task;
            }
            catch (Exception)
            {
                return TaskBuilder.Cancelled;
            }
        }

        private Task<T> Enqueue<T>(AsyncWorkItem<T> workItem)
        {
            try
            {
                _TaskQueue.Enqueue(workItem);
                return workItem.Task;
            }
            catch (Exception)
            {
                return TaskBuilder<T>.Cancelled;
            }
        }

        public Task<T> Enqueue<T>(Func<T> action)
        {
            var workItem = new WorkItem<T>(action);
            try
            {
                _TaskQueue.Enqueue(workItem);
                return workItem.Task;
            }
            catch (Exception)
            {
                return TaskBuilder<T>.Cancelled;
            }
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
            return Enqueue(new ActionWorkItem(action));
        }

        public Task Enqueue(Func<Task> action)
        {
            return Enqueue(new AsyncActionWorkItem(action));
        }

        public Task<T> Enqueue<T>(Func<Task<T>> action)
        {
            return Enqueue(new AsyncWorkItem<T>(action));
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
            return Enqueue(new AsyncActionWorkItem(action, cancellationToken));
        }

        public Task<T> Enqueue<T>(Func<Task<T>> action, CancellationToken cancellationToken)
        {
            return Enqueue(new AsyncWorkItem<T>(action, cancellationToken));
        }
    }
}
