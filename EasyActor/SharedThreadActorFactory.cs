using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Concurrent;

using Castle.DynamicProxy;

using EasyActor.Factories;
using EasyActor.Queue;


namespace EasyActor
{
    public class SharedThreadActorFactory : ActorFactoryBase, IActorFactory, IActorLifeCycle
    {
        private Priority _Priority;
        private MonoThreadedQueue _Queue;
        private ConcurrentQueue<IAsyncDisposable> _Disposable;

        public SharedThreadActorFactory(Priority priority = Priority.Normal)
        {
            _Queue = new MonoThreadedQueue(priority);
            _Priority = priority;
            _Disposable = new ConcurrentQueue<IAsyncDisposable>();
        }

        public T Build<T>(T concrete) where T:class
        {
            var res = Create(concrete, _Queue);

            var disp = concrete as IAsyncDisposable;
            if (disp != null)
                _Disposable.Enqueue(disp);

            return res;
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
