using System;
using System.Threading;
using System.Threading.Tasks;
using Concurrent;
using EasyActor.Options;

namespace EasyActor.DispatcherManagers
{
    internal sealed class SynchronizationContextFiberManage : IDispatcherManager
    {
        public ActorFactorType Type => ActorFactorType.InCurrentContext;
        public bool DisposeDispatcher => false;
        private readonly IFiber _Fiber;

        public SynchronizationContextFiberManage() : this(SynchronizationContext.Current)
        {
        }

        public SynchronizationContextFiberManage(SynchronizationContext synchronizationContext)
        {
            if (synchronizationContext == null)
                throw new ArgumentNullException(nameof(synchronizationContext), "synchronizationContext can not be null");
            _Fiber = Fiber.GetFiberFromSynchronizationContext(synchronizationContext);
        }

        public ICancellableDispatcher GetDispatcher() => _Fiber;

        public Task DisposeAsync() => Task.CompletedTask;
    }
}
