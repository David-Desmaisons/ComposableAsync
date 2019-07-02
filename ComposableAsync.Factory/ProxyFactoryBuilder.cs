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
        public IProxyFactory GetManagedProxyFactory(IDispatcher dispatcher)
        {
            return new ProxyFactory(new MonoDispatcherManager(dispatcher, true));
        }

        /// <inheritdoc />
        public IProxyFactory GetManagedProxyFactory(IDispatcher dispatcher1, IDispatcher dispatcher2)
        {
            var dispatcher = dispatcher1.Then(dispatcher2);
            return GetManagedProxyFactory(dispatcher);
        }

        /// <inheritdoc />
        public IProxyFactory GetManagedProxyFactory(params IDispatcher[] dispatchers)
        {
            var dispatcher = Build(dispatchers);
            return GetManagedProxyFactory(dispatcher);
        }

        /// <inheritdoc />
        public IProxyFactory GetUnmanagedProxyFactory(IDispatcher dispatcher)
        {
            return new ProxyFactory(new MonoDispatcherManager(dispatcher));
        }

        /// <inheritdoc />
        public IProxyFactory GetUnmanagedProxyFactory(IDispatcher dispatcher1, IDispatcher dispatcher2)
        {
            var dispatcher = dispatcher1.Then(dispatcher2);
            return GetUnmanagedProxyFactory(dispatcher);
        }

        /// <inheritdoc />
        public IProxyFactory GetUnmanagedProxyFactory(params IDispatcher[] dispatchers)
        {
            var dispatcher = Build(dispatchers);
            return GetUnmanagedProxyFactory(dispatcher);
        }

        private static IDispatcher Build(IDispatcher[] dispatchers)
        {
            if (dispatchers.Length == 0)
                throw new ArgumentException(nameof(dispatchers));

            return dispatchers[0].Then(dispatchers.Skip(1));
        }
    }
}
