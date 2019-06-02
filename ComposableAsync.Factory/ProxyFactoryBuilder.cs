using System;
using System.Linq;

namespace ComposableAsync.Factory
{
    /// <summary>
    /// Actor factory builder
    /// </summary>
    public class ProxyFactoryBuilder : IProxyFactoryBuilder
    {
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
