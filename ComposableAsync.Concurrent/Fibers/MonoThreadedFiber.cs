using ComposableAsync.Concurrent.Collections;
using ComposableAsync.Concurrent.SynchronizationContexts;
using ComposableAsync.Concurrent.WorkItems;
using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace ComposableAsync.Concurrent.Fibers
{
    [DebuggerNonUserCode]
    internal sealed class MonoThreadedFiber : IMonoThreadFiber
    {
        private static int _Count = 0;

        public SynchronizationContext SynchronizationContext =>
            _SynchronizationContext ?? (_SynchronizationContext = new MonoThreadedFiberSynchronizationContext(this));
        public bool IsAlive => Thread.IsAlive;
        public Thread Thread { get; }

        private SynchronizationContext _SynchronizationContext;
        private readonly IMpScQueue<IWorkItem> _TaskQueue;
        private readonly Action<Thread> _OnCreate;
        private readonly TaskCompletionSource<int> _EndFiber = new TaskCompletionSource<int>();

        public MonoThreadedFiber(Action<Thread> onCreate = null, IMpScQueue<IWorkItem> queue = null)
        {
            _OnCreate = onCreate;
            _TaskQueue = queue ?? new BlockingMpscQueue<IWorkItem>();
            Thread = new Thread(Consume)
            {
                IsBackground = true,
                Name = $"MonoThreadedQueue-{_Count++}"
            };

            _OnCreate?.Invoke(Thread);
            Thread.Start();
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

        private Task<T> Enqueue<T>(ITraceableWorkItem<T> workItem)
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
            return Enqueue<T>(new WorkItem<T>(action));
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

        public IDispatcher Clone() => new MonoThreadedFiber(_OnCreate);

        private void Consume()
        {
            SynchronizationContext.SetSynchronizationContext(this.SynchronizationContext);

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

        public Task Enqueue(Func<Task> action, CancellationToken cancellationToken)
        {
            return Enqueue(new AsyncActionCancellableWorkItem(action, cancellationToken));
        }

        public Task<T> Enqueue<T>(Func<Task<T>> action, CancellationToken cancellationToken)
        {
            return Enqueue(new AsyncCancellableWorkItem<T>(action, cancellationToken));
        }

        public Task<T> Enqueue<T>(Func<T> action, CancellationToken cancellationToken)
        {
            return Enqueue(new CancellableWorkItem<T>(action, cancellationToken));
        }

        public Task Enqueue(Action action, CancellationToken cancellationToken)
        {
            return Enqueue(new ActionCancellableWorkItem(action, cancellationToken));
        }

        ~MonoThreadedFiber()
        {
            StopQueueing();
        }
    }
}
