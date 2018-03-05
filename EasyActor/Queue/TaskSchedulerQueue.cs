using EasyActor.Queue;
using EasyActor.TaskHelper;
using System;
using System.Threading.Tasks;

namespace EasyActor
{
    internal class TaskSchedulerQueue : IStopableTaskQueue
    {
        private readonly TaskScheduler _Scheduler;
        private readonly TaskFactory _TaskFactory;
        private readonly ConcurrentExclusiveSchedulerPair _ConcurrentExclusiveSchedulerPair;
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

        private static Task Safe(Func<Task> compute) 
        {
            try 
            {
                return compute();
            }
            catch 
            {
                return TaskBuilder.Cancelled;
            }
        }

        public Task Enqueue(Action action) 
        {
            return Safe(() => _TaskFactory.StartNew(action));
        }


        public Task<T> Enqueue<T>(Func<T> action)
        {
            return Safe(() => _TaskFactory.StartNew(action));
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

        public TaskScheduler TaskScheduler => _Scheduler;

        public Task Stop(Func<Task> cleanup)
        {
            if (cleanup != null)
                Enqueue(cleanup);

            _ConcurrentExclusiveSchedulerPair.Complete();
            return _ConcurrentExclusiveSchedulerPair.Completion;
        }
    }
}
