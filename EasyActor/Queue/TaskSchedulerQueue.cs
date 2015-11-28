using EasyActor.Queue;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace EasyActor
{
    internal class TaskSchedulerQueue : ITaskQueue
    {
        private TaskScheduler _Scheduler;
        private TaskFactory _TaskFactory;
        public TaskSchedulerQueue(TaskScheduler taskScheduler)
        {
            _Scheduler = taskScheduler;
            _TaskFactory = new TaskFactory(_Scheduler);
        }

        public Task<T> Enqueue<T>(Func<T> action)
        {
            return _TaskFactory.StartNew(action);
        }

        public Task Enqueue(Func<Task> action)
        {
            var workItem = new AsyncActionWorkItem(action);
            _TaskFactory.StartNew(() => workItem.Do());
            return workItem.Task;
        }

        public Task<T> Enqueue<T>(Func<Task<T>> action)
        {
            var workItem = new AsyncWorkItem<T>(action);
            _TaskFactory.StartNew(() => workItem.Do());
            return workItem.Task;
        }

        public TaskScheduler TaskScheduler
        {
            get { return _Scheduler; }
        }
    }
}
