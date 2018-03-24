using System;
using System.Threading.Tasks;
using Concurrent;
using Concurrent.Disposable;

namespace EasyActor.Factories
{
    public abstract class ActorMonoTheadPoolFactory : ActorFactoryBase, IActorFactory
    {
        private readonly ComposableAsyncDisposable _ComposableAsyncDisposable = new ComposableAsyncDisposable();

        private IStopableFiber GetMonoFiber() => _ComposableAsyncDisposable.Add(ObtainMonoFiber());

        protected abstract IStopableFiber ObtainMonoFiber();

        public T Build<T>(T concrete) where T : class
        {
            var cached = CheckInCache(concrete);
            return cached ?? Create<T>(concrete, GetMonoFiber());
        }

        public Task<T> BuildAsync<T>(Func<T> concrete) where T : class
        {
            var fiber = GetMonoFiber();
            return fiber.Enqueue(() => Create<T>(concrete(), fiber));
        }

        public Task DisposeAsync() => _ComposableAsyncDisposable.DisposeAsync();
    }
}
