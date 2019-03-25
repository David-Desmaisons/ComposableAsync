using System;
using System.Threading;
using Concurrent;
using EasyActor.DispatcherManagers;

namespace EasyActor
{
    /// <summary>
    /// Actor factory builder
    /// </summary>
    public class ActorFactoryBuilder : IActorFactoryBuilder
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
        public IProxyFactory GetFactory(bool shared = false, Action<Thread> onCreate = null)
        {
            return shared ? new ProxyFactory(new SharedThreadFiberManager(onCreate)) : 
                            new ProxyFactory(new StandardFiberManager(onCreate));
        }

        /// <summary>
        /// Returns an actor factory using Thread Pool as Fiber implementation
        /// </summary>
        public IProxyFactory GetThreadPoolFactory()
        {
            return new ProxyFactory(new TheadPoolFiberManager());
        }

        /// <summary>
        /// Returns an actor factory using the current synchronization context
        /// </summary>
        public IProxyFactory GetInContextFactory()
        {
            return new ProxyFactory(new SynchronizationContextFiberManage());
        }

        /// <summary>
        /// Returns an actor factory using Task Pool as Fiber implementation
        /// </summary>
        public IProxyFactory GetTaskBasedFactory()
        {
            return new ProxyFactory(new TaskPoolFiberManager());
        }

        /// <summary>
        /// Returns an actor factory using the provided synchronizationContext 
        /// </summary>
        public IProxyFactory GetInContextFactory(SynchronizationContext synchronizationContext)
        {
            return new ProxyFactory(new SynchronizationContextFiberManage(synchronizationContext));
        }

        /// <summary>
        /// Returns an actor factory using the provided fiber 
        /// </summary>
        public IProxyFactory GetFactoryForFiber(IFiber fiber)
        {
            return new ProxyFactory(new MonoDispatcherManager(fiber));
        }
    }
}
