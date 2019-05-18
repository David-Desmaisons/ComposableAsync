using System;
using System.Collections.Generic;
using System.Security;
using System.Threading;
using System.Threading.Tasks;

namespace ComposableAsync.Concurrent.TaskSchedulers
{
    /// <summary>
    /// Task scheduler based on fiber
    /// </summary>
    internal sealed class FiberTaskScheduler : TaskScheduler
    {
        private readonly IFiber _Fiber;
        private IMonoThreadFiber MonoThreadFiber => _Fiber as IMonoThreadFiber;

        public FiberTaskScheduler(IFiber fiber)
        {
            _Fiber = fiber ?? throw new ArgumentNullException(nameof(fiber), "fiber can not be null");
        }

        [SecurityCritical]
        protected override void QueueTask(Task task)
        {
            _Fiber.Dispatch(() => TryExecuteTask(task));
        }

        [SecurityCritical]
        protected override bool TryExecuteTaskInline(Task task, bool taskWasPreviouslyQueued)
        {
            return (Thread.CurrentThread == MonoThreadFiber?.Thread) && TryExecuteTask(task);
        }

        [SecurityCritical]
        protected override IEnumerable<Task> GetScheduledTasks() => null;

        public override int MaximumConcurrencyLevel => 1;
    }
}