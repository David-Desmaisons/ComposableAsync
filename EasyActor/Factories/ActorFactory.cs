using System;
using System.Threading.Tasks;
using EasyActor.Queue;
using EasyActor.Factories;
using System.Threading;
using EasyActor.Helper;

namespace EasyActor
{
    public class ActorFactory : ActorFactoryBase, IActorFactory
    {
        private readonly Action<Thread> _OnCreate;

        public ActorFactory(Action<Thread> onCreate = null )
        {
            _OnCreate = onCreate;
        }

        public override ActorFactorType Type => ActorFactorType.Standard;

        private T Build<T>(T concrete, IAbortableTaskQueue queue) where T : class
        {
            var asyncDisposable =  concrete as IAsyncDisposable;
            return CreateIActorLifeCycle(concrete, queue, TypeHelper.ActorCompleteLifeCycleType,
                        new ActorCompleteLifeCycleInterceptor(queue,asyncDisposable),  
                        new ActorLifeCycleInterceptor(queue, asyncDisposable));
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
            var actor = await queue.Enqueue(() => Build<T>(concrete(), queue)).ConfigureAwait(false);
            return new Tuple<T, MonoThreadedQueue>(actor, queue);
        }
    }
}
