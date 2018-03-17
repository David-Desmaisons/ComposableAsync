using Castle.DynamicProxy;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using Concurrent;
using EasyActor.Helper;
using EasyActor.Options;
using EasyActor.Proxy;

namespace EasyActor.Factories
{
    public abstract class ActorFactoryBase
    {
        private static readonly IDictionary<object, ActorDescription> _Actors = new ConcurrentDictionary<object, ActorDescription>();

        internal static ProxyGenerator Generator { get; }

        static ActorFactoryBase()
        {
            Generator = new ProxyGenerator();
        }

        internal static ActorDescription GetCachedActor(object raw)
        {
            ActorDescription res;
            _Actors.TryGetValue(raw, out res);
            return res;
        }

        public static SynchronizationContext GetContextFromProxy(object raw)
        {
            var res = GetCachedActor(raw);
            return res?.Fiber.SynchronizationContext;
        }

        public static void Clean(object raw)
        {
            _Actors.Remove(raw);
        }

        private void Register<T>(T registered, T proxyfied, IFiber fiber)
        {
            var actor = new ActorDescription(proxyfied, fiber, Type);
            _Actors.Add(registered, actor);
        }

        protected T CheckInCache<T>(T concrete) where T : class
        {
            var res = GetCachedActor(concrete);
            if (res == null)
                return null;

            if (res.Type != Type)
                throw new ArgumentException($"Instance already proxyfied using another factory: {res.Type}", nameof(concrete));

            return (res.ActorProxy) as T;
        }

        protected T Create<T>(T concrete, IFiber fiber) where T : class
        {
            var cached = CheckInCache(concrete);
            if (cached != null)
                return cached;

            var interceptors = new IInterceptor[] { new FiberDispatcherInterceptor<T>(fiber), new FiberProviderInterceptor(fiber) };
            var res = (T)Generator.CreateInterfaceProxyWithTargetInterface(typeof(T), new[] { TypeHelper.FiberProviderType }, concrete, interceptors);
            Register(concrete, res, fiber);
            return res;
        }

        protected T CreateIActorLifeCycle<T>(T concrete, IFiber fiber, Type addicionalType, params IInterceptor[] interceptor) where T : class
        {
            var interceptors = new List<IInterceptor> { new FiberDispatcherInterceptor<T>(fiber), new FiberProviderInterceptor(fiber) };
            interceptors.AddRange(interceptor);
            var res = (T)Generator.CreateInterfaceProxyWithTargetInterface(typeof(T), new[] { TypeHelper.FiberProviderType, addicionalType }, concrete, interceptors.ToArray());
            Register(concrete, res, fiber);
            return res;
        }

        protected T CreateIActorLifeCycle<T>(T concrete, Func<IFiber> fiberBuilder, Type addicionalType, params IInterceptor[] interceptor) where T : class
        {
            var cached = CheckInCache(concrete);
            if (cached != null)
                return cached;

            var fiber = fiberBuilder();
            var interceptors = new List<IInterceptor>(interceptor) { new FiberDispatcherInterceptor<T>(fiber), new FiberProviderInterceptor(fiber) };
            var res = (T)Generator.CreateInterfaceProxyWithTargetInterface(typeof(T), new[] { TypeHelper.FiberProviderType, addicionalType }, concrete, interceptors.ToArray());
            Register(concrete, res, fiber);
            return res;
        }

        public abstract ActorFactorType Type { get; }
    }
}
