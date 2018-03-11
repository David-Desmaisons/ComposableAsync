using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;
using EasyActor.Disposable;
using EasyActor.Fiber;

namespace EasyActor.Factories
{
    public sealed class SharedThreadActorFactory : ActorFactoryBase, IActorFactory, IActorCompleteLifeCycle
    {
        private readonly IAbortableFiber _Fiber;
        private readonly ConcurrentQueue<IAsyncDisposable> _Disposable;

        public SharedThreadActorFactory(Action<Thread> onCreated = null)
        {
            _Fiber = new MonoThreadedFiber(onCreated);
            _Disposable = new ConcurrentQueue<IAsyncDisposable>();
        }

        public override ActorFactorType Type => ActorFactorType.Shared;

        public T Build<T>(T concrete) where T : class
        {
            var res = Create(concrete, _Fiber);

            var disp = concrete as IAsyncDisposable;
            if (disp != null)
                _Disposable.Enqueue(disp);

            return res;
        }

        public Task<T> BuildAsync<T>(Func<T> concrete) where T : class
        {
            return _Fiber.Enqueue(() => Build<T>(concrete()));
        }

        private async Task GetEndTask()
        {
            IAsyncDisposable actordisp = null;
            while (_Disposable.TryDequeue(out actordisp))
            {
                await actordisp.DisposeAsync();
            }
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
