using System;
using System.Threading;
using System.Threading.Tasks;
using EasyActor.Fiber;
using EasyActor.Helper;
using EasyActor.Proxy;

namespace EasyActor.Factories
{
    public class ActorFactory : ActorFactoryBase, IActorFactory
    {
        private readonly Action<Thread> _OnCreate;

        public ActorFactory(Action<Thread> onCreate = null)
        {
            _OnCreate = onCreate;
        }

        public override ActorFactorType Type => ActorFactorType.Standard;

        private T Build<T>(T concrete, MonoThreadedFiber fiber) where T : class
        {
            var asyncDisposable = concrete as IAsyncDisposable;
            return CreateIActorLifeCycle(concrete, fiber, TypeHelper.ActorCompleteLifeCycleType,
                        new ActorCompleteLifeCycleInterceptor(fiber, asyncDisposable),
                        new ActorLifeCycleInterceptor(fiber, asyncDisposable));
        }

        public T Build<T>(T concrete) where T : class
        {
            return Build<T>(concrete, new MonoThreadedFiber(_OnCreate));
        }

        public Task<T> BuildAsync<T>(Func<T> concrete) where T : class
        {
            var queue = new MonoThreadedFiber(_OnCreate);
            return queue.Enqueue(() => Build<T>(concrete(), queue));
        }

        internal async Task<Tuple<T, MonoThreadedFiber>> InternalBuildAsync<T>(Func<T> concrete) where T : class
        {
            var queue = new MonoThreadedFiber(_OnCreate);
            var actor = await queue.Enqueue(() => Build<T>(concrete(), queue)).ConfigureAwait(false);
            return new Tuple<T, MonoThreadedFiber>(actor, queue);
        }
    }
}
