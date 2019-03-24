using System;
using System.Threading.Tasks;
using Castle.DynamicProxy;
using Concurrent;
using Concurrent.Disposable;
using EasyActor.DispatcherManagers;
using EasyActor.Proxy;

namespace EasyActor.Factories
{
    /// <summary>
    /// Actor factory
    /// </summary>
    public sealed class ActorFactory : IActorFactory
    {
        private readonly IDispatcherManager _DispatcherManager;
        private readonly ComposableAsyncDisposable _ComposableAsyncDisposable = new ComposableAsyncDisposable();
        private readonly ProxyGenerator _Generator = new ProxyGenerator();

        public ActorFactory(IDispatcherManager dispatcherManager)
        {
            _DispatcherManager = _ComposableAsyncDisposable.Add(dispatcherManager);
        }

        public T Build<T>(T concrete) where T : class
        {
            return Create<T>(concrete, GetDispatcher());
        }

        public Task<T> BuildAsync<T>(Func<T> concrete) where T : class
        {
            var fiber = GetDispatcher();
            return fiber.Enqueue(() => Create<T>(concrete(), fiber));
        }

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
            if (!(dispatcher is IFiber fiber))
                return options;

            options.AddMixinInstance(new FiberProvider(fiber));
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
