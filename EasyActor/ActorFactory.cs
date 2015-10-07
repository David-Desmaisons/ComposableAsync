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
        private MonoThreadedQueue _Queue;

        public ActorFactory(bool SharedThread=false, Priority priority= Priority.Normal)
        {
            _Queue =  SharedThread ? new MonoThreadedQueue(priority) : null;
            _Priority = priority;
            _Generator = new ProxyGenerator();
        }

        public T Build<T>(T concrete) where T:class
        {
            if (_Queue==null)
            {
                var queue = new MonoThreadedQueue(_Priority);
                var interceptors = new IInterceptor[] { new ActorLifeCycleInterceptor(queue), new DispatcherInterceptor(queue) };
                return (T)_Generator.CreateInterfaceProxyWithTargetInterface(typeof(T), new Type[] { typeof(IActorLifeCycle) }, concrete, interceptors);
            }

            return _Generator.CreateInterfaceProxyWithTargetInterface<T>(concrete, new IInterceptor[] { new DispatcherInterceptor(_Queue) });
         }
    }
}
