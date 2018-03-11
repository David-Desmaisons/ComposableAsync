using System;
using System.Threading.Tasks;
using EasyActor.Disposable;
using EasyActor.Fiber;
using EasyActor.Helper;
using EasyActor.Proxy;
using EasyActor.TaskHelper;

namespace EasyActor.Factories
{
    public sealed class TaskPoolActorFactory : ActorFactoryBase, IActorFactory
    {
        private IStopableFiber GetQueue() => TaskSchedulerFiber.GetFiber();

        public override ActorFactorType Type => ActorFactorType.TaskPool;

        private T Build<T>(T concrete, IStopableFiber queue) where T : class
        {
            var asyncDisposable = concrete as IAsyncDisposable;
            return CreateIActorLifeCycle(concrete, queue, TypeHelper.ActorLifeCycleType,
                        new ActorLifeCycleInterceptor(queue, asyncDisposable));
        }

        public T Build<T>(T concrete) where T : class
        {
            var cached = CheckInCache(concrete);
            return cached ?? Build<T>(concrete, GetQueue());
        }

        public Task<T> BuildAsync<T>(Func<T> concrete) where T : class
        {
            var queue = GetQueue();
            return queue.Enqueue(() => Build<T>(concrete(), queue));
        }

        public void Dispose() { }

        public Task DisposeAsync() => TaskBuilder.Completed;
    }
}
