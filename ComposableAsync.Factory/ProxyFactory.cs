using System;
using System.Threading.Tasks;
using Castle.DynamicProxy;
using ComposableAsync.Factory.Proxy;

namespace ComposableAsync.Factory
{
    /// <summary>
    /// Proxy factory
    /// </summary>
    public sealed class ProxyFactory : IProxyFactory
    {
        private readonly IDispatcherManager _DispatcherManager;
        private readonly ComposableAsyncDisposable _ComposableAsyncDisposable = new ComposableAsyncDisposable();
        private readonly ProxyGenerator _Generator = new ProxyGenerator();

        /// <summary>
        /// Create a proxy builder using the provided <see cref="IDispatcherManager"/>
        /// </summary>
        /// <param name="dispatcherManager"></param>
        public ProxyFactory(IDispatcherManager dispatcherManager)
        {
            _DispatcherManager = _ComposableAsyncDisposable.Add(dispatcherManager);
        }

        /// <summary>
        /// Create the proxy
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="concrete"></param>
        /// <returns></returns>
        public T Build<T>(T concrete) where T : class
        {
            return Create<T>(concrete, GetDispatcher());
        }

        /// <summary>
        /// Creates the proxy using the dispatcher context
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="concrete"></param>
        /// <returns></returns>
        public Task<T> BuildAsync<T>(Func<T> concrete) where T : class
        {
            var fiber = GetDispatcher();
            return fiber.Enqueue(() => Create<T>(concrete(), fiber));
        }

        /// <summary>
        /// Dispose all the resources asynchronously
        /// </summary>
        /// <returns></returns>
        public Task DisposeAsync() => _ComposableAsyncDisposable.DisposeAsync();

        private T Create<T>(T concrete, ICancellableDispatcher dispatcher) where T : class
        {
            var interceptors = new IInterceptor[] { new DispatcherInterceptor<T>(dispatcher) };
            var options = GetProxyGenerationOptions(dispatcher);
            return _Generator.CreateInterfaceProxyWithTarget<T>(concrete, options, interceptors);
        }

        private ProxyGenerationOptions GetProxyGenerationOptions(ICancellableDispatcher dispatcher)
        {
            var options = new ProxyGenerationOptions();
            options.AddMixinInstance(new CancellableDispatcherProvider(dispatcher));
            return options;
        }

        private ICancellableDispatcher GetDispatcher()
        {
            var dispatcher = _DispatcherManager.GetDispatcher();
            if (_DispatcherManager.DisposeDispatcher)
            {
                _ComposableAsyncDisposable.Add(dispatcher as IAsyncDisposable);
            }
            return dispatcher;
        }
    }
}
