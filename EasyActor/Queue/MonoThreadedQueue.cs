using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;
using EasyActor.TaskHelper;


namespace EasyActor.Queue
{
    public class MonoThreadedQueue : IAbortableTaskQueue, IDisposable
    {
        private static int _Count = 0;

        private readonly BlockingCollection<IWorkItem> _TaskQueue = new BlockingCollection<IWorkItem>();
        private readonly Thread _Current;
        private readonly CancellationTokenSource _CTS;
        private AsyncActionWorkItem _Clean;
        private bool _Running = false;

        public MonoThreadedQueue(Action<Thread> onCreate=null)
        {
            _CTS = new CancellationTokenSource();

            _Current = new Thread(Consume)
            {
                IsBackground = true,
                Name = string.Format("MonoThreadedQueue-{0}", _Count++)
            };

            onCreate?.Invoke(_Current);
            _Current.Start();
        }

        public int EnqueuedTasksNumber
        {
            get { return _TaskQueue.Count + (_Running ? 1 : 0); }
        }

        public void Send(Action action)
        {
            if (Thread.CurrentThread==_Current)
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
            catch(Exception)
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

        public Task Enqueue(Action action)
        {
            return Enqueue( new ActionWorkItem(action) );
        }

        public Task Enqueue(Func<Task> action)
        {
            return Enqueue(new AsyncActionWorkItem(action));
        }

        public Task<T> Enqueue<T>( Func<Task<T>> action)
        {
            return Enqueue(new AsyncWorkItem<T>(action));
        }
     
        private void StopQueueing() 
        {
            try
            {
                _CTS.Cancel();
                _TaskQueue.CompleteAdding();
            }
            catch(ObjectDisposedException)
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
            SynchronizationContext.SetSynchronizationContext(this.SynchronizationContext);

            try
            {
                foreach (var action in _TaskQueue.GetConsumingEnumerable(_CTS.Token))
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
        }

        private SynchronizationContext _SynchronizationContext;
        private SynchronizationContext SynchronizationContext
        {
            get 
            {
                if (_SynchronizationContext == null)
                    _SynchronizationContext = new MonoThreadedQueueSynchronizationContext(this);

                return _SynchronizationContext;
            }
        }

        public TaskScheduler TaskScheduler
        {
            get { return new SynchronizationContextTaskScheduler(SynchronizationContext); }
        }

        public void Dispose()
        {
            StopQueueing();
        }
    }
}
