using System;
using System.Threading;
using System.Threading.Tasks;
using EasyActor.Helper;
using EasyActor.Proxy;
using EasyActor.Queue;
using Retlang.Core;

namespace EasyActor.Factories
{
    public class ActorRetlangFactory : ActorFactoryBase, IActorFactory
    {
        private readonly Action<Thread> _OnCreate;

        public ActorRetlangFactory(Action<Thread> onCreate = null)
        {
            _OnCreate = onCreate;
        }

        public override ActorFactorType Type => ActorFactorType.Standard;

        private T Build<T>(T concrete, MonoThreadedRetlangQueue queue) where T : class
        {
            var asyncDisposable = concrete as IAsyncDisposable;
            return CreateIActorLifeCycle(concrete, queue, TypeHelper.ActorCompleteLifeCycleType,
                        new ActorCompleteLifeCycleInterceptor(queue, asyncDisposable));
        }

        public T Build<T>(T concrete) where T : class
        {
            return Build<T>(concrete, GetMonoThreadedRetlangQueue(_OnCreate));
        }

        public Task<T> BuildAsync<T>(Func<T> concrete) where T : class
        {
            var queue = GetMonoThreadedRetlangQueue(_OnCreate);
            return queue.Enqueue(() => Build<T>(concrete(), queue));
        }

        private static MonoThreadedRetlangQueue GetMonoThreadedRetlangQueue(Action<Thread> updateThread)
        {
            var queue = new BusyWaitQueue(new DefaultExecutor(), 100000, 30000);
            //var queue = new BoundedQueue(new DefaultExecutor()) { MaxDepth = 10000, MaxEnqueueWaitTime = 1000 };
            return new MonoThreadedRetlangQueue(queue, updateThread);
        }

        internal async Task<Tuple<T, MonoThreadedRetlangQueue>> InternalBuildAsync<T>(Func<T> concrete) where T : class
        {
            var queue = GetMonoThreadedRetlangQueue(_OnCreate);
            var actor = await queue.Enqueue(() => Build<T>(concrete(), queue)).ConfigureAwait(false);
            return new Tuple<T, MonoThreadedRetlangQueue>(actor, queue);
        }
    }
}
