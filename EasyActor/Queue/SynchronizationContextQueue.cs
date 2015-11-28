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
    internal class SynchronizationContextQueue : ITaskQueue
    {
        private SynchronizationContext _Context;
        public SynchronizationContextQueue(SynchronizationContext synchronizationContext)
        {
            _Context = synchronizationContext;
        }

        public Task<T> Enqueue<T>(Func<T> action)
        {
            var workitem = new WorkItem<T>(action);
            _Context.Post(_ => workitem.Do(), null);
            return workitem.Task;
        }

        public Task Enqueue(Func<Task> action)
        {
            var workitem = new AsyncActionWorkItem(action);
            _Context.Post(_ => workitem.Do(), null);
            return workitem.Task;
        }

        public Task<T> Enqueue<T>(Func<Task<T>> action)
        {
            var workitem = new AsyncWorkItem<T>(action);
            _Context.Post(_ => workitem.Do(),null);
            return workitem.Task;
        }

        public TaskScheduler TaskScheduler
        {
            get { return new SynchronizationContextTaskScheduler(_Context); }
        }
    }
}
