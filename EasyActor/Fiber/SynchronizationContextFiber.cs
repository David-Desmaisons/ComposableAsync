using System;
using System.Threading;
using System.Threading.Tasks;
using EasyActor.Fiber.Scheduler;
using EasyActor.Fiber.WorkItems;
using EasyActor.TaskHelper;

namespace EasyActor.Fiber
{
    internal class SynchronizationContextFiber : IFiber
    {
        private readonly SynchronizationContext _Context;
        public SynchronizationContextFiber(SynchronizationContext synchronizationContext)
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

        public void Dispatch(Action action)
        {
            _Context.Post(_ => action(), null);
        }

        public Task Enqueue(Action action) 
        {
            var workitem = new ActionWorkItem(action);
            _Context.Post(_ => workitem.Do(), null);
            return workitem.Task;
        }

        public TaskScheduler TaskScheduler => new SynchronizationContextTaskScheduler(_Context);
    }
}
