using EasyActor.Queue;
using EasyActor.TaskHelper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace EasyActor
{
    internal class TaskSchedulerQueue : IStopableTaskQueue
    {
        private readonly TaskScheduler _Scheduler;
        private readonly TaskFactory _TaskFactory;
        private readonly ConcurrentExclusiveSchedulerPair _ConcurrentExclusiveSchedulerPair;
        private Func<Task> _SetCleanUp; 
        public TaskSchedulerQueue()
        {
            _ConcurrentExclusiveSchedulerPair = new ConcurrentExclusiveSchedulerPair();
            _Scheduler = _ConcurrentExclusiveSchedulerPair.ExclusiveScheduler;
            _TaskFactory = new TaskFactory(_Scheduler);
        }

        private static Task<T> Safe<T>(Func<Task<T>> compute)
        {
            try
            {
                return compute();
            }
            catch
            {
                return TaskBuilder<T>.Cancelled;
            }
        }

        public Task<T> Enqueue<T>(Func<T> action)
        {
            return Safe( () => _TaskFactory.StartNew(action));
        }

        public Task Enqueue(Func<Task> action)
        {
            return Safe(() =>
                {
                    var workItem = new AsyncActionWorkItem(action);
                    _TaskFactory.StartNew(() => workItem.Do());
                    return workItem.Task;
                });
        }

        public Task<T> Enqueue<T>(Func<Task<T>> action)
        {
            return Safe(() =>
               {
                   var workItem = new AsyncWorkItem<T>(action);
                   _TaskFactory.StartNew(() => workItem.Do());
                   return workItem.Task;
               });
        }

        public TaskScheduler TaskScheduler
        {
            get { return _Scheduler; }
        }

        public Task Stop(Func<Task> cleanup)
        {
            if (cleanup!=null)
                Enqueue(cleanup); 
            
            _ConcurrentExclusiveSchedulerPair.Complete();
            return _ConcurrentExclusiveSchedulerPair.Completion;
        }
    }
}
