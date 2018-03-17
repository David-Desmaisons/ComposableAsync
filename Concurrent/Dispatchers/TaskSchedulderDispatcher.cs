﻿using System;
using System.Threading.Tasks;
using Concurrent.Tasks;
using Concurrent.WorkItems;

namespace Concurrent.Dispatchers
{
    public class TaskSchedulderDispatcher : IDispatcher
    {
        private readonly TaskFactory _TaskFactory;
        private readonly Func<Task> _Complete;

        public TaskSchedulderDispatcher(TaskScheduler taskScheduler, Func<Task> complete)
        {
            _TaskFactory = new TaskFactory(taskScheduler);
            _Complete = complete;
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

        public void Dispatch(Action action)
        {
            Safe(() => _TaskFactory.StartNew(action));
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

        public Task Stop(Func<Task> cleanup)
        {
            if (cleanup != null)
                Enqueue(cleanup);

            return _Complete();
        }
    }
}
