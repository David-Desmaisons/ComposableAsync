using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Castle.DynamicProxy;
using EasyActor.Queue;
using System.Collections.Concurrent;

namespace EasyActor
{
    public class SharedThreadActorFactory : IActorFactory, IActorLifeCycle
    {
        private ProxyGenerator _Generator;
        private Priority _Priority;
        private MonoThreadedQueue _Queue;
        private ConcurrentQueue<IAsyncDisposable> _Disposable;

        public SharedThreadActorFactory(Priority priority = Priority.Normal)
        {
            _Queue = new MonoThreadedQueue(priority);
            _Priority = priority;
            _Generator = new ProxyGenerator();
            _Disposable = new ConcurrentQueue<IAsyncDisposable>();
        }

        public T Build<T>(T concrete) where T:class
        {
            var res = _Generator.CreateInterfaceProxyWithTargetInterface<T>(concrete, new IInterceptor[] { new DispatcherInterceptor(_Queue) });

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
