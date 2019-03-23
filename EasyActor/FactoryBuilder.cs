using System;
using System.Threading;
using Concurrent;
using EasyActor.DispatcherManagers;
using EasyActor.Factories;

namespace EasyActor
{
    /// <summary>
    /// Actor factory builder
    /// </summary>
    public class FactoryBuilder : IFactoryBuilder
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
        public IActorFactory GetFactory(bool shared = false, Action<Thread> onCreate = null)
        {
            return shared ? new ActorFactory(new SharedThreadFiberManager(onCreate)) : 
                            new ActorFactory(new StandardFiberManager(onCreate));
        }

        /// <summary>
        /// Returns an actor factory using Thread Pool as Fiber implementation
        /// </summary>
        public IActorFactory GetThreadPoolFactory()
        {
            return new ActorFactory(new TheadPoolFiberManager());
        }

        /// <summary>
        /// Returns an actor factory using the current synchronization context
        /// </summary>
        public IActorFactory GetInContextFactory()
        {
            return new ActorFactory(new SynchronizationContextFiberManage());
        }

        /// <summary>
        /// Returns an actor factory using Task Pool as Fiber implementation
        /// </summary>
        public IActorFactory GetTaskBasedFactory()
        {
            return new ActorFactory(new TaskPoolFiberManager());
        }

        /// <summary>
        /// Returns an actor factory using the provided synchronizationContext 
        /// </summary>
        public IActorFactory GetInContextFactory(SynchronizationContext synchronizationContext)
        {
            return new ActorFactory(new SynchronizationContextFiberManage(synchronizationContext));
        }

        /// <summary>
        /// Returns an actor factory using the provided fiber 
        /// </summary>
        public IActorFactory GetFactoryForFiber(IFiber fiber)
        {
            return new ActorFactory(new DispatcherManager(fiber));
        }
    }
}
