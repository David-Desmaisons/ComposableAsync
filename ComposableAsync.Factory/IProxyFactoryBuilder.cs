using System;
using System.Threading;
using ComposableAsync.Concurrent;

namespace ComposableAsync.Factory
{
    /// <summary>
    /// IProxyFactory and ILoadBalancerFactory factory
    /// </summary>
    public interface IProxyFactoryBuilder
    {
        /// <summary>
        /// Returns an actor factory 
        /// <param name="shared">
        /// if true all the actors created by this factory will use the same thread. 
        /// </param>
        /// <param name="onCreate">
        /// Delegate called on each new created thread. 
        /// </param>
        /// </summary>
        IProxyFactory GetActorFactory(bool shared = false, Action<Thread> onCreate = null);

        /// <summary>
        /// Returns an actor factory using Thread Pool as Fiber implementation
        /// </summary>
        IProxyFactory GetThreadPoolBasedActorFactory();

        /// <summary>
        /// Returns an actor factory using Task Pool as Fiber implementation
        /// </summary>
        IProxyFactory GetTaskBasedActorFactory();

        /// <summary>
        /// Returns an actor factory using the current synchronization context
        /// </summary>
        IProxyFactory GetInContextActorFactory();

        /// <summary>
        /// Returns an actor factory using the provided synchronizationContext 
        /// </summary>
        IProxyFactory GetInContextActorFactory(SynchronizationContext synchronizationContext);

        /// <summary>
        /// Returns an actor factory using the provided fiber 
        /// </summary>
        IProxyFactory GetActorFactoryFrom(IFiber dispatcher);

        /// <summary>
        /// Returns an proxy factory using the provided dispatcher
        /// Disposing the created factory will dispose the provided dispatcher
        /// </summary>
        /// <param name="dispatcher"></param>
        /// <returns></returns>
        IProxyFactory GetManagedProxyFactory(ICancellableDispatcher dispatcher);

        /// <summary>
        /// Returns an proxy factory using the provided dispatchers sequentially
        /// Disposing the created factory will dispose the provided dispatcher
        /// </summary>
        /// <param name="dispatcher1"></param>
        /// <param name="dispatcher2"></param>
        /// <returns></returns>
        IProxyFactory GetManagedProxyFactory(ICancellableDispatcher dispatcher1, ICancellableDispatcher dispatcher2);

        /// <summary>
        /// Returns an proxy factory using the provided dispatchers sequentially
        /// Disposing the created factory will dispose the provided dispatcher
        /// </summary>
        /// <param name="dispatchers"></param>
        /// <returns></returns>
        IProxyFactory GetManagedProxyFactory(params ICancellableDispatcher[] dispatchers);

        /// <summary>
        /// Returns an proxy factory using the provided dispatcher
        /// Disposing the created factory will not dispose the provided dispatcher 
        /// </summary>
        /// <param name="dispatcher"></param>
        /// <returns></returns>
        IProxyFactory GetUnmanagedProxyFactory(ICancellableDispatcher dispatcher);

        /// <summary>
        /// Returns an proxy factory using the provided dispatchers sequentially
        /// Disposing the created factory will dispose the provided dispatcher
        /// </summary>
        /// <param name="dispatcher1"></param>
        /// <param name="dispatcher2"></param>
        /// <returns></returns>
        IProxyFactory GetUnmanagedProxyFactory(ICancellableDispatcher dispatcher1, ICancellableDispatcher dispatcher2);

        /// <summary>
        /// Returns an proxy factory using the provided dispatchers sequentially
        /// Disposing the created factory will dispose the provided dispatcher
        /// </summary>
        /// <param name="dispatchers"></param>
        /// <returns></returns>
        IProxyFactory GetUnmanagedProxyFactory(params ICancellableDispatcher[] dispatchers);
    }
}
