using System;
using System.Threading;
using System.Threading.Tasks;
using ComposableAsync.Concurrent;

namespace ComposableAsync.Actors.DispatcherManagers
{
    internal sealed class SynchronizationContextFiberManager : IDispatcherManager
    {
        public bool DisposeDispatcher => false;
        private readonly IFiber _Fiber;

        public SynchronizationContextFiberManager() : this(SynchronizationContext.Current)
        {
        }

        public SynchronizationContextFiberManager(SynchronizationContext synchronizationContext)
        {
            if (synchronizationContext == null)
                throw new ArgumentNullException(nameof(synchronizationContext), "synchronizationContext can not be null");
            _Fiber = Fiber.GetFiberFromSynchronizationContext(synchronizationContext);
        }

        public IDispatcher GetDispatcher() => _Fiber;

        public Task DisposeAsync() => Task.CompletedTask;
    }
}
