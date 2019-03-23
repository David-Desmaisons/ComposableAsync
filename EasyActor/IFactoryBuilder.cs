﻿using System;
using System.Threading;
using Concurrent;

namespace EasyActor
{
    /// <summary>
    /// IActorFactory and ILoadBalancerFactory factory
    /// </summary>
    public interface IFactoryBuilder
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
        IActorFactory GetFactory(bool shared = false, Action<Thread> onCreate = null);

        /// <summary>
        /// Returns an actor factory using Thread Pool as Fiber implementation
        /// </summary>
        IActorFactory GetThreadPoolFactory();

        /// <summary>
        /// Returns an actor factory using Task Pool as Fiber implementation
        /// </summary>
        IActorFactory GetTaskBasedFactory();

        /// <summary>
        /// Returns an actor factory using the current synchronization context
        /// </summary>
        IActorFactory GetInContextFactory();

        /// <summary>
        /// Returns an actor factory using the provided synchronizationContext 
        /// </summary>
        IActorFactory GetInContextFactory(SynchronizationContext synchronizationContext);

        /// <summary>
        /// Returns an actor factory using the provided fiber 
        /// </summary>
        IActorFactory GetFactoryForFiber(IFiber dispatcher);
    }
}
