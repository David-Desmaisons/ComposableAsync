using Castle.DynamicProxy;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace EasyActor
{
    public class SynchronizationContextFactory : IActorFactory
    {
        private ProxyGenerator _Generator;
        private SynchronizationContextQueue _Context;

        public SynchronizationContextFactory():
            this(SynchronizationContext.Current)
        {
        }

        public SynchronizationContextFactory(SynchronizationContext synchronizationContext)
        {
            if (synchronizationContext == null)
                throw new ArgumentNullException("synchronizationContext can not be null");

            _Context = new SynchronizationContextQueue(synchronizationContext);
            _Generator = new ProxyGenerator();
        }

        public T Build<T>(T concrete) where T : class
        {
            return _Generator.CreateInterfaceProxyWithTargetInterface<T>(concrete, new IInterceptor[] { new QueueDispatcherInterceptor(_Context) });   
        }
    }
}
