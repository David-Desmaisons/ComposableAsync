﻿using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;


namespace EasyActor.Queue
{

    public class AsyncQueueMonoThreadDispatcher : IDisposable
    {
        private static int _Count = 0;
        
        private BlockingCollection<Action> _TaskQueue = new BlockingCollection<Action>();
        private Thread _Current;

        public AsyncQueueMonoThreadDispatcher(Priority iPriority = Priority.Normal)
        {
            _Current = new Thread(Consume)
            {
                IsBackground = true,
                Priority = (ThreadPriority)iPriority,
                Name = string.Format("AsyncQueueDispatcher-{0}", _Count++)
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

        public Task Enqueue(Action action)
        {
            TaskCompletionSource<object> tcs = new TaskCompletionSource<object>();
            _TaskQueue.Add( () =>
            {
                try
                {
                    action();
                    tcs.TrySetResult(null);
                }
                catch (Exception e)
                {
                    tcs.TrySetException(e);
                }

            });
            return tcs.Task;
        }


        public Task Enqueue(Func<Task> action)
        {
            TaskCompletionSource<object> tcs = new TaskCompletionSource<object>();
            _TaskQueue.Add(async  () =>
                {
                    try
                    {
                        await action();
                        tcs.TrySetResult(null);
                    }
                    catch(Exception e)
                    {
                        tcs.TrySetException(e);
                    }
                   
                });
            return tcs.Task;
        }



        public Task<T> Enqueue<T>( Func<Task<T>> action)
        {
            TaskCompletionSource<T> tcs = new TaskCompletionSource<T>();
            _TaskQueue.Add(async () =>
            {
                try
                {
                    tcs.TrySetResult(await action());
                }
                catch (Exception e)
                {
                    tcs.TrySetException(e);
                }

            });
            //ObjectDisposedException	 The BlockingCollection<T> has been disposed.
            //InvalidOperationException The BlockingCollection<T> has been marked as complete with regards to additions.
            //-or- The underlying collection didn't accept the item.
            return tcs.Task;
        }

        private CancellationTokenSource _CTS;

        private void Consume()
        {
            _CTS = new CancellationTokenSource();

            SynchronizationContext.SetSynchronizationContext(new DispatcherSynchronizationContext(this));

            try
            {
                foreach (var action in _TaskQueue.GetConsumingEnumerable(_CTS.Token))
                {
                    action();
                }
            }
            catch (OperationCanceledException)
            {  
                _TaskQueue.CompleteAdding();
            }
        }

        public void Dispose() 
        {
            _CTS.Cancel();
        }
    }
}
