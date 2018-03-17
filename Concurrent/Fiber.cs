using System;
using System.Threading;
using Concurrent.Dispatchers;
using Concurrent.Fibers;

namespace Concurrent
{
    public static class Fiber
    {
        public static IMonoThreadFiber GetMonoThreadedFiber(Action<Thread> onCreate = null)
        {
            return new MonoThreadedFiber(onCreate);
        }

        public static IMonoThreadFiber GetThreadPoolFiber(Action<Thread> onCreate = null)
        {
            return new ThreadPoolFiber();
        }

        public static IFiber GetFiberFromSynchronizationContext(SynchronizationContext synchronizationContext)
        {
            return new SynchronizationContextFiber(synchronizationContext);
        }

        public static IFiber GetFiberFromCurrentContext()
        {
            var context = SynchronizationContext.Current;
            return (context == null) ? new SynchronizationContextFiber(context) : null;
        }

        public static IDispatcher GetDispatcherFromCurrentContext()
        {
            return (IDispatcher) GetFiberFromCurrentContext() ?? new NullDispatcher();
        }

        public static IStopableFiber GetTaskBasedFiber()
        {
            return TaskSchedulerFiber.GetFiber();
        }
    }
}
