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

    public class MonoThreadedQueue : IDisposable
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
            catch (Exception exception)
            {
                if ((exception is ObjectDisposedException) || (exception is InvalidOperationException))
                    return TaskBuilder.GetCancelled<object>();

                throw;
            }
        }

        private Task<T> Enqueue<T>(AsyncWorkItem<T> workitem)
        {
            try
            {
                _TaskQueue.Add(workitem);
                return workitem.Task;
            }
            catch(Exception exception)
            {
                if ((exception is ObjectDisposedException) || (exception is InvalidOperationException))
                    return TaskBuilder.GetCancelled<T>();

                throw;
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
            _CTS.Cancel();
            _TaskQueue.CompleteAdding();
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
            SynchronizationContext.SetSynchronizationContext(new DispatcherSynchronizationContext(this));

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
    }
}
