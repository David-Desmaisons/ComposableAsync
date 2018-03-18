using System;
using System.Threading.Tasks;
using Concurrent;
using Concurrent.Tasks;
using EasyActor.Helper;
using EasyActor.Options;
using EasyActor.Proxy;

namespace EasyActor.Factories
{
    public sealed class TaskPoolActorFactory : ActorFactoryBase, IActorFactory
    {
        private IStopableFiber GetFiber() => Fiber.GetTaskBasedFiber();

        public override ActorFactorType Type => ActorFactorType.TaskPool;

        private T Build<T>(T concrete, IStopableFiber fiber) where T : class
        {
            var asyncDisposable = concrete as IAsyncDisposable;
            return CreateIActorLifeCycle(concrete, fiber, TypeHelper.AsyncDisposable,
                        new DisposabeInterceptor(fiber, asyncDisposable));
        }

        public T Build<T>(T concrete) where T : class
        {
            var cached = CheckInCache(concrete);
            return cached ?? Build<T>(concrete, GetFiber());
        }

        public Task<T> BuildAsync<T>(Func<T> concrete) where T : class
        {
            var queue = GetFiber();
            return queue.Enqueue(() => Build<T>(concrete(), queue));
        }

        public void Dispose() { }

        public Task DisposeAsync() => TaskBuilder.Completed;
    }
}
