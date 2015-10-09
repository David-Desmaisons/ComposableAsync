using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Castle.DynamicProxy;
using EasyActor.Queue;

namespace EasyActor
{
    public class ActorFactory : IActorFactory
    {
        private ProxyGenerator _Generator;
        private Priority _Priority;

        public ActorFactory(Priority priority = Priority.Normal)
        {
            _Priority = priority;
            _Generator = new ProxyGenerator();
        }

        public T Build<T>(T concrete) where T : class
        {
            var queue = new MonoThreadedQueue(_Priority);
            var interceptors = new IInterceptor[] { new ActorLifeCycleInterceptor(queue, concrete as IAsyncDisposable), new DispatcherInterceptor(queue) };
            return (T)_Generator.CreateInterfaceProxyWithTargetInterface(typeof(T), new Type[] { typeof(IActorLifeCycle) }, concrete, interceptors);
        }
    }
}
