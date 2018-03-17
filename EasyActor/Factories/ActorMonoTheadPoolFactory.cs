using System;
using System.Threading.Tasks;
using Concurrent;
using Concurrent.Tasks;
using EasyActor.Disposable;
using EasyActor.Helper;
using EasyActor.Proxy;

namespace EasyActor.Factories
{
    public abstract class ActorMonoTheadPoolFactory : ActorFactoryBase, IActorFactory
    {
        protected abstract IAbortableFiber GetMonoFiber();

        private T Build<T>(T concrete, IAbortableFiber fiber) where T : class
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

        internal async Task<Tuple<T, IAbortableFiber>> InternalBuildAsync<T>(Func<T> concrete) where T : class
        {
            var queue = GetMonoFiber();
            var actor = await queue.Enqueue(() => Build<T>(concrete(), queue)).ConfigureAwait(false);
            return new Tuple<T, IAbortableFiber>(actor, queue);
        }

        public void Dispose()
        {
        }

        public Task DisposeAsync() => TaskBuilder.Completed;
    }
}
