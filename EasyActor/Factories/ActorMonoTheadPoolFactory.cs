using System;
using System.Threading.Tasks;
using EasyActor.Disposable;
using EasyActor.Fiber;
using EasyActor.Helper;
using EasyActor.Proxy;
using EasyActor.TaskHelper;

namespace EasyActor.Factories
{
    public abstract class ActorMonoTheadPoolFactory : ActorFactoryBase, IActorFactory
    {
        protected abstract IMonoThreadFiber GetMonoFiber();

        private T Build<T>(T concrete, IMonoThreadFiber fiber) where T : class
        {
            var asyncDisposable = concrete as IAsyncDisposable;
            return CreateIActorLifeCycle(concrete, fiber, TypeHelper.ActorCompleteLifeCycleType,
                        new ActorCompleteLifeCycleInterceptor(fiber, asyncDisposable),
                        new ActorLifeCycleInterceptor(fiber, asyncDisposable));
        }

        public T Build<T>(T concrete) where T : class
        {
            var cached = CheckInCache(concrete);
            return cached ?? Build<T>(concrete, GetMonoFiber());
        }

        public Task<T> BuildAsync<T>(Func<T> concrete) where T : class
        {
            var queue = GetMonoFiber();
            return queue.Enqueue(() => Build<T>(concrete(), queue));
        }

        internal async Task<Tuple<T, IMonoThreadFiber>> InternalBuildAsync<T>(Func<T> concrete) where T : class
        {
            var queue = GetMonoFiber();
            var actor = await queue.Enqueue(() => Build<T>(concrete(), queue)).ConfigureAwait(false);
            return new Tuple<T, IMonoThreadFiber>(actor, queue);
        }

        public void Dispose()
        {
        }

        public Task DisposeAsync() => TaskBuilder.Completed;
    }
}
