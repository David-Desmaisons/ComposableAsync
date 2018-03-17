using System;
using System.Threading;
using System.Threading.Tasks;
using Concurrent.Dispatchers;
using Concurrent.SynchronizationContexts;

namespace Concurrent.Fibers
{
    internal sealed class TaskSchedulerFiber : TaskSchedulderDispatcher, IStopableFiber
    {
        private TaskSchedulerFiber(TaskScheduler taskScheduler, Func<Task> complete) : base (taskScheduler, complete)
        {
            SynchronizationContext = new TaskSchedulerSynchronizationContext(taskScheduler);
        }

        internal static IStopableFiber GetFiber()
        {
            var concurrentExclusiveSchedulerPair = new ConcurrentExclusiveSchedulerPair();
            Func<Task> complete = () =>
            {
                concurrentExclusiveSchedulerPair.Complete();
                return concurrentExclusiveSchedulerPair.Completion;
            };
            return new TaskSchedulerFiber(concurrentExclusiveSchedulerPair.ExclusiveScheduler, complete);
        }

        public SynchronizationContext SynchronizationContext { get; }
    }
}
