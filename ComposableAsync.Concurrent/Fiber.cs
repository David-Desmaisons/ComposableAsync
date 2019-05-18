using System;
using System.Threading;
using ComposableAsync.Concurrent.Fibers;

namespace ComposableAsync.Concurrent
{
    /// <summary>
    /// <see cref="IFiber"/> factory
    /// </summary>
    public static class Fiber
    {
        /// <summary>
        /// Returns a mono threaded fiber
        /// </summary>
        /// <param name="onCreate"></param>
        /// <returns></returns>
        public static IMonoThreadFiber CreateMonoThreadedFiber(Action<Thread> onCreate = null)
        {
            return new MonoThreadedFiber(onCreate);
        }

        /// <summary>
        /// Returns a fiber using the thread pool
        /// </summary>
        /// <returns></returns>
        public static IMonoThreadFiber GetThreadPoolFiber()
        {
            return new ThreadPoolFiber();
        }

        /// <summary>
        /// Returns a fiber based on ConcurrentExclusiveSchedulerPair
        /// </summary>
        /// <returns></returns>
        public static IStoppableFiber GetTaskBasedFiber()
        {
            return new TaskSchedulerFiber();
        }

        /// <summary>
        /// Returns a fiber based on a given synchronization context
        /// </summary>
        /// <param name="synchronizationContext"></param>
        /// <returns></returns>
        public static IFiber GetFiberFromSynchronizationContext(SynchronizationContext synchronizationContext)
        {
            return new SynchronizationContextFiber(synchronizationContext);
        }

        /// <summary>
        /// Returns a fiber based from current synchronization context
        /// or null if the current context is null
        /// </summary>
        /// <returns></returns>
        public static IFiber GetFiberFromCurrentContext()
        {
            var context = SynchronizationContext.Current;
            return (context != null) ? new SynchronizationContextFiber(context) : null;
        }

        /// <summary>
        /// Returns a dispatcher based from current synchronization context
        /// or null dispatcher if the current context is null
        /// </summary>
        /// <returns></returns>
        public static IDispatcher GetDispatcherFromCurrentContext()
        {
            return GetFiberFromCurrentContext() ?? NullDispatcher.Instance;
        }
    }
}
