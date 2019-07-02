using System;
using System.Threading;
using System.Threading.Tasks;
using ComposableAsync.Concurrent.Dispatchers;
using ComposableAsync.Concurrent.SynchronizationContexts;

namespace ComposableAsync.Concurrent.Fibers
{
    internal sealed class TaskSchedulerFiber : TaskSchedulerDispatcher, IStoppableFiber
    {
        public bool IsAlive { get; private set; } = true;
        public SynchronizationContext SynchronizationContext { get; }

        private readonly ConcurrentExclusiveSchedulerPair _ConcurrentExclusiveSchedulerPair;

        internal TaskSchedulerFiber() : this(new ConcurrentExclusiveSchedulerPair())
        {
        }

        private TaskSchedulerFiber(ConcurrentExclusiveSchedulerPair concurrentExclusiveSchedulerPair) : base(concurrentExclusiveSchedulerPair.ExclusiveScheduler)
        {
            _ConcurrentExclusiveSchedulerPair = concurrentExclusiveSchedulerPair;
            SynchronizationContext = new TaskSchedulerSynchronizationContext(concurrentExclusiveSchedulerPair.ExclusiveScheduler);
        }

        public async Task DisposeAsync()
        {
            GC.SuppressFinalize(this);
            _ConcurrentExclusiveSchedulerPair.Complete();
            await _ConcurrentExclusiveSchedulerPair.Completion;
            IsAlive = false;
        }

        ~TaskSchedulerFiber()
        {
            _ConcurrentExclusiveSchedulerPair.Complete();
        }
    }
}
