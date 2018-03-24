using System;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Castle.DynamicProxy;
using Concurrent;
using Concurrent.Disposable;
using EasyActor.FiberManangers;
using EasyActor.Helper;
using EasyActor.Options;
using EasyActor.Proxy;

namespace EasyActor.Factories
{
    public sealed class ActorFactory : IActorFactory
    {
        private static readonly ConditionalWeakTable<object, ActorDescription> _Actors = new ConditionalWeakTable<object, ActorDescription>();
        private static ProxyGenerator Generator { get; }
        private static readonly object _Locker = new object();

        public ActorFactorType Type => _FiberMananger.Type;

        private readonly IFiberMananger _FiberMananger;
        private readonly ComposableAsyncDisposable _ComposableAsyncDisposable = new ComposableAsyncDisposable();

        public ActorFactory(IFiberMananger fiberMananger)
        {
            _FiberMananger = _ComposableAsyncDisposable.Add(fiberMananger);
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

        public static SynchronizationContext GetContextFromProxy(object raw)
        {
            var res = GetCachedActor(raw);
            return res?.Fiber.SynchronizationContext;
        }

        public static void Clean(object raw)
        {
            lock (_Locker)
            {
                _Actors.Remove(raw);
            }
        }

        private T Register<T>(T registered, T proxyfied, IFiber fiber)
        {
            lock (_Locker)
            {
                var actor = new ActorDescription(proxyfied, fiber, Type);
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

        private T Create<T>(T concrete, IFiber fiber) where T : class
        {
            var interceptors = new IInterceptor[] { new FiberDispatcherInterceptor<T>(fiber), new FiberProviderInterceptor(fiber) };
            var res = (T)Generator.CreateInterfaceProxyWithTargetInterface(typeof(T), new[] { TypeHelper.FiberProviderType }, concrete, interceptors);
            return Register(concrete, res, fiber);
        }

        private IFiber GetFiber()
        {
            var fiber = _FiberMananger.GetFiber();
            if (_FiberMananger.DisposeFiber)
            {
                _ComposableAsyncDisposable.Add(fiber as IAsyncDisposable);
            }
            return fiber;
        }

        public T Build<T>(T concrete) where T : class
        {
            var cached = CheckInCache(concrete);
            return cached ?? Create<T>(concrete, GetFiber());
        }

        public Task<T> BuildAsync<T>(Func<T> concrete) where T : class
        {
            var fiber = GetFiber();
            return fiber.Enqueue(() => Create<T>(concrete(), fiber));
        }

        public Task DisposeAsync() => _ComposableAsyncDisposable.DisposeAsync();
    }
}
