using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Castle.DynamicProxy;
using EasyActor.Queue;

namespace EasyActor
{
    public class SharedThreadActorFactory : IActorFactory
    {
        private ProxyGenerator _Generator;
        private Priority _Priority;
        private MonoThreadedQueue _Queue;

        public SharedThreadActorFactory(Priority priority = Priority.Normal)
        {
            _Queue = new MonoThreadedQueue(priority);
            _Priority = priority;
            _Generator = new ProxyGenerator();
        }

        public T Build<T>(T concrete) where T:class
        {
            return _Generator.CreateInterfaceProxyWithTargetInterface<T>(concrete, new IInterceptor[] { new DispatcherInterceptor(_Queue) });
        }
    }
}
