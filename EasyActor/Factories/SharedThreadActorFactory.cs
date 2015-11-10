using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Concurrent;

using Castle.DynamicProxy;

using EasyActor.Factories;
using EasyActor.Queue;
using System.Threading;


namespace EasyActor
{
    public class SharedThreadActorFactory : ActorFactoryBase, IActorFactory, IActorLifeCycle
    {
        private MonoThreadedQueue _Queue;
        private ConcurrentQueue<IAsyncDisposable> _Disposable;

        public SharedThreadActorFactory(Action<Thread> onCreated = null)
        {
            _Queue = new MonoThreadedQueue(onCreated);
            _Disposable = new ConcurrentQueue<IAsyncDisposable>();
        }

        public ActorFactorType Type
        {
            get { return ActorFactorType.Shared; }
        }

        public T Build<T>(T concrete) where T:class
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

        private Task GetEndTask()
        {
            return  _Queue.SetCleanUp(async () =>
                {
                    IAsyncDisposable actordisp = null;
                    while (_Disposable.TryDequeue(out actordisp))
                    {
                        await actordisp.DisposeAsync();
                    }
                });
        }

        public Task Abort()
        {
            var res = GetEndTask();
            _Queue.Dispose();
            return res;
        }

        public Task Stop()
        {
            var res = GetEndTask();
            _Queue.Stop();
            return res;
        }

    }
}
