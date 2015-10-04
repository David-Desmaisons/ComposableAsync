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
        private AsyncQueueMonoThreadDispatcher _Queue;

        public ActorFactory(bool SharedThread=false, Priority priority= Priority.Normal)
        {
            if (SharedThread)
            {
                _Queue = new AsyncQueueMonoThreadDispatcher(priority);
            }
            _Priority = priority;
            _Generator = new ProxyGenerator();
        }

        public T Build<T>(T concrete) where T:class
        {
            var queue = _Queue ?? new AsyncQueueMonoThreadDispatcher(_Priority);
            return _Generator.CreateInterfaceProxyWithTargetInterface<T>(concrete, new IInterceptor[] { new DispatcherInterceptor(queue) });
        }
    }
}
