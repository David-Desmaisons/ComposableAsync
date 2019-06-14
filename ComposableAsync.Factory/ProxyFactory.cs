using Castle.DynamicProxy;
using ComposableAsync.Factory.Proxy;
using System;
using System.Threading.Tasks;

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
        /// Returns an proxy factory using the provided dispatcher
        /// Disposing the created factory will dispose the provided dispatcher
        /// </summary>
        /// <param name="dispatcher"></param>
        public ProxyFactory(IDispatcher dispatcher) :
            this(new MonoDispatcherManager(dispatcher, true))
        {
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

        private T Create<T>(T concrete, IDispatcher dispatcher) where T : class
        {
            var interceptors = new IInterceptor[] { new DispatcherInterceptor<T>(dispatcher) };
            var options = GetProxyGenerationOptions(dispatcher);
            return _Generator.CreateInterfaceProxyWithTarget<T>(concrete, options, interceptors);
        }

        private ProxyGenerationOptions GetProxyGenerationOptions(IDispatcher dispatcher)
        {
            var options = new ProxyGenerationOptions();
            options.AddMixinInstance(new DispatcherProvider(dispatcher));
            return options;
        }

        private IDispatcher GetDispatcher()
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
