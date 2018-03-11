using System;
using System.Threading;
using System.Threading.Tasks;
using EasyActor.Fiber;
using EasyActor.TaskHelper;

namespace EasyActor.Factories
{
    public class SynchronizationContextFactory : ActorFactoryBase, IActorFactory
    {
        private readonly SynchronizationContextFiber _Context;

        public SynchronizationContextFactory() : this(SynchronizationContext.Current)
        {
        }

        public SynchronizationContextFactory(SynchronizationContext synchronizationContext)
        {
            if (synchronizationContext == null)
                throw new ArgumentNullException(nameof(synchronizationContext), "synchronizationContext can not be null");

            _Context = new SynchronizationContextFiber(synchronizationContext);
        }

        public override ActorFactorType Type => ActorFactorType.InCurrentContext;

        public T Build<T>(T concrete) where T : class
        {
            return Create(concrete, _Context);
        }

        public Task<T> BuildAsync<T>(Func<T> concrete) where T : class
        {
            return _Context.Enqueue(() => Build<T>(concrete()));
        }

        public void Dispose() { }

        public Task DisposeAsync() => TaskBuilder.Completed;
    }
}
