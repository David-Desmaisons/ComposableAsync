using System;
using System.Collections.Generic;
using System.Linq;
using System.Security;
using System.Text;
using System.Threading;
using System.Threading.Tasks;


namespace EasyActor.TaskHelper
{   
    /// <summary>
    /// Adapted from http://referencesource.microsoft.com/#mscorlib/system/threading/Tasks/TaskScheduler.cs,30e7d3d352bbb730
    /// </summary>
    internal class SynchronizationContextTaskScheduler :  TaskScheduler
    {
        private SynchronizationContext m_synchronizationContext;
     
        public SynchronizationContextTaskScheduler(SynchronizationContext synContext)
        {
            if (synContext == null)
            {
                throw new ArgumentNullException("synContext can not be null");
            }
 
            m_synchronizationContext = synContext;
        }

        internal SynchronizationContext SynchronizationContext { get { return m_synchronizationContext; } }
 
        [SecurityCritical]
        protected override void QueueTask(Task task)
        {
            m_synchronizationContext.Post(PostCallback, task);
        }
      
        [SecurityCritical]
        protected override bool TryExecuteTaskInline(Task task, bool taskWasPreviouslyQueued)
        {
            return (SynchronizationContext.Current == m_synchronizationContext) ?  TryExecuteTask(task) : false;
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

        public override Int32 MaximumConcurrencyLevel
        {
            get { return 1; }
        }

        private void PostCallback(object obj)
        {
            base.TryExecuteTask((Task)obj);
        }
    }
}
