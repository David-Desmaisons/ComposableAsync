using System;
using System.Threading;
using ComposableAsync.Factory;
using ComposableAsync.Concurrent.DispatcherManagers;

namespace ComposableAsync.Concurrent
{
    /// <summary>
    /// Actor factory builder
    /// </summary>
    public class ActorFactoryBuilder : IActorFactoryBuilder
    {
        /// <inheritdoc />
        public IProxyFactory GetActorFactory(bool shared = false, Action<Thread> onCreate = null)
        {
            return shared ? new ProxyFactory(new SharedThreadFiberManager(onCreate)) :
                            new ProxyFactory(new StandardFiberManager(onCreate));
        }

        /// <inheritdoc />
        public IProxyFactory GetThreadPoolBasedActorFactory()
        {
            return new ProxyFactory(new TheadPoolFiberManager());
        }

        /// <inheritdoc />
        public IProxyFactory GetInContextActorFactory()
        {
            return new ProxyFactory(new SynchronizationContextFiberManager());
        }

        /// <inheritdoc />
        public IProxyFactory GetTaskBasedActorFactory()
        {
            return new ProxyFactory(new TaskPoolFiberManager());
        }

        /// <inheritdoc />
        public IProxyFactory GetInContextActorFactory(SynchronizationContext synchronizationContext)
        {
            return new ProxyFactory(new SynchronizationContextFiberManager(synchronizationContext));
        }

        /// <inheritdoc />
        public IProxyFactory GetActorFactoryFrom(IFiber fiber)
        {
            return new ProxyFactory(new MonoDispatcherManager(fiber));
        }
    }
}
