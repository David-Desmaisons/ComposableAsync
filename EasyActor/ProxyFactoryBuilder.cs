using Concurrent;
using EasyActor.DispatcherManagers;
using System;
using System.Linq;
using System.Threading;
using ComposableAsync;

namespace EasyActor
{
    /// <summary>
    /// Actor factory builder
    /// </summary>
    public class ProxyFactoryBuilder : IProxyFactoryBuilder
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

        /// <inheritdoc />
        public IProxyFactory GetManagedProxyFactory(ICancellableDispatcher dispatcher)
        {
            return new ProxyFactory(new MonoDispatcherManager(dispatcher, true));
        }

        /// <inheritdoc />
        public IProxyFactory GetManagedProxyFactory(ICancellableDispatcher dispatcher1, ICancellableDispatcher dispatcher2)
        {
            var dispatcher = dispatcher1.Then(dispatcher2);
            return GetManagedProxyFactory(dispatcher);
        }

        /// <inheritdoc />
        public IProxyFactory GetManagedProxyFactory(params ICancellableDispatcher[] dispatchers)
        {
            var dispatcher = Build(dispatchers);
            return GetManagedProxyFactory(dispatcher);
        }

        /// <inheritdoc />
        public IProxyFactory GetUnmanagedProxyFactory(ICancellableDispatcher dispatcher)
        {
            return new ProxyFactory(new MonoDispatcherManager(dispatcher));
        }

        /// <inheritdoc />
        public IProxyFactory GetUnmanagedProxyFactory(ICancellableDispatcher dispatcher1, ICancellableDispatcher dispatcher2)
        {
            var dispatcher = dispatcher1.Then(dispatcher2);
            return GetUnmanagedProxyFactory(dispatcher);
        }

        /// <inheritdoc />
        public IProxyFactory GetUnmanagedProxyFactory(params ICancellableDispatcher[] dispatchers)
        {
            var dispatcher = Build(dispatchers);
            return GetUnmanagedProxyFactory(dispatcher);
        }

        private static ICancellableDispatcher Build(ICancellableDispatcher[] dispatchers)
        {
            if (dispatchers.Length == 0)
                throw new ArgumentException(nameof(dispatchers));

            return dispatchers[0].Then(dispatchers.Skip(1));
        }
    }
}
