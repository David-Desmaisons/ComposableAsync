using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using EasyActor.TaskHelper;


namespace EasyActor.Queue
{

    public class MonoThreadedQueue : IDisposable, ITaskQueue
    {
        private static int _Count = 0;

        private BlockingCollection<IWorkItem> _TaskQueue = new BlockingCollection<IWorkItem>();
        private Thread _Current;
        private CancellationTokenSource _CTS;
        private AsyncActionWorkItem _Clean;

        public MonoThreadedQueue(Priority iPriority = Priority.Normal)
        {
            _CTS = new CancellationTokenSource();

            _Current = new Thread(Consume)
            {
                IsBackground = true,
                Priority = (ThreadPriority)iPriority,
                Name = string.Format("MonoThreadedQueue-{0}", _Count++)
            };
            _Current.Start();
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
                return TaskBuilder.GetCancelled<T>();
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
     
        public void Dispose() 
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

        public void Stop()
        {
            _TaskQueue.CompleteAdding();
        }

        public Task SetCleanUp(Func<Task> cleanup)
        {
            _Clean = new AsyncActionWorkItem(cleanup);
            return _Clean.Task;
        }

        private void Consume()
        {
            SynchronizationContext.SetSynchronizationContext(this.SynchronizationContext);

            try
            {
                foreach (var action in _TaskQueue.GetConsumingEnumerable(_CTS.Token))
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

            if (_Clean != null)
                _Clean.Do();
        }


        private SynchronizationContext _SynchronizationContext;
        public SynchronizationContext SynchronizationContext
        {
            get 
            {
                if (_SynchronizationContext == null)
                    _SynchronizationContext = new MonoThreadedQueueSynchronizationContext(this);

                return _SynchronizationContext;
            }
        }
    }
}
