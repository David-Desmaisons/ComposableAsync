using System;
using System.Threading;
using System.Threading.Tasks;
using EasyActor.TaskHelper;
using Retlang.Core;

namespace EasyActor.Queue
{
    public class MonoThreadedRetlangQueue : IMonoThreadQueue, IAbortableTaskQueue
    {
        private static int _Count = 0;
        private readonly IQueue _TaskQueue;
        private readonly Thread _Current;
        private AsyncActionWorkItem _Clean;
        private bool _Running = true;

        public MonoThreadedRetlangQueue(IQueue queue, Action<Thread> onCreate = null)
        {
            _TaskQueue = queue;

            _Current = new Thread(Consume)
            {
                IsBackground = true,
                Name = $"MonoThreadedQueue-{_Count++}"
            };

            onCreate?.Invoke(_Current);
            _Current.Start();
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
            if (!_Running)
                return TaskBuilder.Cancelled;

            try
            {
                _TaskQueue.Enqueue(workitem.Do);
                return workitem.Task;
            }
            catch (Exception)
            {
                return TaskBuilder.Cancelled;
            }
        }

        private Task<T> Enqueue<T>(AsyncWorkItem<T> workitem)
        {
            if (!_Running)
                return TaskBuilder<T>.Cancelled;

            try
            {
                _TaskQueue.Enqueue(workitem.Do);
                return workitem.Task;
            }
            catch (Exception)
            {
                return TaskBuilder<T>.Cancelled;
            }
        }

        public Task<T> Enqueue<T>(Func<T> action)
        {
            if (!_Running)
                return TaskBuilder<T>.Cancelled;

            try
            {
                var workitem = new WorkItem<T>(action);
                _TaskQueue.Enqueue(workitem.Do);
                return workitem.Task;
            }
            catch (Exception)
            {
                return TaskBuilder<T>.Cancelled;
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
            _TaskQueue.Stop();
            _Running = false;
        }

        public Task Abort(Func<Task> cleanup)
        {
            _Clean = new AsyncActionWorkItem(cleanup);
            StopQueueing();
            return _Clean.Task;
        }

        private void Consume()
        {
            SynchronizationContext.SetSynchronizationContext(this.SynchronizationContext);

            _TaskQueue.Run();
            _Clean?.Do();
        }

        private SynchronizationContext _SynchronizationContext;
        private SynchronizationContext SynchronizationContext =>
            _SynchronizationContext ?? (_SynchronizationContext = new MonoThreadedQueueSynchronizationContext(this));

        public TaskScheduler TaskScheduler => new SynchronizationContextTaskScheduler(SynchronizationContext);

        public void Dispose()
        {
            StopQueueing();
        }
    }
}
