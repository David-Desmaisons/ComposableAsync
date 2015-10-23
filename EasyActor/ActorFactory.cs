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

        public ActorFactorType Type
        {
            get { return ActorFactorType.Standard; }
        }

        private T Build<T>(T concrete, MonoThreadedQueue queue) where T : class
        {
            return CreateIActorLifeCycle(concrete, queue, new ActorLifeCycleInterceptor(queue, concrete as IAsyncDisposable));
        }

        public T Build<T>(T concrete) where T : class
        {
            return Build<T>(concrete, new MonoThreadedQueue(_Priority));
        }

        public Task<T> BuildAsync<T>(Func<T> concrete) where T : class
        {
            var queue = new MonoThreadedQueue(_Priority);
            return queue.Enqueue( ()=> Build<T>(concrete(),queue) );
        }
      
    }
}
