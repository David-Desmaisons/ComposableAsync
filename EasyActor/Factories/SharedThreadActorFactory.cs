using System;
using System.Threading;
using System.Threading.Tasks;
using EasyActor.Disposable;
using EasyActor.Fiber;

namespace EasyActor.Factories
{
    public sealed class SharedThreadActorFactory : ActorFactoryBase, IActorFactory, IActorCompleteLifeCycle
    {
        private readonly IAbortableFiber _Fiber;
        private readonly ComposableAsyncDisposable _Disposable;

        public SharedThreadActorFactory(Action<Thread> onCreated = null)
        {
            _Fiber = new MonoThreadedFiber(onCreated);
            _Disposable = new ComposableAsyncDisposable();
        }

        public override ActorFactorType Type => ActorFactorType.Shared;

        public T Build<T>(T concrete) where T : class
        {
            var res = Create(concrete, _Fiber);
            _Disposable.Add(concrete as IAsyncDisposable);
            return res;
        }

        public Task<T> BuildAsync<T>(Func<T> concrete) where T : class
        {
            return _Fiber.Enqueue(() => Build<T>(concrete()));
        }

        private Task GetEndTask()
        {
            return _Disposable.DisposeAsync();
        }

        public Task Abort()
        {
            return _Fiber.Abort(GetEndTask);
        }

        public Task Stop()
        {
            var stoppable = _Fiber as IStopableFiber;
            return stoppable?.Stop(GetEndTask) ?? _Fiber.Abort(GetEndTask);
        }

        public void Dispose() => Stop().Wait();

        public Task DisposeAsync() => Stop();
    }
}
