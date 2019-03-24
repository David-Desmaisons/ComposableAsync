using System;
using System.Threading;
using Concurrent;

namespace EasyActor
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
        IProxyFactory GetFactory(bool shared = false, Action<Thread> onCreate = null);

        /// <summary>
        /// Returns an actor factory using Thread Pool as Fiber implementation
        /// </summary>
        IProxyFactory GetThreadPoolFactory();

        /// <summary>
        /// Returns an actor factory using Task Pool as Fiber implementation
        /// </summary>
        IProxyFactory GetTaskBasedFactory();

        /// <summary>
        /// Returns an actor factory using the current synchronization context
        /// </summary>
        IProxyFactory GetInContextFactory();

        /// <summary>
        /// Returns an actor factory using the provided synchronizationContext 
        /// </summary>
        IProxyFactory GetInContextFactory(SynchronizationContext synchronizationContext);

        /// <summary>
        /// Returns an actor factory using the provided fiber 
        /// </summary>
        IProxyFactory GetFactoryForFiber(IFiber dispatcher);
    }
}
