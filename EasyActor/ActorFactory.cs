using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Castle.DynamicProxy;

namespace EasyActor
{
    public class ActorFactory
    {
        private ProxyGenerator _Generator;
        private Priority _Priority;
        public ActorFactory(Priority iPriority= Priority.Normal)
        {
            _Priority = iPriority;
            _Generator = new ProxyGenerator();
        }

        public T Build<T>(T concrete, Nullable<Priority> iPriority=null) where T:class
        {
            return _Generator.CreateInterfaceProxyWithTargetInterface<T>(concrete, new IInterceptor[] { new DispatcherInterceptor(iPriority ?? _Priority) });
        }
    }
}
