using System;
using System.Threading.Tasks;
using Concurrent;
using Concurrent.Tasks;

namespace EasyActor.Factories
{
    public abstract class ActorMonoTheadPoolFactory : ActorFactoryBase, IActorFactory
    {
        protected abstract IStopableFiber GetMonoFiber();

        private T PrivateBuild<T>(T concrete, IStopableFiber fiber) where T : class
        {
            return CreateDisposable(concrete, fiber);
        }

        public T Build<T>(T concrete) where T : class
        {
            var cached = CheckInCache(concrete);
            return cached ?? PrivateBuild<T>(concrete, GetMonoFiber());
        }

        public Task<T> BuildAsync<T>(Func<T> concrete) where T : class
        {
            var queue = GetMonoFiber();
            return queue.Enqueue(() => PrivateBuild<T>(concrete(), queue));
        }

        public void Dispose()
        {
        }

        public Task DisposeAsync() => TaskBuilder.Completed;
    }
}
