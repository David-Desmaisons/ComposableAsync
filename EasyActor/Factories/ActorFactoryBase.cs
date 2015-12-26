using Castle.DynamicProxy;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace EasyActor.Factories
{
    public class ActorFactoryBase
    {      
        private static readonly ProxyGenerator _Generator;
        private static readonly Dictionary<object, TaskScheduler> _SynchronizationContext = new Dictionary<object, TaskScheduler>();

        internal static ProxyGenerator Generator {get {return _Generator;}}

        static ActorFactoryBase()
        {
            _Generator = new ProxyGenerator();
        }

        public static TaskScheduler GetContextFromProxy(object Proxy)
        {
            TaskScheduler res = null;
            _SynchronizationContext.TryGetValue(Proxy, out res);
            return res;
        }

        private void Register<T>(T registered, ITaskQueue queue)
        {
            _SynchronizationContext.Add(registered, queue.TaskScheduler);
        }

        protected  T Create<T>(T concrete, ITaskQueue queue) where T : class
        {
            var interceptors = new IInterceptor[] { new QueueDispatcherInterceptor(queue) };
            Register(concrete, queue);
            return _Generator.CreateInterfaceProxyWithTargetInterface<T>(concrete, interceptors);
        }

        protected  T CreateIActorLifeCycle<T>(T concrete, ITaskQueue queue, Type addicionalType,  params IInterceptor[] interceptor) where T : class
        {
            var interceptors = new List<IInterceptor>(interceptor);
            interceptors.Add(new QueueDispatcherInterceptor(queue)); 
            Register(concrete, queue);
            return (T)_Generator.CreateInterfaceProxyWithTargetInterface(typeof(T), new Type[] {addicionalType }, concrete, interceptors.ToArray());
        }
    }
}
