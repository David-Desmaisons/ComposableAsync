using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Castle.DynamicProxy;

using EasyActor.Queue;
using EasyActor.Factories;

namespace EasyActor
{
    public class ActorFactory : ActorFactoryBase, IActorFactory
    {
        private Priority _Priority;

        public ActorFactory(Priority priority = Priority.Normal)
        {
            _Priority = priority;
        }

        public T Build<T>(T concrete) where T : class
        {
            var queue = new MonoThreadedQueue(_Priority);
            return CreateIActorLifeCycle(concrete, queue, new ActorLifeCycleInterceptor(queue, concrete as IAsyncDisposable));
        }

        public ActorFactorType Type
        {
            get { return ActorFactorType.Standard; }
        }
    }
}
