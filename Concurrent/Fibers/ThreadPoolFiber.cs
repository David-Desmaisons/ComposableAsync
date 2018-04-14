using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Concurrent.SynchronizationContexts;
using Concurrent.Tasks;
using Concurrent.WorkItems;

namespace Concurrent.Fibers
{
    [DebuggerNonUserCode]
    internal sealed class ThreadPoolFiber : IMonoThreadFiber
    {
        public bool IsAlive => !_EndFiber.Task.IsCompleted;
        public SynchronizationContext SynchronizationContext =>
            _SynchronizationContext ?? (_SynchronizationContext = new MonoThreadedFiberSynchronizationContext(this));
        public Thread Thread => _Current;

        private SynchronizationContext _SynchronizationContext;
        private readonly BlockingCollection<IWorkItem> _TaskQueue = new BlockingCollection<IWorkItem>();
        private readonly CancellationTokenSource _Cts = new CancellationTokenSource();
        private Thread _Current;
        private readonly TaskCompletionSource<int> _EndFiber = new TaskCompletionSource<int>();

        public ThreadPoolFiber()
        {
            ThreadPool.QueueUserWorkItem(_ => Consume());
        }

        public void Send(Action action)
        {
            if (Thread.CurrentThread == _Current)
            {
                action();
                return;
            }

            Enqueue(action).Wait();
        }

        private Task Enqueue(ActionWorkItem workitem)
        {
            try
            {
                _TaskQueue.Add(workitem);
                return workitem.Task;
            }
            catch (Exception)
            {
                return TaskBuilder.Cancelled;
            }
        }

        private Task<T> Enqueue<T>(AsyncWorkItem<T> workitem)
        {
            try
            {
                _TaskQueue.Add(workitem);
                return workitem.Task;
            }
            catch (Exception)
            {
                return TaskBuilder<T>.Cancelled;
            }
        }

        public Task<T> Enqueue<T>(Func<T> action)
        {
            var workitem = new WorkItem<T>(action);
            try
            {
                _TaskQueue.Add(workitem);
                return workitem.Task;
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
                var workitem = new DispatchItem(action);
                _TaskQueue.Add(workitem);
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
            _Current = Thread.CurrentThread;
            try
            {
                foreach (var action in _TaskQueue.GetConsumingEnumerable(_Cts.Token))
                {
                    action.Do();
                }
            }
            catch (OperationCanceledException)
            {
                _TaskQueue.CompleteAdding();
                foreach (var action in _TaskQueue.GetConsumingEnumerable())
                {
                    action.Cancel();
                }
            }
            _TaskQueue.Dispose();
            _EndFiber.TrySetResult(0);
        }

        private void StopQueueing()
        {
            _Cts.Cancel();
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
                return TaskBuilder.Completed;
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
