using System;
using System.Threading;
using System.Threading.Tasks;
using Concurrent;
using Concurrent.Disposable;
using EasyActor.Helper;
using EasyActor.Options;
using EasyActor.Proxy;

namespace EasyActor.Factories
{
    public sealed class SharedThreadActorFactory : ActorFactoryBase, IActorFactory
    {
        private readonly IStopableFiber _Fiber;
        private readonly RefCountAsyncDisposable _RefCountAsyncDisposable;
        private readonly IAsyncDisposable _FiberReference;

        public SharedThreadActorFactory(Action<Thread> onCreated = null)
        {
            _Fiber = Fiber.CreateMonoThreadedFiber(onCreated);
            _RefCountAsyncDisposable = new RefCountAsyncDisposable(_Fiber);
            _FiberReference = _RefCountAsyncDisposable.GetDisposable();
        }

        public override ActorFactorType Type => ActorFactorType.Shared;

        public T Build<T>(T concrete) where T : class
        {
            var asyncDisposable = concrete as IAsyncDisposable;
            return CreateIActorLifeCycle(concrete, _Fiber, TypeHelper.AsyncDisposable,
                new DisposabeInterceptor(_RefCountAsyncDisposable.GetDisposable(), asyncDisposable));
        }

        public Task<T> BuildAsync<T>(Func<T> concrete) where T : class
        {
            return _Fiber.Enqueue(() => Build<T>(concrete()));
        }

        public void Dispose() => _FiberReference.Dispose();

        public Task DisposeAsync() => _FiberReference.DisposeAsync();
    }
}
