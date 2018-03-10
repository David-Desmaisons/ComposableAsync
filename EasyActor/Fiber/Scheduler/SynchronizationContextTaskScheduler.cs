using System;
using System.Collections.Generic;
using System.Security;
using System.Threading;
using System.Threading.Tasks;

namespace EasyActor.Fiber.Scheduler
{
    /// <summary>
    /// Adapted from http://referencesource.microsoft.com/#mscorlib/system/threading/Tasks/TaskScheduler.cs,30e7d3d352bbb730
    /// </summary>
    internal sealed class SynchronizationContextTaskScheduler : TaskScheduler
    {
        private readonly SynchronizationContext _SynchronizationContext;

        public SynchronizationContextTaskScheduler(SynchronizationContext synContext)
        {
            if (synContext == null)
                throw new ArgumentNullException(nameof(synContext), "synContext can not be null");

            _SynchronizationContext = synContext;
        }

        internal SynchronizationContext SynchronizationContext => _SynchronizationContext;

        [SecurityCritical]
        protected override void QueueTask(Task task)
        {
            _SynchronizationContext.Post(PostCallback, task);
        }

        [SecurityCritical]
        protected override bool TryExecuteTaskInline(Task task, bool taskWasPreviouslyQueued)
        {
            return (SynchronizationContext.Current == _SynchronizationContext) && TryExecuteTask(task);
        }

        [SecurityCritical]
        protected override IEnumerable<Task> GetScheduledTasks()
        {
            return null;
        }

        public IEnumerable<Task> GetScheduledTasksEnumerable()
        {
            return GetScheduledTasks();
        }

        public override Int32 MaximumConcurrencyLevel => 1;

        private void PostCallback(object obj)
        {
            base.TryExecuteTask((Task)obj);
        }
    }
}
