using System;
using System.Threading.Tasks;
using System.Collections.Concurrent;
using EasyActor.Factories;
using EasyActor.Queue;
using System.Threading;


namespace EasyActor
{
    public class SharedThreadActorFactory : ActorFactoryBase, IActorFactory, IActorCompleteLifeCycle
    {
        private readonly IAbortableTaskQueue _Queue;
        private readonly ConcurrentQueue<IAsyncDisposable> _Disposable;

        public SharedThreadActorFactory(Action<Thread> onCreated = null)
        {
            _Queue = new MonoThreadedQueue(onCreated);
            _Disposable = new ConcurrentQueue<IAsyncDisposable>();
        }

        public override ActorFactorType Type => ActorFactorType.Shared;

        public T Build<T>(T concrete) where T : class
        {
            var res = Create(concrete, _Queue);

            var disp = concrete as IAsyncDisposable;
            if (disp != null)
                _Disposable.Enqueue(disp);

            return res;
        }

        public Task<T> BuildAsync<T>(Func<T> concrete) where T : class
        {
            return _Queue.Enqueue(() => Build<T>(concrete()));
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
            return _Queue.Abort(GetEndTask);
        }

        public Task Stop()
        {
            return _Queue.Stop(GetEndTask);
        }
    }
}
