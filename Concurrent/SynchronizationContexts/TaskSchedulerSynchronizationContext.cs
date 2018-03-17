using System;
using System.Threading;
using System.Threading.Tasks;

namespace Concurrent.SynchronizationContexts
{
    public sealed class TaskSchedulerSynchronizationContext : SynchronizationContext
    {
        private readonly TaskFactory _TaskFactory;

        public TaskSchedulerSynchronizationContext(TaskScheduler taskScheduler): this (new TaskFactory(taskScheduler))
        {
            if (taskScheduler == null)
                throw new ArgumentNullException(nameof(taskScheduler), "taskScheduler can not be null");

            if (taskScheduler.MaximumConcurrencyLevel > 1)
                throw new ArgumentNullException(nameof(taskScheduler), "taskScheduler MaximumConcurrencyLevel can not be more than 1");
        }

        private TaskSchedulerSynchronizationContext(TaskFactory taskFactory)
        {
            _TaskFactory = taskFactory;
        }

        public override void Send(SendOrPostCallback d, object state)
        {
            _TaskFactory.StartNew(() => d(state)).Wait();
        }

        public override void Post(SendOrPostCallback d, object state)
        {
            _TaskFactory.StartNew(() => d(state));
        }

        public override SynchronizationContext CreateCopy()
        {
            return new TaskSchedulerSynchronizationContext(_TaskFactory);
        }
    }
}
