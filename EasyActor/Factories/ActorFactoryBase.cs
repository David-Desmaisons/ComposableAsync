using Castle.DynamicProxy;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace EasyActor.Factories
{
    public abstract class ActorFactoryBase
    {      
        private static readonly ProxyGenerator _Generator;
        private static readonly Dictionary<object, ActorDescription> _Actors = new Dictionary<object, ActorDescription>();

        internal static ProxyGenerator Generator => _Generator;

        static ActorFactoryBase()
        {
            _Generator = new ProxyGenerator();
        }

        private static ActorDescription GetChachedActor(object raw)
        {
            ActorDescription res = null;
            _Actors.TryGetValue(raw, out res);
            return res;
        }

        public static TaskScheduler GetContextFromProxy(object raw)
        {
            var res = GetChachedActor(raw);
            return res?.TaskScheduler;
        }

        public static void Clean(object raw)
        {
            _Actors.Remove(raw);
        }

        private void Register<T>(T registered, T proxyfied, ITaskQueue queue)
        {
            var actor = new ActorDescription(proxyfied, queue.TaskScheduler, Type);
            _Actors.Add(registered, actor);
        }

        private T CheckInCache<T>(T concrete) where T:class
        {
            var res = GetChachedActor(concrete);
            if (res == null)
                return null;

            if (res.Type != Type)
                throw new ArgumentException($"Instance already proxyfied using another factory: {res.Type}", nameof(concrete));

            return (res.ActorProxy) as T;
        }

        protected  T Create<T>(T concrete, ITaskQueue queue) where T : class
        {
            var cached = CheckInCache(concrete);
            if (cached != null)
                return cached;

            var interceptors = new IInterceptor[] { new QueueDispatcherInterceptor(queue) };
            var res = _Generator.CreateInterfaceProxyWithTargetInterface<T>(concrete, interceptors);
            Register(concrete, res, queue);
            return res;
        }

        protected  T CreateIActorLifeCycle<T>(T concrete, ITaskQueue queue, Type addicionalType,  params IInterceptor[] interceptor) where T : class
        {
            var cached = CheckInCache(concrete);
            if (cached != null)
                return cached;

            var interceptors = new List<IInterceptor>(interceptor) {new QueueDispatcherInterceptor(queue)};
            var res = (T)_Generator.CreateInterfaceProxyWithTargetInterface(typeof(T), new [] {addicionalType }, concrete, interceptors.ToArray());
            Register(concrete, res, queue);
            return res;
        }

        public abstract ActorFactorType Type
        {
            get;
        }
    }
}
