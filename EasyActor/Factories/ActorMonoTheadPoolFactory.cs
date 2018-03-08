using System;
using System.Threading.Tasks;
using EasyActor.Fiber;
using EasyActor.Helper;
using EasyActor.Proxy;

namespace EasyActor.Factories
{
    public abstract class ActorMonoTheadPoolFactory : ActorFactoryBase, IActorFactory
    {
        protected abstract IMonoThreadFiber GetFiber();

        private T Build<T>(T concrete, IMonoThreadFiber fiber) where T : class
        {
            var asyncDisposable = concrete as IAsyncDisposable;
            return CreateIActorLifeCycle(concrete, fiber, TypeHelper.ActorCompleteLifeCycleType,
                        new ActorCompleteLifeCycleInterceptor(fiber, asyncDisposable),
                        new ActorLifeCycleInterceptor(fiber, asyncDisposable));
        }

        public T Build<T>(T concrete) where T : class
        {
            return Build<T>(concrete, GetFiber());
        }

        public Task<T> BuildAsync<T>(Func<T> concrete) where T : class
        {
            var queue = GetFiber();
            return queue.Enqueue(() => Build<T>(concrete(), queue));
        }

        internal async Task<Tuple<T, IMonoThreadFiber>> InternalBuildAsync<T>(Func<T> concrete) where T : class
        {
            var queue = GetFiber();
            var actor = await queue.Enqueue(() => Build<T>(concrete(), queue)).ConfigureAwait(false);
            return new Tuple<T, IMonoThreadFiber>(actor, queue);
        }
    }
}
