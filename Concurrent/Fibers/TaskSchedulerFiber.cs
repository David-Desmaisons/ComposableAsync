using System;
using System.Threading;
using System.Threading.Tasks;
using Concurrent.Dispatchers;
using Concurrent.SynchronizationContexts;

namespace Concurrent.Fibers
{
    internal sealed class TaskSchedulerFiber : TaskSchedulderDispatcher, IStopableFiber
    {
        private readonly ConcurrentExclusiveSchedulerPair _ConcurrentExclusiveSchedulerPair;

        internal TaskSchedulerFiber():this(new ConcurrentExclusiveSchedulerPair())
        {          
        }

        private TaskSchedulerFiber(ConcurrentExclusiveSchedulerPair concurrentExclusiveSchedulerPair) : base (concurrentExclusiveSchedulerPair.ExclusiveScheduler)
        {
            _ConcurrentExclusiveSchedulerPair = concurrentExclusiveSchedulerPair;
            SynchronizationContext = new TaskSchedulerSynchronizationContext(concurrentExclusiveSchedulerPair.ExclusiveScheduler);
        }

        public override async Task Stop(Func<Task> cleanup)
        {
            await base.Stop(cleanup);

            _ConcurrentExclusiveSchedulerPair.Complete();
            await _ConcurrentExclusiveSchedulerPair.Completion;
        }

        public SynchronizationContext SynchronizationContext { get; }
    }
}
