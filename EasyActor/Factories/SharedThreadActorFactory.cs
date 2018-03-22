using System;
using System.Threading;
using System.Threading.Tasks;
using Concurrent;
using EasyActor.Options;

namespace EasyActor.Factories 
{
    public sealed class SharedThreadActorFactory : ActorFactoryBase, IActorFactory
    {
        private readonly IStopableFiber _Fiber;

        public SharedThreadActorFactory(Action<Thread> onCreated = null)
        {
            _Fiber = Fiber.CreateMonoThreadedFiber(onCreated);
        }

        public override ActorFactorType Type => ActorFactorType.Shared;

        private T PrivateBuild<T>(T concrete) where T : class
        {
            return Create(concrete, _Fiber);
        }

        public T Build<T>(T concrete) where T : class
        {
            var cached = CheckInCache(concrete);
            return cached ?? PrivateBuild(concrete);
        }

        public Task<T> BuildAsync<T>(Func<T> concrete) where T : class
        {
            return _Fiber.Enqueue(() => PrivateBuild<T>(concrete()));
        }

        public Task DisposeAsync() => _Fiber.DisposeAsync();
    }
}
