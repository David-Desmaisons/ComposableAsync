using System;
using System.Threading;
using ComposableAsync.Factory;

namespace ComposableAsync.Concurrent
{
    /// <summary>
    /// IProxyFactory and ILoadBalancerFactory factory
    /// </summary>
    public interface IActorFactoryBuilder
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
    }
}
