using System;
using System.Threading.Tasks;

namespace EasyActor.Fiber
{
    internal class TaskSchedulerFiber 
    {
        public static IStopableFiber GetFiber()
        {
            var concurrentExclusiveSchedulerPair = new ConcurrentExclusiveSchedulerPair();
            Func<Task> complete = () =>
            {
                concurrentExclusiveSchedulerPair.Complete();
                return concurrentExclusiveSchedulerPair.Completion;
            };
            return new TaskSchedulderDispatcher(concurrentExclusiveSchedulerPair.ExclusiveScheduler, complete);
        }
    }
}
