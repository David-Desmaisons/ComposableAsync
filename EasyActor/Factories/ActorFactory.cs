using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Castle.DynamicProxy;

using EasyActor.Queue;
using EasyActor.Factories;
using System.Threading;

namespace EasyActor
{
    public class ActorFactory : ActorFactoryBase, IActorFactory
    {
        private Action<Thread> _OnCreate;

        public ActorFactory(Action<Thread> onCreate = null )
        {
            _OnCreate = onCreate;
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
            return Build<T>(concrete, new MonoThreadedQueue(_OnCreate));
        }

        public Task<T> BuildAsync<T>(Func<T> concrete) where T : class
        {
            var queue = new MonoThreadedQueue(_OnCreate);
            return queue.Enqueue( ()=> Build<T>(concrete(),queue) );
        }

        internal async Task<Tuple<T, MonoThreadedQueue>> InternalBuildAsync<T>(Func<T> concrete) where T : class
        {
            var queue = new MonoThreadedQueue(_OnCreate);
            var actor = await queue.Enqueue(() => Build<T>(concrete(), queue));
            return new Tuple<T, MonoThreadedQueue>(actor, queue);
        }
      
    }
}
