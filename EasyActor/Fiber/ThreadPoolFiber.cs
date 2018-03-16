using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;
using EasyActor.Fiber.Scheduler;
using EasyActor.Fiber.WorkItems;
using EasyActor.TaskHelper;

namespace EasyActor.Fiber
{
    public sealed class ThreadPoolFiber : IMonoThreadFiber
    {
        private readonly BlockingCollection<IWorkItem> _TaskQueue = new BlockingCollection<IWorkItem>();
        private readonly CancellationTokenSource _Cts;
        private Thread _Current;
        private AsyncActionWorkItem _Clean;
        private bool _Running = false;

        public ThreadPoolFiber()
        {
            _Cts = new CancellationTokenSource();
            ThreadPool.QueueUserWorkItem(_ => Consume());
        }

        public int EnqueuedTasksNumber => _TaskQueue.Count + (_Running ? 1 : 0);

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

        private void StopQueueing()
        {
            try
            {
                _Cts.Cancel();
                _TaskQueue.CompleteAdding();
            }
            catch (ObjectDisposedException)
            {
            }
        }

        public Task Stop(Func<Task> cleanup)
        {
            _Clean = new AsyncActionWorkItem(cleanup);
            _TaskQueue.CompleteAdding();
            return _Clean.Task;
        }

        public Task Abort(Func<Task> cleanup)
        {
            _Clean = new AsyncActionWorkItem(cleanup);
            StopQueueing();
            return _Clean.Task;
        }

        private void Consume()
        {
            _Current = Thread.CurrentThread;
            var currentContext = SynchronizationContext.Current;
            SynchronizationContext.SetSynchronizationContext(this.SynchronizationContext);

            try
            {
                foreach (var action in _TaskQueue.GetConsumingEnumerable(_Cts.Token))
                {
                    _Running = true;
                    action.Do();
                    _Running = false;
                }
            }
            catch (OperationCanceledException)
            {
                _TaskQueue.CompleteAdding();
                foreach (var action in _TaskQueue.GetConsumingEnumerable())
                {
                    _Running = true;
                    action.Cancel();
                    _Running = false;
                }
            }
            _TaskQueue.Dispose();
            _Clean?.Do();
            SynchronizationContext.SetSynchronizationContext(currentContext);
        }

        private SynchronizationContext _SynchronizationContext;
        private SynchronizationContext SynchronizationContext =>
            _SynchronizationContext ?? (_SynchronizationContext = new MonoThreadedFiberSynchronizationContext(this));

        public TaskScheduler TaskScheduler => new SynchronizationContextTaskScheduler(SynchronizationContext);

        public void Dispose()
        {
            StopQueueing();
        }
    }
}
