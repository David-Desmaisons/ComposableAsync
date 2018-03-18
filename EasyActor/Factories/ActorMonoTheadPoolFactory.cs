using System;
using System.Threading.Tasks;
using Concurrent;
using Concurrent.Tasks;
using EasyActor.Helper;
using EasyActor.Proxy;

namespace EasyActor.Factories
{
    public abstract class ActorMonoTheadPoolFactory : ActorFactoryBase, IActorFactory
    {
        protected abstract IStopableFiber GetMonoFiber();

        private T Build<T>(T concrete, IStopableFiber fiber) where T : class
        {
            var asyncDisposable = concrete as IAsyncDisposable;
            return CreateIActorLifeCycle(concrete, fiber, TypeHelper.AsyncDisposable,
                        new DisposabeInterceptor(fiber, asyncDisposable));
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

        internal async Task<Tuple<T, IStopableFiber>> InternalBuildAsync<T>(Func<T> concrete) where T : class
        {
            var fiber = GetMonoFiber();
            var actor = await fiber.Enqueue(() => Build<T>(concrete(), fiber)).ConfigureAwait(false);
            return new Tuple<T, IStopableFiber>(actor, fiber);
        }

        public void Dispose()
        {
        }

        public Task DisposeAsync() => TaskBuilder.Completed;
    }
}
