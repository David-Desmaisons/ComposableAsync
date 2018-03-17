using System;
using System.Threading;
using System.Threading.Tasks;
using Concurrent;
using Concurrent.Tasks;
using EasyActor.Options;

namespace EasyActor.Factories
{
    public sealed class SynchronizationContextFactory : ActorFactoryBase, IActorFactory
    {
        private readonly IFiber _Fiber;

        public SynchronizationContextFactory() : this(SynchronizationContext.Current)
        {
        }

        public SynchronizationContextFactory(SynchronizationContext synchronizationContext)
        {
            if (synchronizationContext == null)
                throw new ArgumentNullException(nameof(synchronizationContext), "synchronizationContext can not be null");

            _Fiber = Fiber.GetFiberFromSynchronizationContext(synchronizationContext);
        }

        public override ActorFactorType Type => ActorFactorType.InCurrentContext;

        public T Build<T>(T concrete) where T : class
        {
            return Create(concrete, _Fiber);
        }

        public Task<T> BuildAsync<T>(Func<T> concrete) where T : class
        {
            return _Fiber.Enqueue(() => Build<T>(concrete()));
        }

        public void Dispose() { }

        public Task DisposeAsync() => TaskBuilder.Completed;
    }
}
