﻿using Castle.DynamicProxy;
using System;
using System.Runtime.CompilerServices;
using System.Threading;
using Concurrent;
using EasyActor.Helper;
using EasyActor.Options;
using EasyActor.Proxy;

namespace EasyActor.Factories
{
    public abstract class ActorFactoryBase
    {
        private static readonly ConditionalWeakTable<object, ActorDescription> _Actors = new ConditionalWeakTable<object, ActorDescription>();
        private static ProxyGenerator Generator { get; }
        private static readonly object _Locker = new object();

        public abstract ActorFactorType Type { get; }

        static ActorFactoryBase()
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
            return Register(concrete, res, fiber);
        }
    }
}
