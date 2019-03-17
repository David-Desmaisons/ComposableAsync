using System;
using System.Threading;
using Concurrent.Dispatchers;
using Concurrent.Fibers;

namespace Concurrent
{
    /// <summary>
    /// <see cref="IFiber"/> factory
    /// </summary>
    public static class Fiber
    {
        public static IMonoThreadFiber CreateMonoThreadedFiber(Action<Thread> onCreate = null)
        {
            return new MonoThreadedFiber(onCreate);
        }

        public static IMonoThreadFiber GetThreadPoolFiber()
        {
            return new ThreadPoolFiber();
        }

        public static IStoppableFiber GetTaskBasedFiber()
        {
            return new TaskSchedulerFiber();
        }

        public static IFiber GetFiberFromSynchronizationContext(SynchronizationContext synchronizationContext)
        {
            return new SynchronizationContextFiber(synchronizationContext);
        }

        public static IFiber GetFiberFromCurrentContext()
        {
            var context = SynchronizationContext.Current;
            return (context != null) ? new SynchronizationContextFiber(context) : null;
        }

        public static IDispatcher GetDispatcherFromCurrentContext()
        {
            return GetFiberFromCurrentContext() ?? NullDispatcher.Instance;
        }
    }
}
