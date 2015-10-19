using EasyActor.Queue;
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


        public SynchronizationContext SynchronizationContext
        {
            get { return _Context; }
        }
    }
}
