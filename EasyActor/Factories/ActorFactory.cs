using System;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Castle.DynamicProxy;
using Concurrent;
using Concurrent.Disposable;
using EasyActor.DispatcherManangers;
using EasyActor.Options;
using EasyActor.Proxy;

namespace EasyActor.Factories
{
    public sealed class ActorFactory : IActorFactory
    {
        private static readonly ConditionalWeakTable<object, ActorDescription> _Actors = new ConditionalWeakTable<object, ActorDescription>();
        private static ProxyGenerator Generator { get; }
        private static readonly object _Locker = new object();

        public ActorFactorType Type => _DispatcherMananger.Type;

        private readonly IDispatcherMananger _DispatcherMananger;
        private readonly ComposableAsyncDisposable _ComposableAsyncDisposable = new ComposableAsyncDisposable();

        public ActorFactory(IDispatcherMananger dispatcherMananger)
        {
            _DispatcherMananger = _ComposableAsyncDisposable.Add(dispatcherMananger);
        }

        static ActorFactory()
        {
            Generator = new ProxyGenerator();
        }

        internal static ActorDescription GetCachedActor(object raw)
        {
            lock (_Locker)
            {
                ActorDescription res;
                _Actors.TryGetValue(raw, out res);
                return res;
            }
        }

        public static void Clean(object raw)
        {
            lock (_Locker)
            {
                _Actors.Remove(raw);
            }
        }

        private T Register<T>(T registered, T proxyfied, IDispatcher dispatcher)
        {
            lock (_Locker)
            {
                var actor = new ActorDescription(proxyfied, dispatcher, Type);
                _Actors.Add(registered, actor);
                return proxyfied;
            }
        }

        private T CheckInCache<T>(T concrete) where T : class
        {
            var res = GetCachedActor(concrete);
            if (res == null)
                return null;

            if (res.Type != Type)
                throw new ArgumentException($"Instance already proxyfied using another factory: {res.Type}", nameof(concrete));

            return (res.ActorProxy) as T;
        }

        private T Create<T>(T concrete, ICancellableDispatcher dispatcher) where T : class
        {
            var interceptors = new IInterceptor[] { new DispatcherInterceptor<T>(dispatcher) };
            var options = new ProxyGenerationOptions();
            var fiber = dispatcher as IFiber;
            if (fiber != null)
                options.AddMixinInstance(new FiberProvider(fiber));
            var res = Generator.CreateInterfaceProxyWithTarget<T>(concrete, options, interceptors);
            return Register(concrete, res, dispatcher);
        }

        private ICancellableDispatcher GetDispatcher()
        {
            var dispatcher = _DispatcherMananger.GetDispatcher();
            if (_DispatcherMananger.DisposeDispatcher)
            {
                _ComposableAsyncDisposable.Add(dispatcher as IAsyncDisposable);
            }
            return dispatcher;
        }

        public T Build<T>(T concrete) where T : class
        {
            var cached = CheckInCache(concrete);
            return cached ?? Create<T>(concrete, GetDispatcher());
        }

        public Task<T> BuildAsync<T>(Func<T> concrete) where T : class
        {
            var fiber = GetDispatcher();
            return fiber.Enqueue(() => Create<T>(concrete(), fiber));
        }

        public Task DisposeAsync() => _ComposableAsyncDisposable.DisposeAsync();
    }
}
